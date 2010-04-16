using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// Provides an opaque way to build a new player.
    /// </summary>
    public sealed class PlayerBuilder
    {
        #region Fields
        private Func<string, ColorRgb, Player> buildDelegate;
        private readonly string name;
        #endregion

        #region Constructors
        public PlayerBuilder(string name, Func<string, ColorRgb, Player> buildDelegate)
        {
            Argument.EnsureNotNull(name, "name");
            Argument.EnsureNotNull(buildDelegate, "buildDelegate");

            this.name = name;
            this.buildDelegate = buildDelegate;
        }
        #endregion

        #region Methods
        public Player Build(ColorRgb color)
        {
            return buildDelegate(name, color);
        }

        public override string ToString()
        {
            return name;
        }
        #endregion
    }
}
