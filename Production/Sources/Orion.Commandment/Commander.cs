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
        public Commander(Faction sourceFaction)
        {
            Argument.EnsureNotNull(sourceFaction, "sourceFaction");
            this.faction = sourceFaction;
        }
        #endregion

        #region Events

        #endregion

        
        #region Properties
        /// <summary>
        /// Gets the <see cref="Faction"/> of this Commander.
        /// </summary>
        /// 
        public Faction Faction
        {
            get { return faction; }
        }

        #endregion
        

        #region Methods
        public abstract IEnumerable<Command> CreateCommands();
        
        #endregion
    }
}
