using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic; 

namespace Orion.Commandment
{
    public abstract class Command
    {
        #region Fields
            Faction sourceFaction; 



        #endregion

        #region Constructors

        #endregion

        #region Events

        #endregion

        #region Properties

        #endregion

        #region Methods
            public abstract void Execute();
            
        #endregion


    }
}
