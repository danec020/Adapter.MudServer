using System.Threading.Tasks;

namespace MudDesigner.MudEngine.Networking
{
    internal class ServerContentParser : AdapterBase
    {
        private bool started;

        public override string Name => "Server Content Parser";

        public override Task Initialize()
        {
            this.SubscribeToMessage<CommandProcessedMessage>(
                (message, subscription) => { },
                (message) => started);

            this.SubscribeToMessage<ClientMessageReceived>(
                (message, subscription) => { },
                (message) => started && !string.IsNullOrEmpty(message.Content.Data) && message.Content.Player != null && message.Content.Connection != null);

            return Task.FromResult(0);
        }

        public override Task Start(IGame game)
        {
            this.started = true;
            return Task.FromResult(0);
        }

        public override Task Delete()
        {
            this.UnsubscribeFromAllMessages();
            return Task.FromResult(0);
        }

        public override void Configure()
        {
        }
    }
}
