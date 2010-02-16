using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Graphics;
using Orion.Geometry;
using Orion.Matchmaking;
using Orion.UserInterface.Widgets;
using Orion.UserInterface.Actions.Enablers;

namespace Orion.UserInterface.Actions
{
    public class ActionButton : Button
    {
        #region Fields
        protected readonly ActionFrame actionFrame;
        protected readonly UserInputManager inputManager;
        protected readonly TextureManager textureManager;
        private string name;
        private string description;
        #endregion

        #region Constructors
        public ActionButton(ActionFrame actionFrame, UserInputManager inputManager,
            string name, Keys hotkey, TextureManager textureManager)
            : base(new Rectangle(1,1), string.Empty)
        {
            Argument.EnsureNotNull(actionFrame, "actionFrame");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(name, "name");

            base.HotKey = hotkey;
            this.actionFrame = actionFrame;
            this.inputManager = inputManager;
            this.textureManager = textureManager;
            this.name = name;
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
            set
            {
                Argument.EnsureNotNull(value, "Name");
                name = value;
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                Argument.EnsureNotNull(value, "Name");
                description = value;
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
        protected override bool OnMouseEnter(MouseEventArgs args)
        {
            actionFrame.TooltipFrame.SetDescription(TooltipText);
            actionFrame.ShowTooltip();
            return base.OnMouseEnter(args);
        }

        protected override bool OnMouseExit(MouseEventArgs args)
        {
            actionFrame.HideTooltip();
            return base.OnMouseExit(args);
        }
        #endregion
    }
}
