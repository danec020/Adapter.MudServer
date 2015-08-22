using MudDesigner.MudEngine.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudDesigner.MudEngine.Networking
{
    public class TestPlayerFactory : IPlayerFactory
    {
        public IPlayer CreatePlayer()
        {
            var player = new TestPlayer();
            return player;
        }
    }
}
