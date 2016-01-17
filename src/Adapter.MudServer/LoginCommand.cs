using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudDesigner.MudEngine.Actors;
using MudDesigner.MudEngine.Commanding;

namespace MudDesigner.MudEngine.Networking
{
    public class LoginCommand : IActorCommand
    {
        public bool IsCompleted
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Task<bool> CanProcessCommand(IActor source, params string[] arguments)
        {
            throw new NotImplementedException();
        }

        public Task ProcessCommand(IActor source, params string[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
