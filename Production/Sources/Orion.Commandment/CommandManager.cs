using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.Commandment.Commands;

namespace Orion.Commandment
{
    /// <summary>
    /// Keeps a list of <see cref="Commander"/>s and handles their <see cref="Command"/>s.
    /// </summary>
    public sealed class CommandManager
    {
        #region Fields
        private readonly List<Commander> commanders = new List<Commander>();
        private readonly Queue<Command> queuedCommands = new Queue<Command>();
        #endregion

        #region Methods
        private void OnCommandGenerated(Commander sender, Command args)
        {
            Argument.EnsureNotNull(args, "args");
            queuedCommands.Enqueue(args);
        }

        /// <summary>
        /// Adds a <see cref="Commander"/> to this <see cref="CommandManager"/>.
        /// </summary>
        /// <param name="commander">The <see cref="Commander"/> to be added.</param>
        public void AddCommander(Commander commander)
        {
            Argument.EnsureNotNull(commander, "commander");

            if (!commanders.Contains(commander))
            {
                commanders.Add(commander);
                commander.CommandGenerated += OnCommandGenerated;
            }
        }

        /// <summary>
        /// Updates this <see cref="CommandManager"/> for a frame.
        /// </summary>
        /// <param name="timeDelta">The amount of time elapsed since the last frame.</param>
        public void Update(float timeDelta)
        {
            foreach (Commander commander in commanders)
                commander.Update(timeDelta);

            while (queuedCommands.Count > 0)
                queuedCommands.Dequeue().Execute();
        }
        #endregion
    }
}
