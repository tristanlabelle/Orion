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
        private readonly Faction sourceFaction;

       
 
        #endregion

        #region Constructors
        protected Command(Faction sourceFaction)
        {
            Argument.EnsureNotNull(sourceFaction, "sourceFaction");
            this.sourceFaction = sourceFaction;
        }
        #endregion

        #region Events

        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="Faction"/> that emitted this <see cref="Command"/>.
        /// </summary>
        public Faction SourceFaction
        {
            get { return sourceFaction; }
        } 
        #endregion

        #region Methods
            public abstract void Execute();
        #endregion


    }
}
