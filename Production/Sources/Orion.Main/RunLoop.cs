using System;
using System.Windows.Forms;

using Orion.UserInterface;

namespace Orion.Main
{
    abstract class RunLoop
    {
        #region Fields
        protected GameUI userInterface;
        #endregion

        #region Constructors
        public RunLoop(GameUI ui)
        {
            userInterface = ui;
        }
        #endregion

        #region Methods
        public void RunOnce()
        {
            Application.DoEvents();
            userInterface.Refresh();
            RunLoopMain();
        }

        protected abstract void RunLoopMain();
        #endregion
    }
}
