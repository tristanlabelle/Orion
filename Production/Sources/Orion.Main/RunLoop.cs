using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Orion.GameLogic;
using Orion.UserInterface;

namespace Orion.Main
{
    public enum DeferenceType
    {
        Defer, Stack
    }

    abstract class RunLoop
    {
        #region Fields
        private RunLoop parent;
        private RunLoop child;

        protected GameUI userInterface;
        #endregion

        #region Constructors

        public RunLoop(GameUI ui)
        {
            userInterface = ui;
        }

        #endregion

        #region Methods

        public void DeferTo(RunLoop runLoop, DeferenceType how)
        {
            if (how == DeferenceType.Defer)
            {
                if (parent == null) throw new InvalidOperationException("Cannot fully defer execution of the main run loop");
                parent.child = runLoop;
            }
            else
            {
                runLoop.parent = this;
                child = runLoop;
            }
        }

        public void RunOnce()
        {
            PrepareRun();
            if (child == null) RunLoopMain();
            else child.RunOnce();
            CleanupRun();
        }

        protected virtual void PrepareRun()
        {
            Application.DoEvents();
            userInterface.Refresh();
        }

        protected abstract void RunLoopMain();

        protected virtual void CleanupRun()
        { }

        protected void ExitRunLoop()
        {
            // this makes little sense
            if (parent == null) throw new InvalidOperationException("Cannot exit the main run loop");
            parent.child = null;
        }

        #endregion
    }
}
