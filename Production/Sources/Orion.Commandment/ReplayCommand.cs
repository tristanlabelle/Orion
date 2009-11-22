using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Commandment
{
    /// <summary>
    /// Encapsulates a command and its location in time.
    /// </summary>
    public struct ReplayCommand
    {
        #region Fields
        private readonly int updateNumber;
        private readonly Command command;
        #endregion

        #region Constructors
        public ReplayCommand(int updateNumber, Command command)
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
