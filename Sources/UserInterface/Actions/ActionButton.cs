using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Graphics;
using Orion.Geometry;
using Orion.Matchmaking;
using Orion.UserInterface.Actions.Enablers;
using Orion.UserInterface.Widgets;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions
{
    public class ActionButton : Button
    {
        #region Fields
        protected readonly ActionFrame actionFrame;
        protected readonly UICommander uiCommander;
        protected readonly GameGraphics gameGraphics;
        private string name;
        private string description;
        #endregion

        #region Constructors
        public ActionButton(ActionFrame actionFrame, UICommander uiCommander,
            string name, Keys hotkey, GameGraphics gameGraphics)
            : base(new Rectangle(1,1), string.Empty)
        {
            Argument.EnsureNotNull(actionFrame, "actionFrame");
            Argument.EnsureNotNull(uiCommander, "uiCommander");
            Argument.EnsureNotNull(name, "name");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            base.HotKey = hotkey;
            this.actionFrame = actionFrame;
            this.uiCommander = uiCommander;
            this.gameGraphics = gameGraphics;
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

        protected GameGraphics GameGraphics
        {
            get { return gameGraphics; }
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
