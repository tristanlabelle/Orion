using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Orion.Game.Matchmaking;
using Orion.Engine.Collections;
using Orion.Engine.Gui;
using Orion.Engine.Geometry;
using Orion.Engine;
using OpenTK.Math;
using Orion.Game.Simulation;

namespace Orion.Game.Presentation.Gui
{
    public sealed class MatchConfigurationUI : MaximizedPanel
    {
        #region Fields
        private const NumberStyles integerParsingStyles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        private const float checkBoxSize = 6;

        private readonly MatchSettings matchSettings;
        private readonly PlayerSettings playerSettings;
        private readonly bool isGameMaster;

        private readonly Panel backgroundPanel;
        private readonly ListPanel playersPanel;
        private readonly DropdownList<PlayerBuilder> addPlayerDropdownList;
        private readonly ListPanel optionsListPanel;
        private readonly Button startButton;
        private readonly Button exitButton;

        private readonly Dictionary<Player, Panel> playerToRowMapping = new Dictionary<Player,Panel>();
        private readonly Dictionary<Player, DropdownList<ColorRgb>> playerToColorDropdownListMapping
            = new Dictionary<Player,DropdownList<ColorRgb>>();
        #endregion

        #region Constructors
        public MatchConfigurationUI(MatchSettings matchSettings, PlayerSettings playerSettings,
            IEnumerable<PlayerBuilder> playerBuilders)
            : this(matchSettings, playerSettings, playerBuilders, true)
        { }

