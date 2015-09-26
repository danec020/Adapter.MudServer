using MudDesigner.MudEngine.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MudDesigner.MudEngine.Networking
{
    public class StandardServer : AdapterBase<IServerConfiguration>, IServer
    {
        private IGame game;

        private Dictionary<IPlayer, IConnection> playerConnections;

		private Dictionary<IPlayer, Socket> playerSockets;

        private IServerConfiguration configuration;

        private Socket serverSocket;

        private EngineTimer<IAdapter> clientTimeoutTimer;

        private IPlayerFactory playerFactory;

		private IConnectionFactory<StandardServer> connectionFactory;

		public StandardServer(IPlayerFactory playerFactory, IConnectionFactory<StandardServer> connectionFactory) : base()
        {
            this.playerFactory = playerFactory;
            this.connectionFactory = connectionFactory;
        }

		public StandardServer(
            IPlayerFactory playerFactory,
			IConnectionFactory<StandardServer> connectionFactory,
            IServerConfiguration serverConfiguration) : base(serverConfiguration)
        {
            this.playerFactory = playerFactory;
            this.connectionFactory = connectionFactory;
        }
			
		public override string Name => 
			string.Format("Mud Engine {0} Server Adapter", (int)System.Environment.OSVersion.Platform == 4 ? "Unix" : "Windows");

        public string Owner { get; set; }

        public ServerStatus Status { get; internal set; }

        public int RunningPort { get; private set; } = 5001;

        public override void Configure(IServerConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "You must provide a configuration to the server that is not null.");
            }

            this.configuration = configuration;
        }

        public override Task Initialize()
        {
            if (this.configuration == null)
            {
                throw new InvalidAdapterStateException(this, $"The {this.Name} adapter requires a valid {typeof(IServerConfiguration).Name} to be provided to it. Please provide one via the {nameof(this.Configure)}({typeof(IServerConfiguration).Name}) method.");
            }
            else if (this.Status != ServerStatus.Stopped)
            {
                throw new InvalidAdapterStateException(this, $"The {this.Name} adapter has already been initialized.");
            }
            else if (this.configuration.Port > 0)
            {
                this.RunningPort = this.configuration.Port;
            }

            this.playerConnections = new Dictionary<IPlayer, IConnection>();
			this.playerSockets = new Dictionary<IPlayer, Socket>();
            this.Status = ServerStatus.Stopped;
            this.clientTimeoutTimer = new EngineTimer<IAdapter>(this);

            return Task.FromResult(0);
        }

        public override Task Start(IGame game)
        {
            if (this.Status != ServerStatus.Stopped)
            {
                throw new InvalidAdapterStateException(this, $"The {this.Name} adapter requires the server to be stopped before you try and start it again.");
            }
            else if (game == null)
            {
                throw new InvalidAdapterStateException(this, $"The {this.Name} adapter require a valid {typeof(IGame).Name} to be provided to it.");
            }

            this.game = game;
            this.Status = ServerStatus.Starting;

            // Get our server address information
            var serverEndPoint = new IPEndPoint(IPAddress.Any, this.RunningPort);

            // Instance the server socket and bind it to a port.
            this.serverSocket = new Socket(serverEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.serverSocket.Bind(serverEndPoint);
            this.serverSocket.Listen(this.configuration.MaxQueuedConnections);

            if (this.RaiseOnStartup())
            {
                return Task.FromResult(0);
            }

            // Start a timer that periodically checks for clients that are no longer connected so we can clean them up.
            var staleConnectionPurgeInterval = TimeSpan.FromSeconds(5).TotalMilliseconds;
            this.clientTimeoutTimer.Start(staleConnectionPurgeInterval, staleConnectionPurgeInterval, 0, this.ReviewClientConnectionStates);

            // Begin listening for connection.
            this.serverSocket.BeginAccept(new AsyncCallback(this.ConnectClient), this.serverSocket);

            this.Status = ServerStatus.Running;
            return Task.FromResult(0);
        }

        public override Task Delete()
        {
            if (this.AdapterConfiguration.OnServerShutdown != null)
            {
                IServerContext context = new ServerContext(this, this.AdapterConfiguration, this.serverSocket);
                this.AdapterConfiguration.OnServerShutdown(context);
                if (context.IsHandled)
                {
                    return Task.FromResult(0);
                }
            }

            this.DisconnectAll();

            this.serverSocket.Blocking = false;
            if (this.serverSocket.Poll(100, SelectMode.SelectWrite))
            {
                this.serverSocket.Shutdown(SocketShutdown.Both);
            }

            this.Status = ServerStatus.Stopped;
            this.PublishMessage(new AdapterDeletedMessage(this));

            return Task.FromResult(0);
		}

		public override string ToString() => $"{this.game.Name} - Adapter: {this.Name}";

		internal Socket GetSocketForPlayer(IPlayer player)
		{
			if (!this.playerSockets.ContainsKey(player))
			{
				return null;
			}

			return this.playerSockets[player];
		}

        private void Disconnect(IPlayer player)
        {
            if (player == null)
            {
                return;
            }

            if (!this.playerConnections.ContainsKey(player))
            {
                return;
            }

            player.Delete();
        }

        private void DisconnectAll()
        {
            foreach (KeyValuePair<IPlayer, IConnection> pair in this.playerConnections)
            {
                IPlayer player = pair.Key;

                this.Disconnect(player);
            }

            this.playerConnections.Clear();
        }

        private void ConnectClient(IAsyncResult socketState)
        {
            Socket server = (Socket)socketState.AsyncState;
            Socket clientConnection = server.EndAccept(socketState);

            // Fetch the next incoming connection.
            server.BeginAccept(new AsyncCallback(this.ConnectClient), server);
            this.CreatePlayerConnection(clientConnection);
        }

        private void CreatePlayerConnection(Socket clientConnection)
        {
            // Initialize a new player.
            IPlayer player = this.playerFactory.CreatePlayer();
            player.Deleting += this.PlayerDeleting;

            player.Initialize().ContinueWith(task =>
            {
                this.PublishMessage(new PlayerCreatedMessage(player));

                // Add the player and it's connection to our collection of sockets
                this.playerSockets.Add(player, clientConnection);

                // Create the user connection instance and store it for the player.
                IConnection userConnection = this.connectionFactory.CreateConnection(player, this);
                this.playerConnections.Add(player, userConnection);

                userConnection.Initialize();
            });
        }

        private Task PlayerDeleting(IGameComponent component)
        {
            IPlayer player = component as IPlayer;
            if (player == null)
            {
                return Task.FromResult(0);
            }

            // Clean up the server references to the player
            this.playerSockets.Remove(player);
            this.playerConnections.Remove(player);

            // Remove our strong reference to the event and publish the deletion
            player.Deleting -= this.PlayerDeleting;
            this.PublishMessage(new PlayerDeletionMessage(player));

            return Task.FromResult(0);
        }

        private void ReviewClientConnectionStates(IAdapter adapter, EngineTimer<IAdapter> timer)
        {
            var connectedClients = this.playerConnections.ToArray();
            foreach (KeyValuePair<IPlayer, IConnection> pair in connectedClients)
            {
                IPlayer player = pair.Key;
                IConnection connection = pair.Value;
                if (connection.IsConnectionValid())
                {
                    continue;
                }

                this.PublishMessage(new InfoMessage("Player connection timed out."));
                this.Disconnect(player);
            }
        }

        private bool RaiseOnStartup()
        {
            var startupContext = new ServerContext(this, this.AdapterConfiguration, this.serverSocket);
            if (this.AdapterConfiguration.OnServerStartup != null)
            {
                this.AdapterConfiguration.OnServerStartup(startupContext);

                // Check and see if our context was replaced.
                if (startupContext.ListeningSocket != this.serverSocket)
                {
                    this.serverSocket = startupContext.ListeningSocket;
                }
            }

            return startupContext.IsHandled;
        }
    }
}
