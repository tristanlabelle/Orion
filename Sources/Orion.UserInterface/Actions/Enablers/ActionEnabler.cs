using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using Orion.Graphics;
using Orion.Geometry;
using Orion.Commandment;

namespace Orion.UserInterface.Actions.Enablers
{
    public abstract class ActionEnabler
    {
        #region Fields
        protected readonly UserInputManager inputManager;
        protected readonly ActionFrame container;
        protected readonly TextureManager textureManager;
        #endregion

        #region Constructors
        public ActionEnabler(UserInputManager manager, ActionFrame frame, TextureManager textureManager)
        {
            inputManager = manager;
            container = frame;
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
