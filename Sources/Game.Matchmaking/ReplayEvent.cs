using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Matchmaking.Commands;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// Encapsulates a command and its location in time.
    /// </summary>
    public struct ReplayEvent
    {
        #region Fields
        private readonly int updateNumber;
        private readonly Command command;
        #endregion

        #region Constructors
        public ReplayEvent(int updateNumber, Command command)
        {
            Argument.EnsureNotNull(command, "command");
            this.updateNumber = updateNumber;
            this.command = command;
        }
        #endregion

        #region Properties
        public int UpdateNumber
        {
            get { return updateNumber; }
        }

        public Command Command
        {
            get { return command; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return "Update #{0}, {1}".FormatInvariant(updateNumber, command);
        }
        #endregion
    }
}
