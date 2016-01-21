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
        private enum LoginState { Initializing, RequestingAccount, RequestingPassword, RequestingHelp, Finished };
        private LoginState currentState = LoginState.Initializing;

        public Task<bool> CanProcessCommand(IPlayer source, string command, params string[] arguments)
        {
            return Task.Factory.StartNew(() =>
            {
                return currentState != LoginState.Finished;
            });
        }

        public Task<CommandResult> ProcessCommand(IPlayer source, string command, params string[] arguments)
        {
            return Task.Factory.StartNew(() =>
            {
                switch (currentState)
                {
                    case LoginState.Initializing:
                        currentState = LoginState.RequestingAccount;
                        return new CommandResult(false, "Please enter your Account Name and press [ENTER].");
                    case LoginState.RequestingAccount:
                        return ProcessAccountName(source, arguments);
                    case LoginState.RequestingPassword:
                        return ProcessAccountPassword(source, arguments);
                    case LoginState.Finished:
                        return new CommandResult(true);
                    case LoginState.RequestingHelp:
                    default:
                        return new CommandResult(false, "Login is currently in an unknown state!");
                }
            });
        }

        private CommandResult ProcessAccountPassword(IPlayer source, string[] arguments)
        {
            currentState = LoginState.Finished;
            return new CommandResult(true, "You have successfully logged in.");
        }

        private CommandResult ProcessAccountName(IPlayer source, string[] arguments)
        {
            currentState = LoginState.RequestingPassword;
            return new CommandResult(false, "Now enter your password and hit [ENTER].");
        }
    }
}
