using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Graphics;
using Orion.Geometry;
using Orion.Commandment;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface.Actions
{
    public abstract class ActionButton : Button, IActionProvider
    {
        #region Fields
        private string name;
        protected ActionFrame container;
        protected UserInputManager inputManager;
        private Frame tooltipContainer;
        #endregion

        #region Constructors
        protected ActionButton(ActionFrame frame, UserInputManager manager, string name, Keys hotkey)
            : base(new Rectangle(1,1), "")
        {
            this.name = name;
            HotKey = hotkey;
            container = frame;
            inputManager = manager;

            Text tooltipText = new Text("{0} ({1})".FormatInvariant(name, hotkey));
            Rectangle tooltipTextRect = tooltipText.Frame;
            Rectangle tooltipRect = tooltipTextRect.ScaledBy(0.4f / tooltipTextRect.Height);

            tooltipContainer = new Frame(tooltipRect.TranslatedTo(-tooltipRect.CenterX + Bounds.CenterX, 1.2f), new FilledFrameRenderer());
            tooltipContainer.Bounds = tooltipTextRect.TranslatedBy(-3, -3).ResizedBy(6, 6);
            tooltipContainer.Children.Add(new Label(tooltipText));
        }
        #endregion

        #region Indexers
        public virtual ActionButton this[int x, int y]
        {
            get
            {
                if (x == 3 && y == 0)
                    return new CancelButton(container, inputManager);
                return null;
            }
        }
        #endregion

        #region Methods

        protected override bool OnMouseEnter(MouseEventArgs args)
        {
            Children.Add(tooltipContainer);
            return base.OnMouseEnter(args);
        }

        protected override bool OnMouseExit(MouseEventArgs args)
        {
            Children.Remove(tooltipContainer);
            return base.OnMouseExit(args);
        }

        protected override void OnPress()
        {
            container.Push(this);
        }

        #endregion
    }
}
