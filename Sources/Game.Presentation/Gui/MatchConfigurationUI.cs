using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Orion.Engine;
using Orion.Engine.Data;
using Orion.Engine.Gui;
using Orion.Engine.Gui.Adornments;
using Orion.Engine.Localization;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Provides the user interface to edit the settings of a match.
    /// </summary>
    public sealed partial class MatchConfigurationUI : ContentControl
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly Localizer localizer;
        private readonly PlayerCollection players;
        private StackLayout playerStack;
        private FormLayout settingsForm;
        private StackLayout aiCreationStack;
        private ComboBox aiBuilderComboBox;
        private CheckBox readyCheckBox;
        private Button startButton;
        #endregion

        #region Constructors
        public MatchConfigurationUI(GameGraphics graphics, Localizer localizer)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(localizer, "localizer");

            this.graphics = graphics;
            this.localizer = localizer;
            this.players = new PlayerCollection(this);

            Padding = 5;
            Adornment = new TextureAdornment(graphics.GetGuiTexture("Granite")) { IsTiling = true };

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
        /// Raised when a player gets kicked.
        /// </summary>
        public event Action<MatchConfigurationUI, Player> PlayerKicked;

        /// <summary>
        /// Raised when the player changes its color.
        /// </summary>
        public event Action<MatchConfigurationUI, Player, ColorRgb> PlayerColorChanged;

        /// <summary>
        /// Raised when the state of readiness of the player changes.
        /// </summary>
        public event Action<MatchConfigurationUI> ReadinessChanged;

        /// <summary>
        /// Raised when the user clicks the button to start the match.
        /// </summary>
        public event Action<MatchConfigurationUI> MatchStarted;

        /// <summary>
        /// Raised when the user exits the screen.
        /// </summary>
        public event Action<MatchConfigurationUI> Exited;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of players in this match.
        /// </summary>
        public PlayerCollection Players
        {
            get { return players; }
        }

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
        
        private OrionGuiStyle Style
        {
        	get { return graphics.GuiStyle; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new kind of AI that can be added to the match.
        /// </summary>
        /// <param name="name">The name of the AI.</param>
        /// <param name="action">A callback to be invoked when the user adds one of such AIs to the match.</param>
        public void AddAIBuilder(string name, Action action)
        {
            Argument.EnsureNotNull(name, "name");
            Argument.EnsureNotNull(action, "action");

            Label text = Style.CreateLabel(name);
            text.Tag = action;
            aiBuilderComboBox.Items.Add(text);
            if (aiBuilderComboBox.SelectedItemIndex == -1)
                aiBuilderComboBox.SelectedItemIndex = 0;

            aiCreationStack.VisibilityFlag = Visibility.Visible;
        }

        public void AddSettings(MatchSettings settings)
        {
            AddIntegerSetting(localizer.GetNoun("MapWidth"), () => settings.MapWidth, MatchSettings.SuggestedMinimumMapSize.Width);
            AddIntegerSetting(localizer.GetNoun("MapHeight"), () => settings.MapHeight, MatchSettings.SuggestedMinimumMapSize.Height);
            AddIntegerSetting(localizer.GetNoun("InitialAladdium"), () => settings.InitialAladdiumAmount, MatchSettings.SuggestedMinimumAladdium);
            AddIntegerSetting(localizer.GetNoun("InitialAlagene"), () => settings.InitialAlageneAmount, MatchSettings.SuggestedMinimumAlagene);
            AddIntegerSetting(localizer.GetNoun("FoodLimit"), () => settings.FoodLimit, MatchSettings.SuggestedMinimumPopulation);

            AddIntegerSetting(localizer.GetNoun("RandomSeed"), () => settings.RandomSeed, int.MinValue);
            AddBooleanSetting(localizer.GetNoun("CheatCodes"), () => settings.AreCheatsEnabled);
            AddBooleanSetting(localizer.GetNoun("StartNomad"), () => settings.StartNomad);
            AddBooleanSetting(localizer.GetNoun("RandomHeroes"), () => settings.AreRandomHeroesEnabled);
            AddBooleanSetting(localizer.GetNoun("RevealTopology"), () => settings.RevealTopology);
        }
        
        public void AddIntegerSetting(string text, Expression<Func<int>> bindingSourcePropertyExpression, int minimumValue)
        {
            Argument.EnsureNotNull(text, "text");
            Argument.EnsureNotNull(bindingSourcePropertyExpression, "bindingSourcePropertyExpression");

            TextField textField = Style.Create<TextField>();

            Bindable bindable = BindableProperty.FromExpression(bindingSourcePropertyExpression);
            textField.Text = bindable.Value.ToString();
            bindable.ValueChanged += sender => textField.Text = bindable.Value.ToString();

            textField.TextChanged += sender =>
            {
                int value;
                bool isValid = int.TryParse(textField.Text, out value) && value >= minimumValue;
                textField.TextColor = isValid ? Colors.Black : Colors.Red;
            };
            textField.KeyboardFocusStateChanged += sender =>
            {
                int value;
                if (int.TryParse(textField.Text, out value) && value >= minimumValue)
                    bindable.Value = value;
            };

            AddSetting(text, textField);
        }

        public void AddBooleanSetting(string text, Expression<Func<bool>> bindingSourcePropertyExpression)
        {
            CheckBox checkBox = Style.Create<CheckBox>();
            Binding.CreateTwoWay(bindingSourcePropertyExpression, () => checkBox.IsChecked);
            AddSetting(text, checkBox);
        }

        private void AddSetting(string text, Control control)
        {
            Argument.EnsureNotNull(text, "text");

            Label label = Style.CreateLabel(text);
            label.VerticalAlignment = Alignment.Center;
            control.VerticalAlignment = Alignment.Center;
            settingsForm.AddEntry(label, control);
        }

        private DockLayout CreateBottomDock()
        {
            DockLayout bottomDock = new DockLayout()
            {
                MinYMargin = 5
            };

            Button backButton = Style.CreateTextButton(localizer.GetNoun("Back"));
            backButton.MinSize = new Size(150, 40);
            backButton.Clicked += (sender, @event) => Exited.Raise(this);
            bottomDock.Dock(backButton, Direction.NegativeX);

            startButton = Style.CreateTextButton(localizer.GetNoun("Start"));
            startButton.MinSize = new Size(150, 40);
            startButton.MinXMargin = 20;
            startButton.Clicked += (sender, @event) => MatchStarted.Raise(this);
            bottomDock.Dock(startButton, Direction.PositiveX);

            readyCheckBox = Style.CreateTextCheckBox(localizer.GetNoun("Ready"));
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
                MinWidth = 300,
                MinXMargin = 10,
                MinEntrySize = 32,
                HeaderContentGap = 10
            };
            contentDock.Dock(settingsForm, Direction.PositiveX);

            Label playersLabel = Style.CreateLabel(localizer.GetNoun("Players"));
            playersLabel.Margin = 5;
            contentDock.Dock(playersLabel, Direction.NegativeY);

            aiCreationStack = CreateAICreationStack();
            contentDock.Dock(aiCreationStack, Direction.PositiveY);

            playerStack = new StackLayout()
            {
                Direction = Direction.PositiveY,
                ChildGap = 5
            };
            contentDock.Dock(playerStack, Direction.PositiveX);

            return contentDock;
        }

        private StackLayout CreateAICreationStack()
        {
            StackLayout stack = new StackLayout()
            {
                Direction = Direction.PositiveX,
                ChildGap = 10,
                VisibilityFlag = Visibility.Hidden
            };

            Label label = Style.CreateLabel(localizer.GetNoun("Bot"));
            label.VerticalAlignment = Alignment.Center;
            stack.Stack(label);

            aiBuilderComboBox = Style.Create<ComboBox>();
            aiBuilderComboBox.VerticalAlignment = Alignment.Center;
            stack.Stack(aiBuilderComboBox);

            Button createAIButton = Style.CreateTextButton(localizer.GetNoun("Create"));
            createAIButton.Clicked += (sender, @args) => 
            {
                Label aiBuilderLabel = (Label)aiBuilderComboBox.SelectedItem;
                Action aiBuildingAction = (Action)aiBuilderLabel.Tag;
                aiBuildingAction();
            };
            stack.Stack(createAIButton);

            return stack;
        }
        #endregion
    }
}
