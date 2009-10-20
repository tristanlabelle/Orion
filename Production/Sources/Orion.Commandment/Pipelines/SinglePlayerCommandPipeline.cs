using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Commandment
{
    public class SinglePlayerCommandPipeline : CommandPipeline
    {
        private CommandLogger logger = new CommandLogger();

        public SinglePlayerCommandPipeline()
        {
            logger.Recipient = executor;
        }

        public override ISinkRecipient UserCommandmentEntryPoint
        {
            get { return logger; }
        }

        public override ISinkRecipient AICommandmentEntryPoint
        {
            get { return executor; }
        }

        public override void Update(float frameDuration)
        {
            base.Update(frameDuration);
            //logger.Flush();
        }
    }
}
