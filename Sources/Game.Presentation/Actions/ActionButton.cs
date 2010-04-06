﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Engine.Input;
using Orion.Game.Presentation;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation.Actions.Enablers;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions
{
    public class ActionButton : Button
    {
        #region Fields
        protected readonly ActionPanel actionPanel;
        protected readonly UserInputManager inputManager;
        protected readonly GameGraphics gameGraphics;
        private string name;
        private string description;
        #endregion

        #region Constructors
        public ActionButton(ActionPanel actionPanel, UserInputManager inputManager,
            string name, Keys hotkey, GameGraphics gameGraphics)
            : base(new Rectangle(1,1), string.Empty)
        {
            Argument.EnsureNotNull(actionPanel, "actionPanel");
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(name, "name");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            base.HotKey = hotkey;
            this.actionPanel = actionPanel;
            this.inputManager = inputManager;
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
        protected override bool OnMouseEntered(MouseEventArgs args)
        {
            actionPanel.TooltipPanel.SetDescription(TooltipText);
            actionPanel.ShowTooltip();
            return base.OnMouseEntered(args);
        }

        protected override bool OnMouseExited(MouseEventArgs args)
        {
            actionPanel.HideTooltip();
            return base.OnMouseExited(args);
        }
        #endregion
    }
}
