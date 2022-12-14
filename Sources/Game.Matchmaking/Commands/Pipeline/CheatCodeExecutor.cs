using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;

namespace Orion.Game.Matchmaking.Commands.Pipeline
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

        public CheatCodeExecutor(Match match)
            : this(CheatCodeManager.Default, match) { }
        #endregion

        #region Methods
        public override void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            accumulatedCommands.Enqueue(command);
        }

        public override void Update(SimulationStep step)
        {
            while (accumulatedCommands.Count > 0)
            {
                Command command = accumulatedCommands.Dequeue();
                SendMessageCommand message = command as SendMessageCommand;
                if (message != null && cheatCodeManager.Exists(message.Text))
                {
                    Faction faction = match.World.FindFactionFromHandle(message.FactionHandle);
                    match.RaiseCheatUsed(faction, message.Text);
                    cheatCodeManager.Execute(message.Text, match, faction);
                }
                else
                {
                    Flush(command);
                }
            }
        }
        #endregion
    }
}
