using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui2;
using Orion.Engine.Data;
using System.Linq.Expressions;
using Orion.Engine.Gui2.Adornments;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Provides the user interface to edit the settings of a match.
    /// </summary>
    public sealed class MatchConfigurationUI2 : ContentControl
    {
        #region Fields
        private readonly OrionGuiStyle style;
        private CheckBox readyCheckBox;
        private Button startButton;
        private FormLayout settingsForm;
        private bool canChangeSettings = true;
        private bool canKickPlayers = true;
        #endregion

        #region Constructors
        public MatchConfigurationUI2(OrionGuiStyle style)
        {
            Argument.EnsureNotNull(style, "style");

            this.style = style;

            Adornment = new ColoredBackgroundAdornment(Colors.Gray);

            DockLayout mainDock = new DockLayout()
            {
                LastChildFill = true
            };

            mainDock.Dock(CreateBottomDock(), Direction.PositiveY);
            mainDock.Dock(CreateContentDock(), Direction.NegativeX);

            Content = mainDock;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the state of readiness of the player changes.
        /// </summary>
        public event Action<MatchConfigurationUI2> ReadinessChanged;

        /// <summary>
        /// Raised when the user clicks the button to start the match.
        /// </summary>
        public event Action<MatchConfigurationUI2> MatchStarted;

        /// <summary>
        /// Raised when the user exits the screen.
        /// </summary>
        public event Action<MatchConfigurationUI2> Exited;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses a value indicating if the user needs to mark himself as ready before the game can start.
        /// </summary>
        public bool NeedsReadying
        {
            get { return readyCheckBox.VisibilityFlag == Visibility.Visible; }
            set { readyCheckBox.VisibilityFlag = value ? Visibility.Visible : Visibility.Hidden; }
        }

        /// <summary>
        /// Accesses a value indicating if the user has marked himself as being ready to start the game.
        /// </summary>
        [PropertyChangedEvent("ReadinessChanged")]
        public bool IsReady
        {
            get { return readyCheckBox.IsChecked; }
            set { readyCheckBox.IsChecked = value; }
        }

        /// <summary>
        /// Accesses a value indicating if the user can start the game.
        /// </summary>
        public bool CanStart
        {
            get { return startButton.HasEnabledFlag; }
            set { startButton.HasEnabledFlag = value; }
        }

        /// <summary>
        /// Accesses a value if the user can changes the settings.
        /// </summary>
        public bool CanChangeSettings
        {
            get { return settingsForm.HasEnabledFlag; }
            set { settingsForm.HasEnabledFlag = value; }
        }

        /// <summary>
        /// Accesses a value indicating if the user can kick other players from the game.
        /// </summary>
        public bool CanKickPlayers
        {
            get { return canKickPlayers; }
            set { canKickPlayers = value; }
        }
        #endregion

        #region Methods
        public void AddBooleanSetting(string text, Expression<Func<bool>> bindingSourcePropertyExpression)
        {
            CheckBox checkBox = style.Create<CheckBox>();
            Binding.CreateTwoWay(bindingSourcePropertyExpression, () => checkBox.IsChecked);
            AddSetting(text, checkBox);
        }

        private void AddSetting(string text, Control control)
        {
            Argument.EnsureNotNull(text, "text");

            Label label = style.CreateLabel(text);
            label.VerticalAlignment = Alignment.Center;
            control.VerticalAlignment = Alignment.Center;
            settingsForm.AddEntry(label, control);
        }

        private DockLayout CreateBottomDock()
        {
            DockLayout bottomDock = new DockLayout();

            Button backButton = style.CreateTextButton("Retour");
            backButton.Clicked += (sender, mouseButton) => Exited.Raise(this);
            bottomDock.Dock(backButton, Direction.NegativeX);

            startButton = style.CreateTextButton("Commencer");
            startButton.Clicked += (sender, mouseButton) => MatchStarted.Raise(this);
            startButton.MinXMargin = 20;
            bottomDock.Dock(startButton, Direction.PositiveX);

            readyCheckBox = style.CreateTextCheckBox("Je suis prêt");
            readyCheckBox.StateChanged += sender => ReadinessChanged.Raise(this);
            bottomDock.Dock(readyCheckBox, Direction.PositiveX);
            return bottomDock;
        }

        private DockLayout CreateContentDock()
        {
            DockLayout contentDock = new DockLayout()
            {
                LastChildFill = true
            };

            settingsForm = new FormLayout()
            {
                MinEntrySize = 32,
                HeaderContentGap = 6
            };

            contentDock.Dock(settingsForm, Direction.PositiveX);
            return contentDock;
        }
        #endregion
    }
}
