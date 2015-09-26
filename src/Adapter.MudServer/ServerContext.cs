using System.Net.Sockets;

namespace MudDesigner.MudEngine.Networking
{
    public class ServerContext : IServerContext
    {
		private StandardServer windowsServer;

		internal ServerContext(StandardServer server, IServerConfiguration configuration, Socket serverSocket)
        {
            this.ListeningSocket = serverSocket;
            this.Server = server;
            this.windowsServer = server;
            this.Configuration = configuration;
        }

        public Socket ListeningSocket { get; set; }

        public bool IsHandled { get; set; }

        public IServer Server { get; private set; }

        public IServerConfiguration Configuration { get; private set; }

        public void SetServerState(ServerStatus status)
        {
            this.windowsServer.Status = status;
        }
    }
}
