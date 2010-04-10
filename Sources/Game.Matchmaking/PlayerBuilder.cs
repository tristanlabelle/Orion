using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Matchmaking
{
    public class PlayerBuilder
    {
        #region Fields
        private Func<ColorRgb, Player> buildDelegate;
        private readonly string name;
        #endregion

        #region Constructors
        public PlayerBuilder(string name, Func<ColorRgb, Player> buildDelegate)
        {
            this.name = name;
            this.buildDelegate = buildDelegate;
        }
        #endregion

        #region Methods
        public Player Create(ColorRgb color)
        {
            return buildDelegate(color);
        }

        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
