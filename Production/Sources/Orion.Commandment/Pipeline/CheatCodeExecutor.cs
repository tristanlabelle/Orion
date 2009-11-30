using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Commandment.Commands;
using Orion.GameLogic;
using Skills = Orion.GameLogic.Skills;

using OpenTK.Math;

namespace Orion.Commandment.Pipeline
{
    /// <summary>
    /// A command filter which executes cheat codes if messages match them.
    /// </summary>
    public sealed class CheatCodeExecutor : CommandFilter
    {
        #region Fields
        private readonly CheatCodeManager cheatCodeManager;
        private readonly Match match;
        private readonly Queue<Command> accumulatedCommands = new Queue<Command>();
        #endregion

        #region Constructors
        public CheatCodeExecutor(CheatCodeManager cheatCodeManager, Match match)
        {
            Argument.EnsureNotNull(cheatCodeManager, "cheatCodeManager");
            Argument.EnsureNotNull(match, "match");

            this.cheatCodeManager = cheatCodeManager;
            this.match = match;
        }
        #endregion

        #region Methods
        public override void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            accumulatedCommands.Enqueue(command);
        }

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            while (accumulatedCommands.Count > 0)
            {
                Command command = accumulatedCommands.Dequeue();
                SendMessage message = command as SendMessage;
                if (message != null && cheatCodeManager.Exists(message.Text))
                {
                    Faction faction = match.World.FindFactionFromHandle(message.FactionHandle);
                    cheatCodeManager.Execute(message.Text, match, faction);
                    command = new SendMessage(message.FactionHandle, "Cheat '{0}' enabled!".FormatInvariant(message.Text));
                }
                Flush(command);
            }
        }
        #endregion
    }
}
