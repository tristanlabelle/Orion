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
        private Stack<IActionProvider> actionProviders = new Stack<IActionProvider>();

        public ActionFrame(Rectangle frame)
            : base(frame)
        {
            Bounds = new Rectangle(4, 4);
        }

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

        private void ResetActions()
        {
            while (Children.Count > 0) Children[0].Dispose();
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
                        ActionButton button = provider.GetButtonAt(x, y);
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
    }
}
