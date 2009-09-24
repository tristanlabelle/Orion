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
        World world;



        #endregion

        #region Constructors
        public Commander(Faction sourceFaction, World world)
        {
            Argument.EnsureNotNull(sourceFaction, "sourceFaction");
            Argument.EnsureNotNull(world, "world");

            this.world = world;
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

        /// <summary>
        /// Gets the <see cref="World"/> of this Commander.
        /// </summary>
        /// 
        public World World
        {
            get { return world; }
        }

        #endregion

        

        #region Methods
        public abstract IEnumerable<Command> CreateCommands();
        #endregion
    }
}
