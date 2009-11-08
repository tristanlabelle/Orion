
namespace Orion.Commandment
{
    public class SinglePlayerCommandPipeline : CommandPipeline
    {
        #region Fields
        private CommandDegugLogger logger = new CommandDegugLogger();
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
            get { return logger; }
        }
        #endregion
    }
}
