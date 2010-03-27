using System;
using System.Collections.Generic;
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
        private Action<Button> exitPanel;
        private Action<Button> startGame;
        private Size size = new Size(150, 150);
        private readonly Label sizeField;

        protected MatchOptions options;
        protected readonly Button sizeChangeButton;
        protected readonly Button startButton;
        protected readonly Button exitButton;

        protected Random random;
        protected DropdownList<PlayerSlot>[] playerSlots
            = new DropdownList<PlayerSlot>[Faction.Colors.Length];
        protected Frame backgroundFrame;
        #endregion

        #region Constructors
        public MatchConfigurationUI(MatchOptions options)
            : this(options, true)
        { }

        public MatchConfigurationUI(MatchOptions options, bool isGameMaster)
        {
            this.options = options;

            backgroundFrame = new Frame(Bounds.TranslatedBy(10, 60).ResizedBy(-20, -70));
            Children.Add(backgroundFrame);
            Rectangle dropdownListRect = new Rectangle(10, backgroundFrame.Bounds.MaxY - 40, 200, 30);
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i] = new DropdownList<PlayerSlot>(dropdownListRect);
                playerSlots[i].TextColor = Faction.Colors[i];
                dropdownListRect = dropdownListRect.TranslatedBy(0, -40);
                backgroundFrame.Children.Add(playerSlots[i]);
            }

            exitPanel = OnPressedExit;
            exitButton = new Button(new Rectangle(10, 10, 100, 40), "Retour");
            Children.Add(exitButton);
            startGame = OnPressedStartGame;
            startButton = new Button(new Rectangle(Bounds.MaxX - 150, 10, 140, 40), "Commencer");
            startButton.Enabled = isGameMaster;
            Children.Add(startButton);

            #region Game Options
            ListFrame optionFrame = new ListFrame(Instant.CreateComponentRectangle(Bounds, new Vector2(0.6f, 0.1f), new Vector2(0.975f, 0.9f)));
            Rectangle rowFrame = Instant.CreateComponentRectangle(Bounds, new Rectangle(0.375f, 0.035f));
            Rectangle optionRect = Instant.CreateComponentRectangle(rowFrame, new Rectangle(0, 0, 0.70f, 1));
            Rectangle valueRect = Instant.CreateComponentRectangle(rowFrame, new Rectangle(0.70f, 0, 0.3f, 1));
            Children.Add(optionFrame);

            #region Map Size
            {
                Frame sizeOption = new Frame(rowFrame);
                sizeOption.Children.Add(new Label(optionRect, "Taille du terrain:"));
                Button changeSizeOption = new Button(valueRect, options.MapSize.ToString());
                changeSizeOption.Enabled = isGameMaster;
                string prompt = "Entrez la nouvelle taille désirée (minimum {0}).".FormatInvariant(MatchOptions.SuggestedMinimumMapSize);
                changeSizeOption.Triggered +=
                    b => Validate(prompt, options.MapSize, ValidateMapSize, () => changeSizeOption.Caption = options.MapSize.ToString());
                sizeOption.Children.Add(changeSizeOption);
                optionFrame.Children.Add(sizeOption);

                options.MapSizeChanged += o => changeSizeOption.Caption = o.MapSize.ToString();
            }
            #endregion

            #region Maximum Population
            {
                Frame populationOption = new Frame(rowFrame);
                populationOption.Children.Add(new Label(optionRect, "Population maximale:"));
                Button changeMaxPop = new Button(valueRect, options.MaximumPopulation.ToString());
                changeMaxPop.Enabled = isGameMaster;
                string prompt = "Entrez la nouvelle population maximale désirée (minimum {0})".FormatInvariant(MatchOptions.SuggestedMinimumPopulation);
                changeMaxPop.Triggered +=
                    b => Validate(prompt, options.MaximumPopulation, ValidateMaximumPopulation, () => changeMaxPop.Caption = options.MaximumPopulation.ToString());
                populationOption.Children.Add(changeMaxPop);
                optionFrame.Children.Add(populationOption);

                options.MaximumPopulationChanged += o => changeMaxPop.Caption = o.MaximumPopulation.ToString();
            }
            #endregion

            #region Initial Aladdium Amount
            {
                Frame aladdiumOption = new Frame(rowFrame);
                aladdiumOption.Children.Add(new Label(optionRect, "Quantité initiale d'aladdium:"));
                Button changeAladdiumOption = new Button(valueRect, options.InitialAladdiumAmount.ToString());
                changeAladdiumOption.Enabled = isGameMaster;
                string prompt = "Entrez la nouvelle quantité d'aladdium désirée (minimum {0}).".FormatInvariant(MatchOptions.SuggestedMinimumAladdium);
                changeAladdiumOption.Triggered +=
                    b => Validate(prompt, options.InitialAladdiumAmount, ValidateInitialAladdium, () => changeAladdiumOption.Caption = options.InitialAladdiumAmount.ToString());
                aladdiumOption.Children.Add(changeAladdiumOption);
                optionFrame.Children.Add(aladdiumOption);

                options.InitialAladdiumAmountChanged += o => changeAladdiumOption.Caption = o.InitialAladdiumAmount.ToString();
            }
            #endregion

            #region Initial Alagene Amount
            {
                Frame alageneOption = new Frame(rowFrame);
                alageneOption.Children.Add(new Label(optionRect, "Quantité initiale d'alagène:"));
                Button changeAlageneOption = new Button(valueRect, options.InitialAlageneAmount.ToString());
                changeAlageneOption.Enabled = isGameMaster;
                string prompt = "Entrez la nouvelle quantité d'alagène désirée (minimum {0}).".FormatInvariant(MatchOptions.SuggestedMinimumAlagene);
                changeAlageneOption.Triggered +=
                    b => Validate(prompt, options.InitialAlageneAmount, ValidateInitialAlagene, () => changeAlageneOption.Caption = options.InitialAlageneAmount.ToString());
                alageneOption.Children.Add(changeAlageneOption);
                optionFrame.Children.Add(alageneOption);

                options.InitialAlageneAmountChanged += o => changeAlageneOption.Caption = o.InitialAlageneAmount.ToString();
            }
            #endregion

            #region Seed
            {
                Frame seedOption = new Frame(rowFrame);
                seedOption.Children.Add(new Label(optionRect, "Germe de génération:"));
                Button changeSeedOption = new Button(valueRect, options.Seed.ToString());
                changeSeedOption.Enabled = isGameMaster;
                string prompt = "Entrez le nouveau germe de génération aléatoire.";
                changeSeedOption.Triggered +=
                    b => Validate(prompt, options.Seed, ValidateSeed, () => changeSeedOption.Caption = options.InitialAlageneAmount.ToString());
                seedOption.Children.Add(changeSeedOption);
                optionFrame.Children.Add(seedOption);

                options.SeedChanged += o => changeSeedOption.Caption = o.Seed.ToString();
            }
            #endregion
            #endregion
        }
        #endregion

        #region Events
        public event Action<MatchConfigurationUI> PressedStartGame;
        public event Action<MatchConfigurationUI> PressedExit;
        public event Action<MatchConfigurationUI, MatchOptions> OptionChanged;
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

        public Random RandomGenerator
        {
            get { return random; }
        }

        public new RootView Root
        {
            get { return (RootView)base.Root; }
        }
        #endregion

        #region Methods

        protected abstract void InitializeSlots();

        #region Events Handling
        protected virtual void OnOptionChanged()
        {
            var handler = OptionChanged;
            if (handler != null) handler(this, options);
        }

        protected virtual void OnPressedExit(Button button)
        {
            Action<MatchConfigurationUI> handler = PressedExit;
            if (handler != null) handler(this);
            Parent.PopDisplay(this);
        }

        protected virtual void OnPressedStartGame(Button button)
        {
            Action<MatchConfigurationUI> handler = PressedStartGame;
            if (handler != null) handler(this);
        }
        #endregion

        #region Options Validation
        private void Validate<TValidatedType>(string prompt, TValidatedType defaultValue, Func<string, bool> validator, Action onValidate)
        {
            Instant.Prompt(this, prompt, defaultValue.ToString(), result =>
            {
                if (validator(result))
                {
                    onValidate();
                    OnOptionChanged();
                }
                else
                    Validate(prompt, defaultValue, validator, onValidate);
            });
        }

        private bool ValidateSeed(string seedString)
        {
            int seed;
            if (!int.TryParse(seedString, out seed))
                return false;

            options.Seed = seed;
            return true;
        }

        private bool ValidateInitialAladdium(string aladdiumString)
        {
            int aladdium;
            if (!int.TryParse(aladdiumString, out aladdium) || aladdium < MatchOptions.SuggestedMinimumAladdium)
                return false;

            options.InitialAladdiumAmount = aladdium;
            return true;
        }

        private bool ValidateInitialAlagene(string alageneAmount)
        {
            int alagene;
            if (!int.TryParse(alageneAmount, out alagene) || alagene < MatchOptions.SuggestedMinimumAladdium)
                return false;

            options.InitialAlageneAmount = alagene;
            return true;
        }

        private bool ValidateMaximumPopulation(string maxPopString)
        {
            int maxPop;
            if (!int.TryParse(maxPopString, out maxPop) || maxPop < MatchOptions.SuggestedMinimumPopulation)
                return false;

            options.MaximumPopulation = maxPop;
            return true;
        }

        private bool ValidateMapSize(string sizeString)
        {
            Size newSize;
            if (!Size.TryParse(sizeString, out newSize))
                return false;

            if (newSize.Width < MatchOptions.SuggestedMinimumMapSize.Width || newSize.Height < MatchOptions.SuggestedMinimumMapSize.Height)
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
