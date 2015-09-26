using System;
using System.Collections.Generic;

namespace MudDesigner.MudEngine.Networking
{
    public class ServerConfiguration : IServerConfiguration
    {
        private List<IAdapter> adapters;

        public ServerConfiguration() : this(16, 6, 512)
        {
        }

        public ServerConfiguration(int maxPasswordSize, int minPasswordSize, int bufferSize)
        {
            this.adapters = new List<IAdapter>();

            this.MaximumPasswordSize = maxPasswordSize;
            this.MinimumPasswordSize = minPasswordSize;
            this.PreferedBufferSize = bufferSize;
        }

		public Action<IServerContext> OnServerStartup { get; set; }

		public Action<IServerContext> OnServerShutdown { get; set; }

        public string ConnectedMessage { get; set; }

        public int MaximumPasswordSize { get; private set; }

        public int MaxQueuedConnections { get; set; }

        public string[] MessageOfTheDay { get; set; }

        public int MinimumPasswordSize { get; private set; }

        public int Port { get; set; }

        public int PreferedBufferSize { get; private set; }

        public IAdapter[] GetAdapters()
        {
            return this.adapters.ToArray();
        }

        public void UseAdapters(IEnumerable<IAdapter> adapters)
        {
            foreach(IAdapter adapter in adapters)
            {
                this.UseAdapter(adapter);
            }
        }

        public void UseAdapter<TAdapter>() where TAdapter : class, IAdapter, new()
        {
            this.adapters.Add(new TAdapter());
        }

        public void UseAdapter<TAdapter>(TAdapter component) where TAdapter : class, IAdapter
        {
            this.adapters.Add(component);
        }
    }
}
