//-----------------------------------------------------------------------
// <copyright file="Bootstrap.cs" company="Sully">
//     Copyright (c) Johnathon Sullinger. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MudDesigner.MudEngine.Networking
{
    using System;
    using MudDesigner.MudEngine.Game;

    /// <summary>
    /// Bootstraps the startup process of the game and server
    /// </summary>
    public class Bootstrap
    {
        /// <summary>
        /// Gets the game.
        /// </summary>
        public IGame Game { get; private set; }

        /// <summary>
        /// Gets the server running the game.
        /// </summary>
        public IServer Server { get; private set; }

        /// <summary>
        /// Initializes the server and game.
        /// </summary>
        /// <param name="startedCallback">The callback to invoke when initalization is completed.</param>
        public void Initialize(Action<IGame, StandardServer> startedCallback)
        {
            var serverConfig = new ServerConfiguration();
            var server = new StandardServer(new TestPlayerFactory(), new ConnectionFactory());
            server.Configure(serverConfig);
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
