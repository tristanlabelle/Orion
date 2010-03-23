﻿using System;
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
        protected readonly ActionFrame actionFrame;
        protected readonly GameGraphics gameGraphics;
        #endregion

        #region Constructors
        public ActionEnabler(UserInputManager inputManager, ActionFrame actionFrame, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(actionFrame, "actionFrame");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.inputManager = inputManager;
            this.actionFrame = actionFrame;
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