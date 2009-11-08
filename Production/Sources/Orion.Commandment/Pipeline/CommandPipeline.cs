using System.Collections.Generic;
using System.Text;
using System;

namespace Orion.Commandment.Pipeline
{
    /// <summary>
    /// Encapsulates the process by which commands go from their source to their destination.
    /// </summary>
    public abstract class CommandPipeline
    {
        #region Instance
        #region Fields
        private readonly List<Commander> commanders = new List<Commander>();
        protected CommandExecutor executor = new CommandExecutor();
        #endregion

        #region Properties
        public abstract ICommandSink UserCommandmentEntryPoint { get; }
        public abstract ICommandSink AICommandmentEntryPoint { get; }
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

        public virtual void Update(int frameNumber, float timeDeltaInSeconds)
        {
            UpdateCommanders(timeDeltaInSeconds);
        }

        public override string ToString()
        {
            return "User: {0}, AI: {1}".FormatInvariant(
                GetString(UserCommandmentEntryPoint),
                GetString(AICommandmentEntryPoint));
        }

        protected void UpdateCommanders(float timeDeltaInSeconds)
        {
            foreach (Commander commander in commanders)
                commander.Update(timeDeltaInSeconds);
        }
        #endregion
        #endregion

        #region Static
        #region Methods
        private static string GetString(ICommandSink sink)
        {
            Argument.EnsureNotNull(sink, "sink");

            StringBuilder stringBuilder = new StringBuilder();
            do
            {
                stringBuilder.Append("-->");
                stringBuilder.Append(sink.GetType().Name);
                CommandFilter filter = sink as CommandFilter;
                if (filter == null) break;
                sink = filter.Recipient;
            } while (sink != null);

            return stringBuilder.ToString();
        }
        #endregion
        #endregion
    }
}