//-----------------------------------------------------------------------
// <copyright file="MainClass.cs" company="Sully">
//     Copyright (c) Johnathon Sullinger. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MudDesigner.MudEngine.Networking
{
    using MudDesigner.MudEngine.Actors;
    using MudDesigner.MudEngine.MessageBrokering;
    using System;
    using System.Threading;

    class MainClass
    {
        public static void Main(string[] args)
        {
            SetupMessageBrokering();

            var bootstrap = new Bootstrap();
            bool startupCompleted = false;
            bootstrap.Initialize((game, server) =>
            {
                // Start the game loop.
                while (game.IsRunning)
                {
                    if (!startupCompleted)
                    {
                        startupCompleted = true;
                    }

                    Thread.Sleep(1);
                }
            });

            while (!startupCompleted)
            {
                Thread.Sleep(1);
            }
        }

        static void SetupMessageBrokering()
        {
            MessageBrokerFactory.Instance.Subscribe<InfoMessage>(
                (msg, subscription) => Console.WriteLine(msg.Content));

            MessageBrokerFactory.Instance.Subscribe<GameMessage>(
                (msg, subscription) => Console.WriteLine(msg.Content));

            MessageBrokerFactory.Instance.Subscribe<PlayerCreatedMessage>(
                (msg, sub) => Console.WriteLine("Player connected."));

            MessageBrokerFactory.Instance.Subscribe<PlayerDeletionMessage>(
                (msg, sub) =>
                {
                    Console.WriteLine("Player disconnected.");
                });
        }
    }
}
