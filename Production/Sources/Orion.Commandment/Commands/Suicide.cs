﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;

namespace Orion.Commandment.Commands
{
    public sealed class Suicide : Command
    {
        #region Fields
        private readonly List<Unit> suiciders;
        #endregion

        #region Constructor
        public Suicide(Faction faction, IEnumerable<Unit> units)
            : base(faction)
        {
            Argument.EnsureNoneNull(units, "units");
            this.suiciders = units.ToList();
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            foreach (Unit suicider in suiciders)
            {
                suicider.Suicide();
            }
        }

        public override string ToString()
        {
            return "[{0}] suicide".FormatInvariant(suiciders.ToCommaSeparatedValues());
        }
        #endregion
    }
}
