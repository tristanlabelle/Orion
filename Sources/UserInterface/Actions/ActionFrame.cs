using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;
using Orion.Matchmaking;
using Orion.UserInterface.Widgets;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions
{
    public class ActionFrame : Frame
    {
        #region Fields
        private readonly Stack<IActionProvider> actionProviders = new Stack<IActionProvider>();
        private readonly TooltipFrame tooltipFrame;
        #endregion

        #region Constructors
        public ActionFrame(Rectangle frame)
            : base(frame)
        {
            tooltipFrame = new TooltipFrame(new Vector2(0, Bounds.MaxY), Bounds.Width);
        }
        #endregion

        #region Properties
        internal TooltipFrame TooltipFrame
        {
            get { return tooltipFrame; }
        }
        #endregion

        #region Methods
        public ActionButton CreateCancelButton(UserInputManager inputManager, TextureManager textureManager)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(textureManager, "textureManager");

            ActionButton button = new ActionButton(this, inputManager, "Cancel", Keys.Escape, textureManager);

            Texture texture = textureManager.GetAction("Cancel");
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                inputManager.SelectedCommand = null;
                this.Restore();
            };

            return button;
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

        internal void ShowTooltip()
        {
            if (!Children.Contains(tooltipFrame))
                Children.Add(tooltipFrame);
        }

        internal void HideTooltip()
        {
            Children.Remove(tooltipFrame);
        }

        public void Refresh()
        {
            Children.Clear();
            if (actionProviders.Count == 0) return;
            
            IActionProvider provider = actionProviders.Peek();
            provider.Refresh();

            Rectangle templateSize = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(0.2f, 0.2f));
            Vector2 padding = new Vector2(Bounds.Width * 0.0375f, Bounds.Height * 0.0375f);

            for (int y = 3; y >= 0; y--)
            {
                Vector2 origin = new Vector2(padding.X, padding.Y + (templateSize.Height + padding.Y) * y);
                for (int x = 0; x < 4; x++)
                {
                    Point point = new Point(x, y);
                    ActionButton button = provider.GetButtonAt(point);
                    if (button != null)
                    {
                        button.Frame = templateSize.TranslatedBy(origin);
                        Children.Add(button);
                    }
                    origin.X += padding.X + templateSize.Width;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // We don't actually own our children buttons so clear them
                // so the base Dispose does not do so
                Children.Clear();
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
