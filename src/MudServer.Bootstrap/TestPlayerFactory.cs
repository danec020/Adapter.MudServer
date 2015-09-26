//-----------------------------------------------------------------------
// <copyright file="TestPlayerFactory.cs" company="Sully">
//     Copyright (c) Johnathon Sullinger. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MudDesigner.MudEngine.Networking
{
    using MudDesigner.MudEngine.Actors;

    public class TestPlayerFactory : IPlayerFactory
    {
        public IPlayer CreatePlayer()
        {
            var player = new TestPlayer();
            return player;
        }
    }
}
