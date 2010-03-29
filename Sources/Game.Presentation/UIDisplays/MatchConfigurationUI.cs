using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Simulation;
using Orion.Game.Presentation;
using Orion.Game.Matchmaking;

namespace Orion.Game.Presentation
{
    public abstract class MatchConfigurationUI : UIDisplay
    {
        #region Fields
        private static readonly NumberStyles integerParsingStyles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        private const float checkBoxSize = 6;

        private Action<Button> exitPanel;
        private Action<Button> startGame;
        private Size size = new Size(150, 150);

        protected MatchSettings options;
        private readonly ListPanel optionsListPanel;
        private readonly Button startButton;
        private readonly Button exitButton;

        protected readonly DropdownList<PlayerSlot>[] playerSlots;
        protected readonly Panel backgroundPanel;
        private readonly bool isGameMaster;
        #endregion

        #region Constructors
        public MatchConfigurationUI(MatchSettings options)
            : this(options, true)
        { }

        public MatchConfigurationUI(MatchSettings options, bool isGameMaster)
        {
            this.isGameMaster = isGameMaster;
            this.options = options;

            backgroundPanel = new Panel(Bounds.TranslatedBy(10, 60).ResizedBy(-20, -70));
            Children.Add(backgroundPanel);
            Rectangle dropdownListRect = new Rectangle(10, backgroundPanel.Bounds.MaxY - 40, 200, 30);

            playerSlots = new DropdownList<PlayerSlot>[Faction.Colors.Length];
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i] = new DropdownList<PlayerSlot>(dropdownListRect);
                playerSlots[i].TextColor = Faction.Colors[i];
                dropdownListRect = dropdownListRect.TranslatedBy(0, -40);
                backgroundPanel.Children.Add(playerSlots[i]);
            }

            exitPanel = OnPressedExit;
            exitButton = new Button(new Rectangle(10, 10, 100, 40), "Retour");
            Children.Add(exitButton);
            startGame = OnPressedStartGame;
            startButton = new Button(new Rectangle(Bounds.MaxX - 150, 10, 140, 40), "Commencer");
            startButton.Enabled = isGameMaster;
            Children.Add(startButton);

            #region Game Options
            optionsListPanel = new ListPanel(Instant.CreateComponentRectangle(Bounds, new Vector2(0.6f, 0.1f), new Vector2(0.975f, 0.9f)));
            Children.Add(optionsListPanel);

