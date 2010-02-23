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
        protected readonly UserInputManager inputManager;
        protected readonly ActionFrame actionFrame;
        protected readonly TextureManager textureManager;
        #endregion

        #region Constructors
        public ActionEnabler(UserInputManager inputManager, ActionFrame actionFrame, TextureManager textureManager)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(actionFrame, "actionFrame");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.inputManager = inputManager;
            this.actionFrame = actionFrame;
            this.textureManager = textureManager;
        }
        #endregion

        #region Properties
        protected SlaveCommander LocalCommander
        {
            get { return inputManager.LocalCommander; }
        }

        protected Faction LocalFaction
        {
            get { return LocalCommander.Faction; }
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
