using System.Collections.Generic;
using System.Text;
using System;
using Orion.GameLogic;

namespace Orion.Commandment.Pipeline
{
    /// <summary>
    /// Encapsulates the process by which commands go from their source to their destination.
    /// </summary>
    public sealed class CommandPipeline
    {
        #region Fields
        private readonly ICommandSink sink;
        private readonly List<CommandFilter> filters = new List<CommandFilter>();
        private readonly Dictionary<Commander, ICommandSink> commanders
            = new Dictionary<Commander, ICommandSink>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="CommandPipeline"/>
        /// from its final <see cref="ICommandSink"/>.
        /// </summary>
        /// <param name="sink">The final <see cref="ICommandSink"/> of this <see cref="CommandPipeline"/>.</param>
        public CommandPipeline(ICommandSink sink)
        {
            Argument.EnsureNotNull(sink, "sink");
            this.sink = sink;
        }

        public CommandPipeline(Match match)
            : this(new CommandExecutor(match)) { }
        #endregion

        #region Methods
        #region Filters
        /// <summary>
        /// Adds a <see cref="CommandFilter"/> to the start of this <see cref="CommandPipeline"/>,
        /// making it the first filter to process <see cref="Command"/>s.
        /// </summary>
        /// <param name="filter">A <see cref="CommandFilter"/> to be added.</param>
        public void PushFilter(CommandFilter filter)
        {
            Argument.EnsureNotNull(filter, "filter");
            filter.Flushed += OnFilterFlushed;
            filters.Insert(0, filter);
        }

        private void OnFilterFlushed(CommandFilter filter, Command command)
        {
            int index = filters.IndexOf(filter);
            if (index == filters.Count - 1) sink.Handle(command);
            else filters[index + 1].Handle(command);
        }
        #endregion

        #region Commanders
        /// <summary>
        /// Adds a <see cref="Commander"/> to this <see cref="CommandPipeline"/>,
        /// specifying the <see cref="ICommandSink"/> it forwards commands to.
        /// </summary>
        /// <param name="commander">A <see cref="Commander"/> to be added.</param>
        /// <param name="sink">
        /// A <see cref="ICommandSink"/> part of this <see cref="CommandPipeline"/>
        /// to which the <see cref="Commander"/> should forward its commands.
        /// </param>
        public void AddCommander(Commander commander, ICommandSink sink)
        {
            Argument.EnsureNotNull(commander, "commander");
            Argument.EnsureNotNull(sink, "sink");
            commanders.Add(commander, sink);
            commander.CommandGenerated += OnCommandGenerated;
        }

        /// <summary>
        /// Adds a <see cref="Commander"/> to this <see cref="CommandPipeline"/>,
        /// assuming the first <see cref="ICommandSink"/> as its sink.
        /// </summary>
        /// <param name="commander">A <see cref="Commander"/> to be added.</param>
        public void AddCommander(Commander commander)
        {
            if (filters.Count == 0) AddCommander(commander, sink);
            else AddCommander(commander, filters[0]);
        }

        private void OnCommandGenerated(Commander commander, Command command)
        {
            Argument.EnsureNotNull(commander, "commander");
            Argument.EnsureNotNull(command, "command");

            commanders[commander].Handle(command);
        }
        #endregion

        public void Update(int frameNumber, float timeDeltaInSeconds)
        {
            foreach (Commander commander in commanders.Keys)
                commander.Update(timeDeltaInSeconds);

            foreach (CommandFilter filter in filters)
                filter.Update(frameNumber, timeDeltaInSeconds);

            sink.Update(frameNumber, timeDeltaInSeconds);
        }
        #endregion
    }
}