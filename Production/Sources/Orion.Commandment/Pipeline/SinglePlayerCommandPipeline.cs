
using Orion.GameLogic;
namespace Orion.Commandment.Pipeline
{
    public sealed class SinglePlayerCommandPipeline : CommandPipeline
    {
        #region Fields
        private readonly CommandTextLogger textLogger = new CommandTextLogger();
        private readonly CommandReplayLogger replayLogger;
        #endregion

        #region Constructors
        public SinglePlayerCommandPipeline(World world)
        {
            replayLogger = new CommandReplayLogger("replay.foo", world);
            textLogger.Recipient = replayLogger;
            replayLogger.Recipient = executor;
        }
        #endregion

        #region Properties
        public override ICommandSink UserCommandmentEntryPoint
        {
            get { return textLogger; }
        }

        public override ICommandSink AICommandmentEntryPoint
        {
            get { return textLogger; }
        }
        #endregion
    }
}
