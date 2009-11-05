using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

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
        private Frame tooltipContainer;

        #endregion

        #region Constructors
        public ActionButton(Texture texture, string name, Keys hotkey, GenericEventHandler<Button> action)
            : base(new Rectangle(0.9f, 0.9f), "", new FilledFrameRenderer())
        {
            this.name = name;
            Pressed += action;
            HotKey = hotkey;

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
