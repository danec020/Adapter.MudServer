using MudDesigner.MudEngine.Actors;
using MudDesigner.MudEngine.MessageBrokering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MudDesigner.MudEngine.Networking;

namespace MudDesigner.MudEngine.Networking
{
    internal sealed class UserConnection : IConnection
    {
        private readonly int bufferSize;

        private byte[] buffer;

        private ISubscription outboundMessage;

        private readonly List<string> currentData;

        private string lastChunk;

        private IPlayer player;

        private Socket socket;

		internal UserConnection(IPlayer player, Socket currentConnection, int bufferSize)
        {
            this.bufferSize = bufferSize;
            this.buffer = new byte[this.bufferSize];
            this.currentData = new List<string>();
            this.lastChunk = string.Empty;
            this.socket = currentConnection;
            this.Connection = socket;

            player.Deleting += this.DisconnectPlayer;
            //MessageBrokerFactory.Instance.Subscribe<CommandProcessedMessage>
            //    ((msg, sub) => this.SendMessage(msg.Content),
            //    msg => !string.IsNullOrEmpty(msg.Content) && msg.Target == player);
        }

        public event EventHandler<ConnectionClosedArgs> Disconnected;

        public Socket Connection { get; private set; }

        public bool IsConnectionValid()
        {
            if (this.socket == null || !this.socket.Connected)
            {
                return false;
            }


            bool pollWasSuccessful = this.socket.Poll(1000, SelectMode.SelectRead);
            bool noBytesReceived = this.socket.Available == 0;
            return !(pollWasSuccessful && noBytesReceived);
        }

        public Task Initialize()
        {
            this.buffer = new byte[this.bufferSize];
            this.socket.BeginReceive(this.buffer, 0, this.bufferSize, 0, new AsyncCallback(this.ReceiveData), null);

            return Task.FromResult(0);
        }

        public Task Delete() => this.player.Delete();

        public void SendMessage(string content)
        {
            if (!this.IsConnectionValid())
            {
                this.player.Delete();
                return;
            }
            else if (content == null)
            {
                return;
            }

            byte[] buffer = Encoding.ASCII.GetBytes(content);
            this.socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(this.CompleteMessageSending), content);
        }

        private void ReceiveData(IAsyncResult result)
        {
            if (!this.IsConnectionValid())
            {
                this.player.Delete();
                return;
            }

            int bytesRead = this.socket.EndReceive(result);
            if (bytesRead == 0 || this.buffer.Count() == 0)
            {
                return;
            }

            this.socket.BeginReceive(this.buffer, 0, this.bufferSize, 0, new AsyncCallback(this.ReceiveData), null);
        }

        private void CompleteMessageSending(IAsyncResult result)
        {
            if (!this.IsConnectionValid())
            {
                return;
            }

            this.socket.EndSend(result);
        }

        private Task DisconnectPlayer(IGameComponent component)
        {
            if (this.outboundMessage != null)
            {
                this.outboundMessage.Unsubscribe();
            }

            component.Deleting -= this.DisconnectPlayer;

            this.socket.Shutdown(SocketShutdown.Both);
            this.socket = null;

            var handler = this.Disconnected;
            if (handler == null)
            {
                return Task.FromResult(0);
            }

            handler(this, new ConnectionClosedArgs(this.player, this));
            return Task.FromResult(0);
        }
    }
}
