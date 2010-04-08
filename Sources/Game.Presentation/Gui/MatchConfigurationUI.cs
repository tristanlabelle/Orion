using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Orion.Game.Matchmaking;
using Orion.Engine.Gui;
using Orion.Engine.Geometry;
using Orion.Engine;
using OpenTK.Math;

namespace Orion.Game.Presentation.Gui
{
    public sealed class MatchConfigurationUI : MaximizedPanel
    {
        #region Fields
        private const NumberStyles integerParsingStyles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        private const float checkBoxSize = 6;

        private MatchSettings settings;
        private readonly bool isGameMaster;

        private readonly Panel backgroundPanel;
        private readonly ListPanel playersPanel;
        private readonly ListPanel optionsListPanel;
        private readonly Button startButton;
        private readonly Button exitButton;
        private readonly IEnumerable<ColorRgb> availableColors;

        private readonly Dictionary<Player, Panel> playerToRowMapping = new Dictionary<Player,Panel>();
        private readonly Dictionary<Player, DropdownList<ColorRgb>> playerToColorDropdownListMapping = new Dictionary<Player,DropdownList<ColorRgb>>();
        #endregion

        #region Constructors
        public MatchConfigurationUI(MatchSettings settings, IEnumerable<ColorRgb> availableColors)
            : this(settings, availableColors, true)
        { }

