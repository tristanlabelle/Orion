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
        #endregion

        public ActionEnabler(UserInputManager manager, ActionFrame frame)
        {
            inputManager = manager;
            container = frame;
        }

        public abstract void LetFill(UnitType type, ActionButton[,] buttonsArray);
    }
}
