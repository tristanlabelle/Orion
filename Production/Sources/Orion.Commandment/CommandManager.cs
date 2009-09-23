using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.Commandment.Commands;

namespace Orion.Commandment
{
    public sealed class CommandManager
    {
        #region Fields
        List<Commander> commmanderList = new List<Commander>();
        Queue<Command> commandQueue = new Queue<Command>();

        #endregion

        #region Constructors

        #endregion

        #region Events

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        /// Query command from all existing commander in the list commanderList
        /// </summary>
        public void QueryCommands()
        {
            foreach (Commander aCommander in commmanderList)
            {
                foreach(Command aCommandFromACommander in aCommander.CreateCommands())
                {
                    commandQueue.Enqueue(aCommandFromACommander);
                }
                
            }
        }

        /// <summary>
        /// add a commander to the list of the commander in the current game
        /// </summary>
        /// <param name="commander">the commander to add</param>
        public void AddCommander(Commander commander)
        {
            commmanderList.Add(commander);
        }


        /// <summary>
        /// Execute the current commandQueue
        /// </summary>
        public void ExecuteCommandQueue()
        {
            foreach (Command command in commandQueue)
            {
                command.Execute();
            }
        }
        #endregion
    }
}
