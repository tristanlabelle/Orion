using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Graphics;
using Orion.Geometry;
using Orion.Matchmaking;

namespace Orion.UserInterface.Actions.Enablers
{
    public abstract class ActionEnabler
    {
        #region Fields
        protected readonly UICommander uiCommander;
        protected readonly ActionFrame actionFrame;
        protected readonly GameGraphics gameGraphics;
        #endregion

        #region Constructors
        public ActionEnabler(UICommander uiCommander, ActionFrame actionFrame, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(uiCommander, "uiCommander");
            Argument.EnsureNotNull(actionFrame, "actionFrame");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.uiCommander = uiCommander;
            this.actionFrame = actionFrame;
            this.gameGraphics = gameGraphics;
        }
        #endregion

        #region Properties
        protected Faction LocalFaction
        {
            get { return uiCommander.Faction; }
        }

        protected World World
        {
            get { return LocalFaction.World; }
        }
        #endregion

        #region Methods
        public abstract void LetFill(UnitType type, ActionButton[,] buttonsArray);
        #endregion
    }
}
