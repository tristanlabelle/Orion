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
        private readonly TextureManager textureManager;
        protected readonly ActionFrame container;
        protected readonly UserInputManager inputManager;
        private string name;
        private string description;
        private Frame tooltipContainer;
        #endregion

        #region Constructors
        protected ActionButton(ActionFrame frame, UserInputManager inputManager, string name, Keys hotkey, TextureManager textureManager)
            : base(new Rectangle(1,1), string.Empty)
        {
            Argument.EnsureNotNull(frame, "frame");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(name, "name");

            base.HotKey = hotkey;
            this.container = frame;
            this.inputManager = inputManager;
            this.name = name;
            UpdateTooltip();
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
            set
            {
                Argument.EnsureNotNull(value, "Name");
                this.name = value;
                UpdateTooltip();
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                UpdateTooltip();
            }
        }

        public string TooltipText
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(name);

                if (HotKey != Keys.None)
                {
                    stringBuilder.Append(" (");
                    stringBuilder.Append(HotKey.ToStringInvariant());
                    stringBuilder.Append(')');
                }

                string description = Description;
                if (description != null)
                {
                    stringBuilder.Append('\n');
                    stringBuilder.Append(description);
                }

                return stringBuilder.ToString();
            }
        }

        protected TextureManager TextureManager
        {
            get { return textureManager; }
        }
        #endregion

        #region Methods
        public virtual ActionButton GetButtonAt(int x, int y)
        {
            if (x == 3 && y == 0)
                return new CancelButton(container, inputManager, textureManager);
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

        private void UpdateTooltip()
        {
            const float defaultFontSize = 28;

            if (tooltipContainer != null)
            {
                tooltipContainer.Dispose();
                tooltipContainer = null;
            }

            IEnumerable<Text> lines = TooltipText.Split('\n').Select(str => new Text(str));
            Rectangle tooltipFrameRect = new Rectangle(lines.Max(t => t.Frame.Width), lines.Count() * defaultFontSize);
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
        #endregion
    }
}
