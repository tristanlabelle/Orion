using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Orion.Geometry;
using Orion.Commandment;
using Orion.UserInterface.Widgets;
using Orion.GameLogic;
using Orion.Graphics;
using OpenTK.Math;

namespace Orion.UserInterface
{
    public abstract class MatchConfigurationUI : UIDisplay
    {
        private static readonly Size minSize = new Size(64, 64);
        #region Fields
        private GenericEventHandler<Button> exitPanel;
        private GenericEventHandler<Button> startGame;
        private Size size = new Size(128, 128);
        private readonly Label sizeField;
        protected readonly Button sizeChangeButton;
        protected readonly Button startButton;
        protected readonly Button exitButton;

        protected Random random;
        protected DropdownList<PlayerSlot>[] playerSlots
            = new DropdownList<PlayerSlot>[Faction.Colors.Length];
        protected Frame backgroundFrame;
        #endregion

        #region Constructors
        public MatchConfigurationUI()
            : this(true)
        { }

        public MatchConfigurationUI(bool enableStartGame)
        {
            backgroundFrame = new Frame(Bounds.TranslatedBy(10, 60).ResizedBy(-20, -70));
            Children.Add(backgroundFrame);
            Rectangle dropdownListRect = new Rectangle(10, backgroundFrame.Bounds.MaxY - 40, 200, 30);
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i] = new DropdownList<PlayerSlot>(dropdownListRect);
                dropdownListRect = dropdownListRect.TranslatedBy(0, -40);
                backgroundFrame.Children.Add(playerSlots[i]);
            }

            exitPanel = OnPressedExit;
            exitButton = new Button(new Rectangle(10, 10, 100, 40), "Retour");
            Children.Add(exitButton);
            startGame = OnPressedStartGame;
            startButton = new Button(new Rectangle(Bounds.MaxX - 150, 10, 140, 40), "Commencer");
            startButton.Enabled = enableStartGame;
            Children.Add(startButton);

            // size
            Rectangle sizeRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.9f, 0.1f), new Vector2(0.975f, 0.135f));
            Rectangle changeRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.675f, 0.1f), new Vector2(0.875f, 0.135f));
            sizeField = new Label(sizeRect, "{0}x{1}".FormatInvariant(size.Width, size.Height));
            sizeChangeButton = new Button(changeRect, "Changer la taille");
            sizeChangeButton.Triggered += button => Instant.Prompt(this, "Entrez la nouvelle taille désirée (minimum {0}x{1}).".FormatInvariant(minSize.Width, minSize.Height), "{0}x{1}".FormatInvariant(size.Width, size.Height), ParseSize);
            Children.Add(sizeField);
            Children.Add(sizeChangeButton);
        }
        #endregion

        #region Events
        public event GenericEventHandler<MatchConfigurationUI> PressedStartGame;
        public event GenericEventHandler<MatchConfigurationUI> PressedExit;
        public event GenericEventHandler<MatchConfigurationUI, Size> SizeChanged;
        #endregion

        #region Properties
        public Size MapSize
        {
            get { return size; }
            set
            {
                size = value;
                sizeField.Text = new Text("{0}x{1}".FormatInvariant(size.Width, size.Height));
                if (SizeChanged != null)
                    SizeChanged(this, size);
            }
        }

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

        protected virtual void OnPressedExit(Button button)
        {
            GenericEventHandler<MatchConfigurationUI> handler = PressedExit;
            if (handler != null) handler(this);
            Parent.PopDisplay(this);
        }

        protected virtual void OnPressedStartGame(Button button)
        {
            GenericEventHandler<MatchConfigurationUI> handler = PressedStartGame;
            if (handler != null) handler(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PressedStartGame = null;
                PressedExit = null;
            }

            base.Dispose(disposing);
        }

        private void ParseSize(string sizeString)
        {
            if (sizeString == null) return;

            Regex regex = new Regex("([0-9]+)x([0-9]+)", RegexOptions.IgnoreCase);
            if (regex.IsMatch(sizeString))
            {
                System.Text.RegularExpressions.Match result = regex.Match(sizeString);
                Size newSize = new Size(Int32.Parse(result.Groups[1].Value), Int32.Parse(result.Groups[2].Value));
                if (newSize.Width < minSize.Width || newSize.Height < minSize.Height)
                {
                    MapSize = new Size(Int32.Parse(result.Groups[1].Value), Int32.Parse(result.Groups[2].Value));
                    return;
                }
            }
            Instant.Prompt(this, "La taille entrée est invalide. Entrez la nouvelle taille désirée (minimum {0}x{1}).".FormatInvariant(minSize.Width, minSize.Height), "{0}x{1}".FormatInvariant(size.Width, size.Height), ParseSize);
        }

        #region UIDisplay Implementation
        internal override void OnEnter(RootView enterOn)
        {
            InitializeSlots();
            exitButton.Triggered += exitPanel;
            startButton.Triggered += startGame;
            base.OnEnter(enterOn);
        }

        internal override void OnShadow(RootView hiddenOf)
        {
            exitButton.Triggered -= exitPanel;
            startButton.Triggered -= startGame;
            foreach (DropdownList<PlayerSlot> list in playerSlots) list.Dispose();
            base.OnShadow(hiddenOf);
        }
        #endregion
        #endregion
    }
}
