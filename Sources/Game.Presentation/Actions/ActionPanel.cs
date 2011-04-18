using System;
using System.Collections.Generic;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Game.Presentation.Gui;
using Key = OpenTK.Input.Key;

namespace Orion.Game.Presentation.Actions
{
    public sealed class ActionPanel
    {
        #region Fields
        private readonly MatchUI ui;
        private readonly Stack<IActionProvider> actionProviders = new Stack<IActionProvider>();
        #endregion
        
        #region Constructors
        public ActionPanel(MatchUI ui)
        {
        	Argument.EnsureNotNull(ui, "ui");
        	this.ui = ui;
        }
        #endregion

        #region Methods
        public ActionDescriptor CreateCancelAction(UserInputManager inputManager, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            return new ActionDescriptor()
            {
            	Name = "Annuler",
            	Texture = gameGraphics.GetActionTexture("Cancel"),
            	HotKey = Key.Escape,
            	Action = () =>
	            {
                inputManager.SelectedCommand = null;
                this.Restore();
	            }
            };
        }

        public void Pop()
        {
            PopAndDispose();
            Refresh();
        }

        public void Push(IActionProvider provider)
        {
            Argument.EnsureNotNull(provider, "provider");

            actionProviders.Push(provider);
            Refresh();
        }

        public void Clear()
        {
            actionProviders.Clear();
            Refresh();
        }

        /// <summary>
        /// Pops all pushed <see cref="IActionProvider"/>s until the root one is reached.
        /// </summary>
        public void Restore()
        {
            while (actionProviders.Count > 1) PopAndDispose();

            Refresh();
        }

        private void PopAndDispose()
        {
            IActionProvider previousActionProvider = actionProviders.Pop();
            previousActionProvider.Dispose();
        }

        public void Refresh()
        {
            if (actionProviders.Count == 0) return;
            
            IActionProvider provider = actionProviders.Peek();
            provider.Refresh();

            for (int y = 3; y >= 0; y--)
            {
                for (int x = 0; x < 4; x++)
                {
                    Point point = new Point(x, y);
                    ui.SetActionButton(3 - y, x, provider.GetActionAt(point));
                    }
                }
            }
        #endregion
    }
}
