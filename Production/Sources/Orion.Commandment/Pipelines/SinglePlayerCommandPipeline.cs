using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Commandment
{
    public class SinglePlayerCommandPipeline : CommandPipeline
    {
        #region Fields
        private CommandLogger logger = new CommandLogger();
        #endregion

        #region Constructors
        public SinglePlayerCommandPipeline()
        {
            logger.Recipient = executor;
        }
        #endregion

        #region Properties
        public override ICommandSink UserCommandmentEntryPoint
        {
            get { return logger; }
        }

        public override ICommandSink AICommandmentEntryPoint
        {
            get { return executor; }
        }
        #endregion
    }
}
