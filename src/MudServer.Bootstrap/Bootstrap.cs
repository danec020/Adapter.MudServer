using System;
using MudDesigner.MudEngine.Game;

namespace MudDesigner.MudEngine.Networking
{
    public class Bootstrap
    {
		public IGame Game { get; private set; }

		public IServer Server { get; private set; }

		public void Initialize(Action<IGame, StandardServer> startedCallback)
        {
            var serverConfig = new ServerConfiguration();
            var server = new StandardServer(new TestPlayerFactory(), new ConnectionFactory(), serverConfig);
            server.Owner = "@Scionwest";
			this.Server = server;

            var gameConfig = new GameConfiguration();
            gameConfig.UseAdapter(server);

            var game = new MudGame();
            game.Configure(gameConfig);
			this.Game = game;

            game.BeginStart(runningGame => startedCallback(runningGame, server));
        }
    }
}
