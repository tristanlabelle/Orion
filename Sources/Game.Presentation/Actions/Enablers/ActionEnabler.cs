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
        protected readonly UserInputManager userInputManager;
        protected readonly ActionPanel actionPanel;
        protected readonly GameGraphics gameGraphics;
        #endregion

        #region Constructors
        public ActionEnabler(UserInputManager userInputManager, ActionPanel actionPanel, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(userInputManager, "userInputManager");
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.userInputManager = userInputManager;
            this.actionPanel = actionPanel;
            this.gameGraphics = gameGraphics;
        }
        #endregion

        #region Properties
        protected Faction LocalFaction
        {
            get { return userInputManager.LocalFaction; }
        }

        protected World World
        {
            get { return userInputManager.World; }
        }

        protected Match Match
        {
            get { return userInputManager.Match; }
        }
        #endregion

        #region Methods
        public abstract void LetFill(UnitType type, ActionButton[,] buttonsArray);
        #endregion
    }
}
