using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.Commandment.Commands;

namespace Orion.Commandment
{
    public abstract class Commander
    {

        #region Fields
        Faction faction; 


        #endregion

        #region Constructors

        #endregion

        #region Events

        #endregion

        #region Properties

        #endregion

        #region Methods
        public abstract IEnumerable<Command> CreateCommands();
        
        #endregion
    }
}
