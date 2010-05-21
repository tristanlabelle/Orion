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
            private readonly Checkbox alliedVictoryCheckbox;
            private readonly Checkbox sharedVisionCheckbox;
            private readonly Checkbox sharedControlCheckbox;
            #endregion

            #region Constructors
            public FactionPanel(Rectangle frame, Faction localFaction, Faction otherFaction)
                : base(frame, otherFaction.Color)
            {
                Argument.EnsureNotNull(otherFaction, "faction");

                this.faction = otherFaction;

                Children.Add(new Label(otherFaction.Name));

                DiplomaticStance stance = localFaction.GetDiplomaticStance(otherFaction);

                Rectangle alliedVictoryCheckboxFrame = Instant.CreateComponentRectangle(Bounds, new Rectangle(0.6f, 0, 0.05f, 1));
                Rectangle sharedVisionCheckboxFrame = Instant.CreateComponentRectangle(Bounds, new Rectangle(0.75f, 0, 0.05f, 1));
                Rectangle sharedControlCheckboxFrame = Instant.CreateComponentRectangle(Bounds, new Rectangle(0.90f, 0, 0.05f, 1));

                alliedVictoryCheckbox = new Checkbox(alliedVictoryCheckboxFrame, stance.HasFlag(DiplomaticStance.AlliedVictory));
                sharedVisionCheckbox = new Checkbox(sharedVisionCheckboxFrame, stance.HasFlag(DiplomaticStance.SharedVision));
                sharedControlCheckbox = new Checkbox(sharedControlCheckboxFrame, stance.HasFlag(DiplomaticStance.SharedControl));

                if (stance.HasFlag(DiplomaticStance.SharedControl))
                {
                    alliedVictoryCheckbox.IsEnabled = false;
                    sharedVisionCheckbox.IsEnabled = false;
                    sharedControlCheckbox.IsEnabled = false;
                }
                else
                {
                    sharedControlCheckbox.StateChanged += (checkbox, state) =>
                    {
                        alliedVictoryCheckbox.IsChecked = state;
                        sharedVisionCheckbox.IsChecked = state;
                    };
                }

                Children.Add(alliedVictoryCheckbox);
                Children.Add(sharedVisionCheckbox);
                Children.Add(sharedControlCheckbox);
            }
            #endregion

            #region Properties
            public Faction Faction
            {
                get { return faction; }
            }

            public bool IsAlliedVictoryChecked
            {
                get { return alliedVictoryCheckbox.IsChecked; }
            }

            public bool IsSharedVisionChecked
            {
                get { return sharedVisionCheckbox.IsChecked; }
            }

            public bool IsSharedControlChecked
            {
                get { return sharedControlCheckbox.IsChecked; }
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

            CreateTitle();
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

        private void CreateTitle()
        {
            Rectangle headerFrame = Instant.CreateComponentRectangle(factionListPanel.Bounds, new Rectangle(1, 0.07f));
            Panel header = new Panel(headerFrame);

            Rectangle alliedVictoryFrame = Instant.CreateComponentRectangle(headerFrame, new Rectangle(0.45f, 0, 0.6f, 1));
            Rectangle sharedVisionFrame = Instant.CreateComponentRectangle(headerFrame, new Rectangle(0.65f, 0, 0.80f, 1));
            Rectangle sharedControlFrame = Instant.CreateComponentRectangle(headerFrame, new Rectangle(0.85f, 0, 1, 1));
            Label alliedVictoryLabel = new Label(alliedVictoryFrame, "Victoire alliée");
            Label sharedVisionLabel = new Label(sharedVisionFrame, "Vision partagée");
            Label sharedControlLabel = new Label(sharedControlFrame, "Contrôle partagé");

            header.Children.Add(alliedVictoryLabel);
            header.Children.Add(sharedVisionLabel);
            header.Children.Add(sharedControlLabel);
            factionListPanel.Children.Add(header);
        }

        private void CreateFactionPanels()
        {
            var otherFactions = LocalFaction.World.Factions
                .Except(LocalFaction)
                .Where(faction => faction.Status == FactionStatus.Undefeated);

            Rectangle factionPanelFrame = Instant.CreateComponentRectangle(factionListPanel.Bounds, new Rectangle(1, 0.07f));
            foreach (Faction faction in otherFactions)
            {
                FactionPanel factionPanel = new FactionPanel(factionPanelFrame, LocalFaction, faction);
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
                DiplomaticStance newStance = DiplomaticStance.Enemy;
                if (factionPanel.IsAlliedVictoryChecked) newStance |= DiplomaticStance.AlliedVictory;
                if (factionPanel.IsSharedVisionChecked) newStance |= DiplomaticStance.SharedVision;
                if (factionPanel.IsSharedControlChecked) newStance |= DiplomaticStance.SharedControl;

                userInputManager.LaunchChangeDiplomacy(factionPanel.Faction, newStance);
            }
        }
        #endregion
    }
}
