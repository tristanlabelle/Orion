using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.GameLogic.Tasks;
using OpenTK;

namespace Orion.Commandment.Commands
{
    public class MockCommand : Command
    {

        #region Fields


        #endregion


        #region Constructors

        public MockCommand(Faction sourceFaction)
            : base(sourceFaction)
        {
        }

        #endregion

        #region Events

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        /// Assign task to all unit for this command
        /// </summary>
        public override void Execute()
        {
            Console.WriteLine("Command Executed");
        }

        #endregion



    }
}
