using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Gui;
using Orion.Engine.Geometry;
using Orion.Game.Simulation;
using OpenTK.Math;

namespace Orion.Game.Presentation.Gui
{
    public sealed class DiplomacyPanel : Panel
    {
        #region Nested Types
        private sealed class FactionPanel : Panel
        {
            #region Fields
            private readonly Faction faction;
            private readonly Checkbox alliedCheckbox;
            #endregion

            #region Constructors
            public FactionPanel(Rectangle frame, Faction faction)
                : base(frame, faction.Color)
            {
                Argument.EnsureNotNull(faction, "faction");

                this.faction = faction;

                Children.Add(new Label(faction.Name));

                Rectangle checkboxFrame = Instant.CreateComponentRectangle(Bounds, new Rectangle(0.95f, 0, 0.05f, 1));
                this.alliedCheckbox = new Checkbox(checkboxFrame, false);
                Children.Add(alliedCheckbox);

                Children.Add(new Label(Instant.CreateComponentRectangle(Bounds, new Rectangle(0.87f, 0, 1, 1)), "Allié"));
            }
            #endregion

            #region Properties
            public Faction Faction
            {
                get { return faction; }
            }

            public bool IsAlliedChecked
            {
                get { return alliedCheckbox.IsChecked; }
                set { alliedCheckbox.IsChecked = value; }
            }
            #endregion
        }
        #endregion

        #region Fields
        private readonly UserInputManager userInputManager;
        private readonly ListPanel factionListPanel;
        private bool isDirty;
        #endregion

        #region Constructors
        public DiplomacyPanel(Rectangle frame, UserInputManager userInputManager)
            : base(frame)
        {
            Argument.EnsureNotNull(userInputManager, "userInputManager");

            this.userInputManager = userInputManager;

            Rectangle listPanelFrame = Instant.CreateComponentRectangle(Bounds, new Rectangle(0, 0.1f, 1, 0.9f));
            this.factionListPanel = new ListPanel(listPanelFrame, Vector2.Zero);
            this.Children.Add(factionListPanel);

            CreateFactionPanels();

            Rectangle buttonFrame = Instant.CreateComponentRectangle(Bounds, new Rectangle(0.4f, 0.01f, 0.2f, 0.08f));
            Button acceptButton = new Button(buttonFrame, "Accepter");
            acceptButton.Triggered += OnAcceptPressed;
            this.Children.Add(acceptButton);

            LocalFaction.World.FactionDefeated += OnFactionDefeated;
        }
        #endregion

        #region Events
        public event Action<DiplomacyPanel> Accepted;
        #endregion

        #region Properties
        public Faction LocalFaction
        {
            get { return userInputManager.LocalFaction; }
        }
        #endregion

        #region Methods
        private void OnFactionDefeated(World world, Faction faction)
        {
            FactionPanel factionPanel = factionListPanel.Children
                .OfType<FactionPanel>()
                .FirstOrDefault(p => p.Faction == faction);

            if (factionPanel != null) factionListPanel.Children.Remove(factionPanel);
        }

        private void CreateFactionPanels()
        {
            var otherFactions = LocalFaction.World.Factions
                .Except(LocalFaction)
                .Where(faction => faction.Status == FactionStatus.Undefeated);

            Rectangle factionPanelFrame = Instant.CreateComponentRectangle(factionListPanel.Bounds, new Rectangle(1, 0.07f));
            foreach (Faction faction in otherFactions)
            {
                FactionPanel factionPanel = new FactionPanel(factionPanelFrame, faction);
                factionListPanel.Children.Add(factionPanel);
            }
        }
        
        private void OnAcceptPressed(Button obj)
        {
            Commit();
            Accepted.Raise(this);
        }

        private void Commit()
        {
            foreach (FactionPanel factionPanel in factionListPanel.Children.OfType<FactionPanel>())
            {
                bool isAlly = LocalFaction.GetDiplomaticStance(factionPanel.Faction) == DiplomaticStance.Ally;
                if (factionPanel.IsAlliedChecked == isAlly) continue;

                userInputManager.LaunchChangeDiplomacy(factionPanel.Faction);
            }
        }
        #endregion
    }
}
