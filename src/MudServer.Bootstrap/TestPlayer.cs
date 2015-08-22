using MudDesigner.MudEngine.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudDesigner.MudEngine;

namespace MudDesigner.MudEngine.Networking
{
    public class TestPlayer : GameComponent, IPlayer
    {
        public ICharacterClass CharacterClass { get; private set; }


        public string Description { get; set; }

        public IGender Gender { get; private set; }

        public IRace Race { get; private set; }

        public void AddMountPoint(IMountPoint mountPoint)
        {
            throw new NotImplementedException();
        }

        public void AssignAbility(IAbility ability)
        {
            throw new NotImplementedException();
        }

        public IMountPoint FindMountPoint(string pointName)
        {
            throw new NotImplementedException();
        }

        public IAbility[] GetAbilities()
        {
            throw new NotImplementedException();
        }

        public IMountPoint[] GetMountPoints()
        {
            throw new NotImplementedException();
        }

        public void SetGender(IGender gender)
        {
            throw new NotImplementedException();
        }

        public void SetRace(IRace race)
        {
            throw new NotImplementedException();
        }

        protected override Task Load()
        {
            return Task.FromResult(0);
        }

        protected override Task Unload()
        {
            return Task.FromResult(0);
        }
    }
}