            {
                Button optionButton = AddLabelButtonOption("Taille du terrain",
                    options.MapSize.ToString(),
                    "Entrez la nouvelle taille désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumMapSize),
                    ValidateMapSize);
                options.MapSizeChanged += o => optionButton.Caption = o.MapSize.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Limite de nourriture",
                    options.MaximumPopulation.ToString(),
                "Entrez la nouvelle limite de nourriture désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumPopulation),
                ValidateFoodLimit);
                options.MaximumPopulationChanged += o => optionButton.Caption = o.MaximumPopulation.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Quantité initiale d'aladdium",
                    options.InitialAladdiumAmount.ToString(),
                "Entrez la nouvelle quantité initiale d'aladdium désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumAladdium),
                ValidateInitialAladdiumAmount);
                options.InitialAladdiumAmountChanged += o => optionButton.Caption = o.InitialAladdiumAmount.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Quantité initiale d'alagène",
                    options.InitialAlageneAmount.ToString(),
                "Entrez la nouvelle quantité initiale d'alagène désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumAlagene),
                ValidateInitialAlageneAmount);
                options.InitialAlageneAmountChanged += o => optionButton.Caption = o.InitialAlageneAmount.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Germe de génération",
                    options.RandomSeed.ToString(),
                "Entrez le nouveau germe de génération aléatoire.",
                ValidateRandomSeed);
                options.RandomSeedChanged += o => optionButton.Caption = o.RandomSeed.ToString();
            }

            optionsListPanel.Children.Add(new Panel(RowFrame)); // separator

            #region Reveal Topology
            {
                Checkbox checkbox = AddCheckboxOption("Révéler le terrain", value => options.RevealTopology = value);
                options.RevealTopologyChanged += o => checkbox.State = o.RevealTopology;
            }
            #endregion

            #region Reveal Topology
            {
                Checkbox checkbox = AddCheckboxOption("Début nomade", value => options.IsNomad = value);
                options.IsNomadChanged += o => checkbox.State = o.IsNomad;
            }
            #endregion
            #endregion
        }
        #endregion

        #region Events
        public event Action<MatchConfigurationUI> PressedStartGame;
        public event Action<MatchConfigurationUI> PressedExit;
        public event Action<MatchConfigurationUI, MatchSettings> OptionChanged;
        #endregion

        #region Properties
        public IEnumerable<PlayerSlot> Players
        {
            get { return playerSlots.Select(list => list.SelectedItem); }
        }

        public int NumberOfPlayers
        {
            get { return Players.Where(slot => slot.NeedsFaction).Count(); }
        }

        public int MaxNumberOfPlayers
        {
            get { return playerSlots.Length; }
        }

        public int NextAvailableSlot
        {
            get
            {
                RemotePlayerSlot firstEmpty = Players.OfType<RemotePlayerSlot>().Where(slot => !slot.NeedsFaction).First();
                DropdownList<PlayerSlot> emptySlot = playerSlots.First(list => list.SelectedItem == firstEmpty);
                return playerSlots.IndexOf(emptySlot);
            }
        }

        public new RootView Root
        {
            get { return (RootView)base.Root; }
        }

        public bool IsGameMaster
        {
            get { return isGameMaster; }
        }

        private Rectangle RowFrame
        {
            get { return Instant.CreateComponentRectangle(Bounds, new Rectangle(0.375f, 0.035f)); }
        }
        #endregion

        #region Methods
        protected abstract void InitializeSlots();

        #region Initialization
        private Button AddLabelButtonOption(string description, string initialValue, string prompt, Func<string, bool> validator)
        {
            Rectangle optionRect = Instant.CreateComponentRectangle(RowFrame, new Rectangle(0, 0, 0.70f, 1));
            Rectangle valueRect = Instant.CreateComponentRectangle(RowFrame, new Rectangle(0.70f, 0, 0.3f, 1));

            Panel optionPanel = new Panel(RowFrame);
            Label descriptionLabel = new Label(optionRect, description + ':');
            Button changeButton = new Button(valueRect, initialValue);
            changeButton.Enabled = isGameMaster;

            optionPanel.Children.Add(descriptionLabel);
            optionPanel.Children.Add(changeButton);
            optionsListPanel.Children.Add(optionPanel);

            changeButton.Triggered +=
                sender => Validate(prompt, changeButton.Caption, validator);

            return changeButton;
        }

        private Checkbox AddCheckboxOption(string text, Action<bool> changedHandler)
        {
            Rectangle checkboxFrame = new Rectangle(
                checkBoxSize * 0.5f, checkBoxSize * 0.5f,
                RowFrame.Height - checkBoxSize, RowFrame.Height - checkBoxSize);
            Rectangle checkboxLabelFrame = new Rectangle(
                checkboxFrame.MaxX + checkBoxSize, 0,
                RowFrame.Width - checkboxFrame.MaxX - checkBoxSize, RowFrame.Height);

            Panel optionPanel = new Panel(RowFrame);
            optionPanel.Children.Add(new Label(checkboxLabelFrame, text));

            Checkbox checkbox = new Checkbox(checkboxFrame);
            checkbox.Enabled = isGameMaster;

            optionPanel.Children.Add(checkbox);
            optionsListPanel.Children.Add(optionPanel);

            optionPanel.MouseButtonPressed += (row, args) => checkbox.Trigger();

            checkbox.StateChanged += (sender, value) => changedHandler(value);

            return checkbox;
        }
        #endregion

        #region Events Handling
        protected virtual void OnOptionChanged()
        {
            OptionChanged.Raise(this, options);
        }

        protected virtual void OnPressedExit(Button button)
        {
            PressedExit.Raise(this);
            Parent.PopDisplay(this);
        }

        protected virtual void OnPressedStartGame(Button button)
        {
            PressedStartGame.Raise(this);
        }
        #endregion

        #region Options Validation
        private void Validate(string prompt, string initialValue, Func<string, bool> validator)
        {
            Instant.Prompt(this, prompt, initialValue, result =>
            {
                if (validator(result))
                    OnOptionChanged();
                else
                    Validate(prompt, initialValue, validator);
            });
        }

        private bool ValidateRandomSeed(string value)
        {
            int seed;
            if (!int.TryParse(value, integerParsingStyles, NumberFormatInfo.InvariantInfo, out seed))
                return false;

            options.RandomSeed = seed;
            return true;
        }

        private bool ValidateInitialAladdiumAmount(string value)
        {
            int aladdium;
            if (!int.TryParse(value, integerParsingStyles, NumberFormatInfo.InvariantInfo, out aladdium)
                || aladdium < MatchSettings.SuggestedMinimumAladdium)
                return false;

            options.InitialAladdiumAmount = aladdium;
            return true;
        }

        private bool ValidateInitialAlageneAmount(string value)
        {
            int alagene;
            if (!int.TryParse(value, integerParsingStyles, NumberFormatInfo.InvariantInfo, out alagene)
                || alagene < MatchSettings.SuggestedMinimumAladdium)
                return false;

            options.InitialAlageneAmount = alagene;
            return true;
        }

        private bool ValidateFoodLimit(string value)
        {
            int maxPop;
            if (!int.TryParse(value, integerParsingStyles, NumberFormatInfo.InvariantInfo, out maxPop)
                || maxPop < MatchSettings.SuggestedMinimumPopulation)
                return false;

            options.MaximumPopulation = maxPop;
            return true;
        }

        private bool ValidateMapSize(string sizeString)
        {
            Size newSize;
            if (!Size.TryParse(sizeString, out newSize))
                return false;

            if (newSize.Width < MatchSettings.SuggestedMinimumMapSize.Width
                || newSize.Height < MatchSettings.SuggestedMinimumMapSize.Height)
                return false;

            options.MapSize = newSize;
            return true;
        }
        #endregion

        #region UIDisplay Implementation
        protected override void OnEntered()
        {
            InitializeSlots();
            exitButton.Triggered += exitPanel;
            startButton.Triggered += startGame;
            base.OnEntered();
        }

        protected override void OnShadowed()
        {
            exitButton.Triggered -= exitPanel;
            startButton.Triggered -= startGame;
            foreach (DropdownList<PlayerSlot> list in playerSlots) list.Dispose();
            base.OnShadowed();
        }
        #endregion

        #region Object Model
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PressedStartGame = null;
                PressedExit = null;
            }

            base.Dispose(disposing);
        }
        #endregion
        #endregion
    }
}
