using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Graphics;
using Orion.Geometry;
using Orion.Commandment;
using Orion.UserInterface.Widgets;
using Orion.UserInterface.Actions.Enablers;

namespace Orion.UserInterface.Actions
{
    public abstract class ActionButton : Button, IActionProvider
    {
        #region Fields
        private string name;
        protected readonly ActionFrame container;
        protected readonly UserInputManager inputManager;
        private Frame tooltipContainer;
        private TextureManager textureManager;
        #endregion

        #region Constructors
        protected ActionButton(ActionFrame frame, UserInputManager manager, Keys hotkey, TextureManager textureManager)
            : base(new Rectangle(1,1), "")
        {
            this.textureManager = textureManager;
            HotKey = hotkey;
            container = frame;
            inputManager = manager;
            tooltipContainer = new Frame(new Rectangle(0, 0));
        }

        protected ActionButton(ActionFrame frame, UserInputManager manager, string name, Keys hotkey, TextureManager textureManager)
            : this(frame, manager, hotkey, textureManager)
        {
            Name = "{0} ({1})".FormatInvariant(name, hotkey);
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
            internal set
            {
                const float defaultFontSize = 28;
                name = value;
                tooltipContainer.Dispose();
                IEnumerable<Text> lines = value.Split('\n').Select(str => new Text(str));
                Rectangle tooltipFrameRect =
                    new Rectangle(lines.Max(t => t.Frame.Width), lines.Count() * defaultFontSize);
                Rectangle tooltipRect = tooltipFrameRect.ScaledBy(0.4f / defaultFontSize);
                tooltipContainer = new Frame(tooltipRect.TranslatedTo(-tooltipRect.CenterX + Bounds.CenterX, 1.2f));
                tooltipContainer.Bounds = tooltipFrameRect.TranslatedBy(-3, -3).ResizedBy(6, 6);

                int i = 0;
                foreach (Text text in lines.Reverse())
                {
                    Label tooltipLabel = new Label(text);
                    tooltipLabel.Frame = tooltipLabel.Frame.TranslatedBy(0, tooltipLabel.Frame.Height * i);
                    tooltipContainer.Children.Add(tooltipLabel);
                    i++;
                }
            }
        }
        #endregion

        #region Methods
        public virtual ActionButton GetButtonAt(int x, int y)
        {
            if (x == 3 && y == 0)
                return new CancelButton(container, inputManager,textureManager);
            return null;
        }

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
