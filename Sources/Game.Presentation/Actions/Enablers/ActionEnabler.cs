using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Presentation;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public abstract class ActionEnabler
    {
        #region Fields
        protected readonly UserInputManager inputManager;
        protected readonly ActionPanel actionPanel;
        protected readonly GameGraphics gameGraphics;
        #endregion

        #region Constructors
        public ActionEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.inputManager = inputManager;
            this.actionPanel = actionPanel;
            this.gameGraphics = gameGraphics;
        }
        #endregion

        #region Properties
        protected Faction LocalFaction
        {
            get { return inputManager.LocalFaction; }
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
