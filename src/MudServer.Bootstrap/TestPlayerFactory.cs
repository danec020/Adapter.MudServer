using MudDesigner.MudEngine.Actors;

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
