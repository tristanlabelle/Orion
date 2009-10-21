using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Graphics;

namespace Orion.Main
{
    abstract class MatchConfigurer
    {
        protected GameUI ui;

        public MatchConfigurer(GameUI ui)
        {
            this.ui = ui;
        }

        public abstract Match Start();
    }
}