        public MatchConfigurationUI(MatchSettings settings, IEnumerable<ColorRgb> availableColors, bool isGameMaster)
        {
            this.settings = settings;
            this.availableColors = availableColors;
            this.isGameMaster = isGameMaster;

            backgroundPanel = new Panel(Bounds.TranslatedBy(10, 60).ResizedBy(-20, -70));
            Children.Add(backgroundPanel);

            exitButton = new Button(new Rectangle(10, 10, 100, 40), "Retour");
            exitButton.Triggered += (sender) => ExitPressed.Raise(this);
            Children.Add(exitButton);

            if (isGameMaster)
            {
                startButton = new Button(new Rectangle(Bounds.MaxX - 150, 10, 140, 40), "Commencer");
                startButton.Enabled = isGameMaster;
                startButton.Triggered += (sender) => StartGamePressed.Raise(this);
                Children.Add(startButton);
            }

            #region Players
            settings.PlayerJoined += OnPlayerJoined;
            settings.PlayerLeft += OnPlayerLeft;
            settings.PlayerChanged += OnPlayerColorChanged;
            playersPanel = new ListPanel(Instant.CreateComponentRectangle(Bounds, new Vector2(0.025f, 0.1f), new Vector2(0.5f, 0.9f)));
            backgroundPanel.Children.Add(playersPanel);

            foreach (Player player in settings.Players)
                AddPlayerRow(player);
            #endregion

            #region Game Options
            optionsListPanel = new ListPanel(Instant.CreateComponentRectangle(Bounds, new Vector2(0.6f, 0.1f), new Vector2(0.975f, 0.9f)));
            backgroundPanel.Children.Add(optionsListPanel);

            {
                Button optionButton = AddLabelButtonOption("Taille du terrain",
                    settings.MapSize.ToString(),
                    "Entrez la nouvelle taille désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumMapSize),
                    ValidateMapSize);
                settings.MapSizeChanged += o => optionButton.Caption = o.MapSize.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Limite de nourriture",
                    settings.FoodLimit.ToString(),
                "Entrez la nouvelle limite de nourriture désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumPopulation),
                ValidateFoodLimit);
                settings.FoodLimitChanged += o => optionButton.Caption = o.FoodLimit.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Quantité initiale d'aladdium",
                    settings.InitialAladdiumAmount.ToString(),
                "Entrez la nouvelle quantité initiale d'aladdium désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumAladdium),
                ValidateInitialAladdiumAmount);
                settings.InitialAladdiumAmountChanged += o => optionButton.Caption = o.InitialAladdiumAmount.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Quantité initiale d'alagène",
                    settings.InitialAlageneAmount.ToString(),
                "Entrez la nouvelle quantité initiale d'alagène désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumAlagene),
                ValidateInitialAlageneAmount);
                settings.InitialAlageneAmountChanged += o => optionButton.Caption = o.InitialAlageneAmount.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Germe de génération",
                    settings.RandomSeed.ToString(),
                "Entrez le nouveau germe de génération aléatoire.",
                ValidateRandomSeed);
                settings.RandomSeedChanged += o => optionButton.Caption = o.RandomSeed.ToString();
            }

            optionsListPanel.Children.Add(new Panel(RowFrame)); // separator

            {
                Checkbox checkbox = AddCheckboxOption("Révéler le terrain", value => settings.RevealTopology = value);
                checkbox.State = settings.RevealTopology;
                settings.RevealTopologyChanged += o => checkbox.State = o.RevealTopology;
            }
            {
                Checkbox checkbox = AddCheckboxOption("Début nomade", value => settings.StartNomad = value);
                checkbox.State = settings.StartNomad;
                settings.StartNomadChanged += o => checkbox.State = o.StartNomad;
            }
            {
                Checkbox checkbox = AddCheckboxOption("Codes de triche", value => settings.AreCheatsEnabled = value);
                checkbox.State = settings.AreCheatsEnabled;
                settings.AreCheatsEnabledChanged += o => checkbox.State = o.AreCheatsEnabled;
            }
            {
                Checkbox checkbox = AddCheckboxOption("Héros aléatoires", value => settings.AreRandomHeroesEnabled = value);
                checkbox.State = settings.AreRandomHeroesEnabled;
                settings.AreRandomHeroesEnabledChanged += o => checkbox.State = o.AreRandomHeroesEnabled;
            }
            #endregion
        }
        #endregion

        #region Events
        public event Action<MatchConfigurationUI> StartGamePressed;
        public event Action<MatchConfigurationUI> ExitPressed;
        public event Action<MatchConfigurationUI, MatchSettings> OptionChanged;
        #endregion

        #region Properties
        private Rectangle RowFrame
        {
            get { return Instant.CreateComponentRectangle(Bounds, new Rectangle(0.375f, 0.035f)); }
        }

        public bool IsGameMaster
        {
            get { return isGameMaster; }
        }
        #endregion

        #region Methods

        private void AddPlayerRow(Player player)
        {
            Rectangle playerRect = Instant.CreateComponentRectangle(playersPanel.Bounds, new Rectangle(1, 0.125f));
            Rectangle playerNameRect = Instant.CreateComponentRectangle(playerRect, new Rectangle(0.75f, 1));
            Rectangle colorDropdownRect = Instant.CreateComponentRectangle(playerRect, new Rectangle(0.75f, 0, 0.15f, 1));
            Rectangle kickRect = Instant.CreateComponentRectangle(playerRect, new Rectangle(0.9f, 0, 0.1f, 1));

            Panel row = new Panel(playerRect);
            Label playerName = new Label(playerNameRect, player.ToString());
            DropdownList<ColorRgb> colors = new DropdownList<ColorRgb>(colorDropdownRect, availableColors);
            colors.SelectedItem = player.Color;
            colors.Enabled = isGameMaster || player is LocalPlayer;
            colors.SelectionChanged += (dropdown, color) => player.Color = color;
            if(isGameMaster)
            {
                Button kick = new Button(kickRect, "X");
                kick.Triggered += (sender) => settings.RemovePlayer(player);
                row.Children.Add(kick);
            }
            row.Children.Add(playerName);
            row.Children.Add(colors);
            playersPanel.Children.Add(row);

            playerToRowMapping.Add(player, row);
            playerToColorDropdownListMapping.Add(player, colors);
        }

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

            settings.RandomSeed = seed;
            return true;
        }

        private bool ValidateInitialAladdiumAmount(string value)
        {
            int aladdium;
            if (!int.TryParse(value, integerParsingStyles, NumberFormatInfo.InvariantInfo, out aladdium)
                || aladdium < MatchSettings.SuggestedMinimumAladdium)
                return false;

            settings.InitialAladdiumAmount = aladdium;
            return true;
        }

        private bool ValidateInitialAlageneAmount(string value)
        {
            int alagene;
            if (!int.TryParse(value, integerParsingStyles, NumberFormatInfo.InvariantInfo, out alagene)
                || alagene < MatchSettings.SuggestedMinimumAladdium)
                return false;

            settings.InitialAlageneAmount = alagene;
            return true;
        }

        private bool ValidateFoodLimit(string value)
        {
            int maxPop;
            if (!int.TryParse(value, integerParsingStyles, NumberFormatInfo.InvariantInfo, out maxPop)
                || maxPop < MatchSettings.SuggestedMinimumPopulation)
                return false;

            settings.FoodLimit = maxPop;
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

            settings.MapSize = newSize;
            return true;
        }
        #endregion

        #region Events Handling
        private void OnPlayerJoined(MatchSettings settings, Player player)
        {
            AddPlayerRow(player);
        }

        private void OnPlayerLeft(MatchSettings settings, Player player)
        {
            playersPanel.Children.Remove(playerToRowMapping[player]);
            playerToRowMapping.Remove(player);
        }

        private void OnPlayerColorChanged(MatchSettings settings, Player player)
        {
            playerToColorDropdownListMapping[player].SelectedItem = player.Color;
        }

        private void OnOptionChanged()
        {
            OptionChanged.Raise(this, settings);
        }
        #endregion
        #endregion
    }
}