        public MatchConfigurationUI(MatchSettings matchSettings, PlayerSettings playerSettings,
            IEnumerable<PlayerBuilder> playerBuilders, bool isGameMaster)
        {
            this.matchSettings = matchSettings;
            this.playerSettings = playerSettings;
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
            playerSettings.PlayerJoined += OnPlayerJoined;
            playerSettings.PlayerLeft += OnPlayerLeft;
            playerSettings.PlayerChanged += OnPlayerColorChanged;
            playersPanel = new ListPanel(Instant.CreateComponentRectangle(Bounds, new Vector2(0.025f, 0.1f), new Vector2(0.5f, 0.9f)));
            backgroundPanel.Children.Add(playersPanel);

            foreach (Player player in playerSettings.Players)
                AddPlayerRow(player);

            if (playerBuilders.Count() > 0)
            {
                Rectangle dropdownListRectangle = Instant.CreateComponentRectangle(playersPanel.Frame, new Rectangle(0.3f, 0, 0.45f, 0.0375f));
                Rectangle addButtonRect = Instant.CreateComponentRectangle(playersPanel.Frame, new Rectangle(0.75f, 0, 0.25f, 0.0375f));
                dropdownListRectangle = dropdownListRectangle.TranslatedBy(0, -dropdownListRectangle.Height);
                addButtonRect = addButtonRect.TranslatedBy(0, -addButtonRect.Height);
                addPlayerDropdownList = new DropdownList<PlayerBuilder>(dropdownListRectangle, playerBuilders);
                Button addPlayerButton = new Button(addButtonRect, "Ajouter");
                addPlayerButton.Triggered += OnAddPlayerButtonPressed;
                backgroundPanel.Children.Add(addPlayerDropdownList);
                backgroundPanel.Children.Add(addPlayerButton);
            }
            #endregion

            #region Game Options
            optionsListPanel = new ListPanel(Instant.CreateComponentRectangle(Bounds, new Vector2(0.6f, 0.1f), new Vector2(0.975f, 0.9f)));
            backgroundPanel.Children.Add(optionsListPanel);

            {
                Button optionButton = AddLabelButtonOption("Taille du terrain",
                    matchSettings.MapSize.ToString(),
                    "Entrez la nouvelle taille désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumMapSize),
                    ValidateMapSize);
                matchSettings.MapSizeChanged += o => optionButton.Caption = o.MapSize.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Limite de nourriture",
                    matchSettings.FoodLimit.ToString(),
                "Entrez la nouvelle limite de nourriture désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumPopulation),
                ValidateFoodLimit);
                matchSettings.FoodLimitChanged += o => optionButton.Caption = o.FoodLimit.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Quantité initiale d'aladdium",
                    matchSettings.InitialAladdiumAmount.ToString(),
                "Entrez la nouvelle quantité initiale d'aladdium désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumAladdium),
                ValidateInitialAladdiumAmount);
                matchSettings.InitialAladdiumAmountChanged += o => optionButton.Caption = o.InitialAladdiumAmount.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Quantité initiale d'alagène",
                    matchSettings.InitialAlageneAmount.ToString(),
                "Entrez la nouvelle quantité initiale d'alagène désirée (minimum {0}).".FormatInvariant(MatchSettings.SuggestedMinimumAlagene),
                ValidateInitialAlageneAmount);
                matchSettings.InitialAlageneAmountChanged += o => optionButton.Caption = o.InitialAlageneAmount.ToString();
            }
            {
                Button optionButton = AddLabelButtonOption("Germe de génération",
                    matchSettings.RandomSeed.ToString(),
                "Entrez le nouveau germe de génération aléatoire.",
                ValidateRandomSeed);
                matchSettings.RandomSeedChanged += o => optionButton.Caption = o.RandomSeed.ToString();
            }

            optionsListPanel.Children.Add(new Panel(RowFrame)); // separator

            {
                Checkbox checkbox = AddCheckboxOption("Révéler le terrain", value => matchSettings.RevealTopology = value);
                checkbox.State = matchSettings.RevealTopology;
                matchSettings.RevealTopologyChanged += o => checkbox.State = o.RevealTopology;
            }
            {
                Checkbox checkbox = AddCheckboxOption("Début nomade", value => matchSettings.StartNomad = value);
                checkbox.State = matchSettings.StartNomad;
                matchSettings.StartNomadChanged += o => checkbox.State = o.StartNomad;
            }
            {
                Checkbox checkbox = AddCheckboxOption("Codes de triche", value => matchSettings.AreCheatsEnabled = value);
                checkbox.State = matchSettings.AreCheatsEnabled;
                matchSettings.AreCheatsEnabledChanged += o => checkbox.State = o.AreCheatsEnabled;
            }
            {
                Checkbox checkbox = AddCheckboxOption("Héros aléatoires", value => matchSettings.AreRandomHeroesEnabled = value);
                checkbox.State = matchSettings.AreRandomHeroesEnabled;
                matchSettings.AreRandomHeroesEnabledChanged += o => checkbox.State = o.AreRandomHeroesEnabled;
            }
            #endregion
        }
        #endregion

        #region Events
        public event Action<MatchConfigurationUI> StartGamePressed;
        public event Action<MatchConfigurationUI> ExitPressed;
        public event Action<MatchConfigurationUI, MatchSettings> OptionChanged;
        public event Action<MatchConfigurationUI, Player, ColorRgb> PlayerColorChanged;
        public event Action<MatchConfigurationUI, Player> AddPlayerPressed;
        public event Action<MatchConfigurationUI, Player> KickPlayerPressed;
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
            Rectangle playerRect = Instant.CreateComponentRectangle(playersPanel.Bounds, new Rectangle(1, 0.0375f));
            Rectangle playerNameRect = Instant.CreateComponentRectangle(playerRect, new Rectangle(0.7f, 1));
            Rectangle colorDropdownRect = Instant.CreateComponentRectangle(playerRect, new Rectangle(0.7f, 0, 0.2f, 1));
            Rectangle kickRect = Instant.CreateComponentRectangle(playerRect, new Rectangle(0.9f, 0, 0.1f, 1));

            Panel row = new Panel(playerRect);
            string playerNameLabelText = player is LocalPlayer ? "Vous" : player.Name;
            Label playerNameLabel = new Label(playerNameRect, playerNameLabelText);
            DropdownList<ColorRgb> colorsDropdownList = new DropdownList<ColorRgb>(colorDropdownRect, playerSettings.AvailableColors);
            colorsDropdownList.StringConverter = (color) => Colors.GetName(color);
            colorsDropdownList.SelectedItem = player.Color;
            colorsDropdownList.Enabled = isGameMaster || player is LocalPlayer;
            colorsDropdownList.SelectionChanged += (dropdown, color) => PlayerColorChanged.Raise(this, player, color);
            if(isGameMaster && !(player is LocalPlayer))
            {
                Button kick = new Button(kickRect, "X");
                kick.Triggered += sender => KickPlayerPressed.Raise(this, player);
                row.Children.Add(kick);
            }
            row.Children.Add(playerNameLabel);
            row.Children.Add(colorsDropdownList);
            playersPanel.Children.Add(row);

            playerToRowMapping.Add(player, row);
            playerToColorDropdownListMapping.Add(player, colorsDropdownList);
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

            matchSettings.RandomSeed = seed;
            return true;
        }

        private bool ValidateInitialAladdiumAmount(string value)
        {
            int aladdium;
            if (!int.TryParse(value, integerParsingStyles, NumberFormatInfo.InvariantInfo, out aladdium)
                || aladdium < MatchSettings.SuggestedMinimumAladdium)
                return false;

            matchSettings.InitialAladdiumAmount = aladdium;
            return true;
        }

        private bool ValidateInitialAlageneAmount(string value)
        {
            int alagene;
            if (!int.TryParse(value, integerParsingStyles, NumberFormatInfo.InvariantInfo, out alagene)
                || alagene < MatchSettings.SuggestedMinimumAladdium)
                return false;

            matchSettings.InitialAlageneAmount = alagene;
            return true;
        }

        private bool ValidateFoodLimit(string value)
        {
            int maxPop;
            if (!int.TryParse(value, integerParsingStyles, NumberFormatInfo.InvariantInfo, out maxPop)
                || maxPop < MatchSettings.SuggestedMinimumPopulation)
                return false;

            matchSettings.FoodLimit = maxPop;
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

            matchSettings.MapSize = newSize;
            return true;
        }
        #endregion

        #region Events Handling
        private void OnAddPlayerButtonPressed(Button sender)
        {
            PlayerBuilder playerBuilder = addPlayerDropdownList.SelectedItem;

            if (playerSettings.AvailableColors.Count() == 0) return;
            Player player = playerBuilder.Build(playerSettings.AvailableColors.First());
            AddPlayerPressed.Raise(this, player);
        }

        private void OnPlayerJoined(PlayerSettings settings, Player player)
        {
            AddPlayerRow(player);
        }

        private void OnPlayerLeft(PlayerSettings settings, Player player, int index)
        {
            playersPanel.Children.Remove(playerToRowMapping[player]);
            playerToRowMapping.Remove(player);
        }

        private void OnPlayerColorChanged(PlayerSettings settings, Player player, int index)
        {
            playerToColorDropdownListMapping[player].SelectedItem = player.Color;
        }

        private void OnOptionChanged()
        {
            OptionChanged.Raise(this, matchSettings);
        }
        #endregion
        #endregion
    }
}
