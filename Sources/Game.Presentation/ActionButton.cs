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
        private Frame tooltipFrame;

        #endregion

        #region Constructors
        public ActionButton(Texture texture, string name, string hotkey, GenericEventHandler<Button> action)
            : base(new Rectangle(0.9f, 0.9f), "", new TexturedFrameRenderer(texture))
        {
            this.name = name;
            this.hotkey = hotkey;
            Pressed += action;

            Text tooltip = new Text("{0} ({1})".FormatInvariant(name, hotkey));
            tooltipFrame = new Frame(tooltip.Frame.Translate(-3, -3).Resize(6, 6), new FilledFrameRenderer());
            Label tooltipLabel = new Label(tooltip);
            tooltipFrame.Frame = tooltipFrame.Frame.TranslateTo(-tooltipFrame.Frame.CenterX, Bounds.MaxY * 1.1f);
            tooltipLabel.Frame = tooltipLabel.Frame.Translate(3, 3);
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

            return base.OnMouseEnter(args);
        }

        #endregion
    }
}
