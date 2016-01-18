//-----------------------------------------------------------------------
// <copyright file="UserConnection.cs" company="Sully">
//     Copyright (c) Johnathon Sullinger. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MudDesigner.MudEngine.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using Commanding;
    using MudDesigner.MudEngine.Actors;
    using MudDesigner.MudEngine.MessageBrokering;

    /// <summary>
    /// Represents a connection to the server
    /// </summary>
    internal sealed class UserConnection : IConnection
    {
        /// <summary>
        /// The buffer size
        /// </summary>
        private readonly int bufferSize;

        /// <summary>
        /// The socket buffer
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// The outbound message subscription
        /// </summary>
        private ISubscription outboundMessage;

        /// <summary>
        /// The current data being processed
        /// </summary>
        private readonly List<string> currentData;

        /// <summary>
        /// The last chunk received from the socket
        /// </summary>
        private string lastChunk;

        /// <summary>
        /// The player that owns the socket on this connection
        /// </summary>
        private IPlayer player;

        /// <summary>
        /// The socket
        /// </summary>
        private Socket socket;

        private ICommandProcessedEventFactory commandProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConnection" /> class.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="currentConnection">The current connection.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        internal UserConnection(IPlayer player, Socket currentConnection, int bufferSize, ICommandProcessedEventFactory commandProcessor)
        {
            this.bufferSize = bufferSize;
            this.buffer = new byte[this.bufferSize];
            this.currentData = new List<string>();
            this.lastChunk = string.Empty;
            this.socket = currentConnection;
            this.Connection = socket;
            this.commandProcessor = commandProcessor;

            player.Deleting += this.DisconnectPlayer;
        }

        /// <summary>
        /// Occurs when a client is disconnected.
        /// </summary>
        public event EventHandler<ConnectionClosedArgs> Disconnected;

        /// <summary>
        /// Gets the connection socket.
        /// </summary>
        public Socket Connection { get; private set; }

        /// <summary>
        /// Determines whether this connection is still valid.
        /// </summary>
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

        /// <summary>
        /// Initializes the component.
        /// </summary>
        /// <returns>
        /// Returns an awaitable Task
        /// </returns>
        public Task Initialize()
        {
            this.buffer = new byte[this.bufferSize];
            this.socket.BeginReceive(this.buffer, 0, this.bufferSize, 0, new AsyncCallback(this.ReceiveData), null);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Lets this instance know that it is about to go out of scope and disposed.
        /// The instance will perform clean-up of its resources in preperation for deletion.
        /// </summary>
        /// <returns>
        /// Returns an awaitable Task
        /// </returns>
        /// <para>
        /// Informs the component that it is no longer needed, allowing it to perform clean up.
        /// Objects registered to one of the two delete events will be notified of the delete request.
        /// </para>
        public Task Delete() => this.player.Delete();

        /// <summary>
        /// Sends a message to the client
        /// </summary>
        /// <param name="message">The message content</param>
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

        /// <summary>
        /// Called when the socket has received data from the client.
        /// </summary>
        /// <param name="result">The result.</param>
        private void ReceiveData(IAsyncResult result)
        {
            if (!this.IsConnectionValid())
            {
                this.player.Delete();
                return;
            }

            int bytesRead = this.socket.EndReceive(result);
            this.socket.BeginReceive(this.buffer, 0, this.bufferSize, 0, new AsyncCallback(this.ReceiveData), null);
            if (bytesRead == 0 || this.buffer.Count() == 0)
            {
                return;
            }

            // TODO: Decode the bits into a string for parsing.
            string commandData = Encoding.Default.GetString(this.buffer, 0, bytesRead);
            if (commandData != "\n" || commandData != "\r\0" || commandData != "\r\n" || commandData != "\r")
            {
                return;
            }

            MessageBrokerFactory.Instance.Publish(new CommandRequestedMessage(commandData, this.player, this.commandProcessor));
        }

        /// <summary>
        /// Called when the socket has completed sending a message to the client.
        /// </summary>
        /// <param name="result">The result.</param>
        private void CompleteMessageSending(IAsyncResult result)
        {
            if (!this.IsConnectionValid())
            {
                return;
            }

            this.socket.EndSend(result);
        }

        /// <summary>
        /// Event handler to disconnect the socket when the palyer is being deleted.
        /// </summary>
        /// <param name="component">The component being deleted.</param>
        /// <returns>Returns an awaitable Task</returns>
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
