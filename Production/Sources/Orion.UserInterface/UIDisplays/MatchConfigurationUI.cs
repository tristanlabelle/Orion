using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Geometry;
using Orion.Commandment;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public abstract class MatchConfigurationUI : UIDisplay
    {
        #region Fields
        private GenericEventHandler<Button> exitPanel;
        private GenericEventHandler<Button> startGame;
        protected Button startButton;
        protected Button exitButton;

        protected Random random;
        protected DropdownList<PlayerSlot>[] playerSlots = new DropdownList<PlayerSlot>[8];
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
            startButton = new Button(new Rectangle(Bounds.MaxX - 110, 10, 100, 40), "Commencer");
            startButton.Enabled = enableStartGame;
            Children.Add(startButton);
        }
        #endregion

        #region Events
        public event GenericEventHandler<MatchConfigurationUI> PressedStartGame;
        public event GenericEventHandler<MatchConfigurationUI> PressedExit;
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
