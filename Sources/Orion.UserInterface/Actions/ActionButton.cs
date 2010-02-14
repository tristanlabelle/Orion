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
    public class ActionButton : Button
    {
        #region Fields
        protected readonly ActionFrame container;
        protected readonly UserInputManager inputManager;
        private readonly TextureManager textureManager;
        private string name;
        private string description;
        private IActionProvider actionProvider;
        #endregion

        #region Constructors
        public ActionButton(ActionFrame container, UserInputManager inputManager,
            string name, Keys hotkey, TextureManager textureManager)
            : base(new Rectangle(1,1), string.Empty)
        {
            Argument.EnsureNotNull(container, "container");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(name, "name");

            base.HotKey = hotkey;
            this.container = container;
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

        public IActionProvider ActionProvider
        {
            get { return actionProvider; }
            set { actionProvider = value; }
        }
        #endregion

        #region Methods
        protected override bool OnMouseEnter(MouseEventArgs args)
        {
            container.TooltipFrame.SetDescription(TooltipText);
            container.ShowTooltip();
            return base.OnMouseEnter(args);
        }

        protected override bool OnMouseExit(MouseEventArgs args)
        {
            container.HideTooltip();
            return base.OnMouseExit(args);
        }

        protected override void OnPress()
        {
            // The base call must be done first to raise the Triggered event
            // because it seems that container.Push disposes this button.
            base.OnPress();
            if (actionProvider == null) actionProvider = new CancelActionProvider(container, inputManager, textureManager);
            container.Push(actionProvider);
        }
        #endregion
    }
}
