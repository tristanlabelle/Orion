using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Commandment
{
    public abstract class CommandPipeline
    {
        private List<Commander> commanders = new List<Commander>();
        protected CommandExecutor executor = new CommandExecutor();

        public abstract ISinkRecipient UserCommandmentEntryPoint { get; }
        public abstract ISinkRecipient AICommandmentEntryPoint { get; }

        public void AddCommander(Commander commander)
        {
            commanders.Add(commander);
        }

        public void RemoveCommander(Commander commander)
        {
            commanders.Remove(commander);
        }

        public virtual void Update(float frameDuration)
        {
            foreach (Commander commander in commanders)
            {
                commander.Update(frameDuration);
            }
        }
    }
}
