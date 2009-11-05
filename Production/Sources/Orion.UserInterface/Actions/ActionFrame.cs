using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;
using Orion.Geometry;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public class ActionFrame : Frame
    {
        #region Fields
        private Stack<IActionProvider> actionProviders = new Stack<IActionProvider>();
        #endregion

        #region Constructors
        public ActionFrame(Rectangle frame)
            : base(frame)
        {
            Bounds = new Rectangle(4, 4);
        }
        #endregion

        #region Methods

        public void Push(IActionProvider provider)
        {
            actionProviders.Push(provider);
            ResetActions();
        }

        public void ClearStack()
        {
            actionProviders.Clear();
        }

        public void Pop()
        {
            actionProviders.Pop();
            ResetActions();
        }

        private void ResetActions()
        {
            Children.Clear();
            if (actionProviders.Count > 0)
            {
                IActionProvider provider = actionProviders.Peek();
                Rectangle templateSize = new Rectangle(0.8f, 0.8f);

                for (int y = 3; y >= 0; y--)
                {
                    Vector2 origin = new Vector2(0.1f, 0.1f + y);
                    for (int x = 0; x < 4; x++)
                    {
                        ActionButton button = provider[x, y];
                        if (button != null)
                        {
                            button.Frame = templateSize.TranslatedBy(origin);
                            Children.Add(button);
                        }
                        origin.X += 1;
                    }
                }
            }
        }

        #endregion
    }
}
