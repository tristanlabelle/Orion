using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Commandment
{
    public abstract class CommandPipeline
    {
		#region Fields
        private List<Commander> commanders = new List<Commander>();
        protected CommandExecutor executor = new CommandExecutor();
		#endregion

		#region Properties
        public abstract ISinkRecipient UserCommandmentEntryPoint { get; }
        public abstract ISinkRecipient AICommandmentEntryPoint { get; }
		#endregion

		#region Methods
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
		#endregion
    }
}
