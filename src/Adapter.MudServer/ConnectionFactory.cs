using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
