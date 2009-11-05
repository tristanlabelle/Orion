using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

using Orion.Geometry;
using Orion.Graphics;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public class ActionButton : Button
    {
        #region Fields
        private string name;
        private string hotkey;
        private Frame tooltipContainer;

        #endregion

        #region Constructors
        public ActionButton(Texture texture, string name, string hotkey, GenericEventHandler<Button> action)
            : base(new Rectangle(0.9f, 0.9f), "", new FilledFrameRenderer())
        {
            this.name = name;
            this.hotkey = hotkey;
            Pressed += action;

            Text tooltipText = new Text("{0} ({1})".FormatInvariant(name, hotkey));
            Rectangle tooltipTextRect = tooltipText.Frame;
            Rectangle tooltipRect = tooltipTextRect.ScaledBy(0.4f / tooltipTextRect.Height);

            tooltipContainer = new Frame(tooltipRect.TranslatedTo(-tooltipRect.CenterX + Bounds.CenterX, 1.2f), new FilledFrameRenderer());
            tooltipContainer.Bounds = tooltipTextRect.TranslatedBy(-3, -3).ResizedBy(6, 6);
            tooltipContainer.Children.Add(new Label(tooltipText));
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
        }

        public string Hotkey
        {
            get { return hotkey; }
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

        #endregion
    }
}
