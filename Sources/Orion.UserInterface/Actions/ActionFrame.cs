using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;
using Orion.Geometry;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface.Actions
{
    public class ActionFrame : Frame
    {
        #region Fields
        private Stack<IActionProvider> actionProviders = new Stack<IActionProvider>();
        private TooltipFrame tooltipFrame;
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
        public void Pop()
        {
            actionProviders.Pop();
            ResetActions();
        }

        public void Push(IActionProvider provider)
        {
            actionProviders.Push(provider);
            ResetActions();
        }

        public void Clear()
        {
            actionProviders.Clear();
            ResetActions();
        }

        public void Restore()
        {
            while (actionProviders.Count > 1) actionProviders.Pop();
            ResetActions();
        }

        internal void ShowTooltip()
        {
            if(!Children.Contains(tooltipFrame))
                Children.Add(tooltipFrame);
        }

        internal void HideTooltip()
        {
            Children.Remove(tooltipFrame);
        }

        private void ResetActions()
        {
            while (Children.Count > 0) Children[0].Dispose();
            Children.Clear();
            if (actionProviders.Count > 0)
            {
                IActionProvider provider = actionProviders.Peek();
                Rectangle templateSize = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(0.2f, 0.2f));
                Vector2 padding = new Vector2(Bounds.Width * 0.0375f, Bounds.Height * 0.0375f);

                for (int y = 3; y >= 0; y--)
                {
                    Vector2 origin = new Vector2(padding.X, padding.Y + (templateSize.Height + padding.Y) * y);
                    for (int x = 0; x < 4; x++)
                    {
                        ActionButton button = provider.GetButtonAt(x, y);
                        if (button != null)
                        {
                            button.Frame = templateSize.TranslatedBy(origin);
                            Children.Add(button);
                        }
                        origin.X += padding.X + templateSize.Width;
                    }
                }
            }
        }
        #endregion
    }
}
