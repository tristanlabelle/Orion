﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 

using Orion.GameLogic;

namespace Orion.Commandment
{
    /// <summary>
    /// Abstract base class for commands, the atomic unit of game state change
    /// which encapsulate an order given by a <see cref="Commander"/>.
    /// </summary>
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

        #region Properties
        /// <summary>
        /// Gets the <see cref="Faction"/> that emitted this <see cref="Command"/>.
        /// </summary>
        public Faction SourceFaction
        {
            get { return sourceFaction; }
        }

        /// <summary>
        /// Gets the id associated to the type of this <see cref="Command"/>.
        /// </summary>
        public byte TypeID
        {
            get
            {
                SerializableCommandAttribute serializationInfo =
                    (SerializableCommandAttribute)GetType().GetCustomAttributes(
                    typeof(SerializableCommandAttribute), false)[0];
                return serializationInfo.ID;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes this <see cref="Command"/>.
        /// </summary>
        public abstract void Execute();
        #endregion
    }
}
