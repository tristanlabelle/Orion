
namespace Orion.Commandment
{
    public class SinglePlayerCommandPipeline : CommandPipeline
    {
        #region Fields
        private CommandTextLogger logger = new CommandTextLogger();
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
