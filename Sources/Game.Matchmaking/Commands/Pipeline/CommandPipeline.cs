using System.Collections.Generic;
using System.Text;
using System;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking.Networking;

namespace Orion.Game.Matchmaking.Commands.Pipeline
{
    /// <summary>
    /// Encapsulates the process by which commands go from their source to their destination.
    /// </summary>
    public sealed class CommandPipeline : IDisposable
    {
        #region Fields
        private readonly Match match;
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
        /// <param name="match">The match in which the commands are executed.</param>
        /// <param name="sink">The final <see cref="ICommandSink"/> of this <see cref="CommandPipeline"/>.</param>
        public CommandPipeline(Match match, ICommandSink sink)
        {
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(sink, "sink");

            this.match = match;
            this.sink = sink;
        }

        public CommandPipeline(Match match)
            : this(match, new CommandExecutor(match)) { }
        #endregion

        #region Properties
        public Match Match
        {
            get { return match; }
        }

        /// <summary>
        /// Gets the command sink that was last pushed on this pipeline.
        /// </summary>
        public ICommandSink TopMostSink
        {
            get
            {
                if (filters.Count == 0) return sink;
                else return filters[0];
            }
        }

        /// <summary>
        /// Gets a value indicating if the command pipeline process supports
        /// being paused.
        /// </summary>
        public bool IsPausable
        {
            get { return filters.None(f => f is CommandSynchronizer); }
        }
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
            commander.CommandIssued += OnCommandIssued;
        }

        /// <summary>
        /// Adds a <see cref="Commander"/> to this <see cref="CommandPipeline"/>,
        /// assuming the first <see cref="ICommandSink"/> as its sink.
        /// </summary>
        /// <param name="commander">A <see cref="Commander"/> to be added.</param>
        public void AddCommander(Commander commander)
        {
            AddCommander(commander, TopMostSink);
        }

        private void OnCommandIssued(Commander commander, Command command)
        {
            Argument.EnsureNotNull(commander, "commander");
            Argument.EnsureNotNull(command, "command");

            commanders[commander].Handle(command);
        }
        #endregion

        public void Update(SimulationStep step)
        {
            foreach (Commander commander in commanders.Keys)
                commander.Update(step);

            foreach (CommandFilter filter in filters)
                filter.Update(step);

            sink.Update(step);
        }

        /// <summary>
        /// Releases all resources used by this pipeline and its filters/sinks.
        /// </summary>
        public void Dispose()
        {
            foreach (CommandFilter filter in filters)
                filter.Dispose();
            filters.Clear();

            sink.Dispose();

            commanders.Clear();
        }
        #endregion
    }
}