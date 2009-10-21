using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using Orion.Graphics;

namespace Orion.Main
{
    public enum DeferenceType
    {
        Defer, Stack
    }

    abstract class RunLoop
    {
        #region Fields
        private RunLoop child;
        private DeferenceType deferenceType;

        protected GameUI userInterface;
        #endregion

        #region Constructors

        public RunLoop(GameUI ui)
        {
            userInterface = ui;
        }

        #endregion

        #region Events

        private event GenericEventHandler<RunLoop, object> Exited;

        #endregion

        #region Methods

        public void DeferTo(RunLoop runLoop, DeferenceType how)
        {
            child = runLoop;
            deferenceType = how;
        }

        public void RunOnce()
        {
            PrepareRun();
            if (child == null)
            {
                RunLoopMain();
            }
            else
            {
                child.RunOnce();
            }
            CleanupRun();
        }

        protected abstract void PrepareRun();
        protected abstract void RunLoopMain();
        protected abstract void CleanupRun();

        protected void ExitRunLoop()
        {
            GenericEventHandler<RunLoop, object> handler = Exited;
            if (handler != null)
            {
                Exited(this, null);
            }
        }

        private void WhenExit(RunLoop runLoop, object dummy)
        {
            child = null;
            if (deferenceType == DeferenceType.Defer) ExitRunLoop();
        }

        #endregion
    }
}
