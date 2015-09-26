using System.Net.Sockets;
using MudDesigner.MudEngine.Actors;

namespace MudDesigner.MudEngine.Networking
{
    public class ConnectionFactory : IConnectionFactory<StandardServer>
    {
		public IConnection CreateConnection(IPlayer player, StandardServer server)
        {
            Socket playerConnection = server.GetSocketForPlayer(player);
            return new UserConnection(player, playerConnection, server.AdapterConfiguration.PreferedBufferSize);
        }
    }
}
