﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Presentation.Audio;
using Orion.Engine.Geometry;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialisation, updating and clean up of the state of the game when
    /// a single player deathmatch is being played.
    /// </summary>
    public sealed class DeathmatchGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly GameAudio audio;
        private readonly Match match;
        private readonly CommandPipeline commandPipeline;
        private readonly SlaveCommander localCommander;
        private readonly MatchUI2 ui;
        private readonly IMatchRenderer matchRenderer;
        private readonly MatchAudioPresenter audioPresenter;
        private SimulationStep lastSimulationStep;
        #endregion

        #region Constructors
        public DeathmatchGameState(GameStateManager manager, GameGraphics graphics,
            Match match, CommandPipeline commandPipeline, SlaveCommander localCommander)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(commandPipeline, "commandPipeline");
            Argument.EnsureNotNull(localCommander, "localCommander");

            this.graphics = graphics;
            this.audio = new GameAudio();
            this.match = match;
            this.commandPipeline = commandPipeline;
            this.localCommander = localCommander;

            UserInputManager userInputManager = new UserInputManager(match, localCommander);
            matchRenderer = new DeathmatchRenderer(userInputManager, graphics);
            this.ui = new MatchUI2(graphics.GuiStyle);
            this.ui.MinimapRendering += OnMinimapRendering;
            this.audioPresenter = new MatchAudioPresenter(audio, userInputManager);
            this.lastSimulationStep = new SimulationStep(-1, 0, 0);

            //this.ui.QuitPressed += OnQuitPressed;
            this.match.World.EntityRemoved += OnEntityRemoved;
            this.match.World.FactionDefeated += OnFactionDefeated;
        }
        #endregion

        #region Properties
        private World World
        {
            get { return match.World; }
        }
        #endregion

        #region Methods
        protected internal override void OnEntered()
        {
            graphics.UIManager.Content = ui;
        }

        protected internal override void OnShadowed()
        {
            graphics.UIManager.Content = null;
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            if (match.IsRunning)
            {
                SimulationStep step = new SimulationStep(
                    lastSimulationStep.Number + 1,
                    lastSimulationStep.TimeInSeconds + timeDeltaInSeconds,
                    timeDeltaInSeconds);

                match.World.Update(step);

                lastSimulationStep = step;
            }

            commandPipeline.Update(lastSimulationStep);

            graphics.UpdateGui(timeDeltaInSeconds);
            //audioPresenter.SetViewBounds(ui.CameraBounds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            Size clientSize = graphics.Window.ClientAreaSize;
            float aspectRatio = clientSize.Height / (float)clientSize.Width;
            float widthInTiles = clientSize.Width / 32f;
            graphics.Context.ProjectionBounds = new Rectangle(30, 50, widthInTiles, widthInTiles * aspectRatio);
            matchRenderer.Draw(graphics.Context.ProjectionBounds);

            graphics.Context.ProjectionBounds = new Rectangle(graphics.Window.ClientAreaSize.Width, graphics.Window.ClientAreaSize.Height);
            graphics.DrawGui();
        }

        private void OnMinimapRendering(MatchUI2 sender, Region rectangle)
        {
            if (rectangle.Area == 0) return;

            Rectangle previousProjectionBounds = graphics.Context.ProjectionBounds;

            Size clientSize = graphics.Window.ClientAreaSize;
            graphics.Context.ProjectionBounds = new Rectangle(
                rectangle.MinX * rectangle.Width / clientSize.Width,
                rectangle.MinY * rectangle.Height / clientSize.Height,
                World.Bounds.Width * clientSize.Width / rectangle.Width,
                World.Bounds.Height * clientSize.Height / rectangle.Height);
            matchRenderer.DrawMinimap();

            graphics.Context.ProjectionBounds = previousProjectionBounds;
        }

        public override void Dispose()
        {
            audioPresenter.Dispose();
            //ui.Dispose();
            commandPipeline.Dispose();
            audio.Dispose();
        }

        private void OnQuitPressed(MatchUI sender)
        {
            Manager.PopTo<MainMenuGameState>();
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            Faction faction = unit.Faction;
            if (faction.Status == FactionStatus.Defeated) return;

            bool hasKeepAliveUnit = faction.Units.Any(u => u.IsAlive && u.Type.KeepsFactionAlive);
            if (hasKeepAliveUnit) return;
            
            faction.MarkAsDefeated();
        }

        private void OnFactionDefeated(World sender, Faction faction)
        {
            faction.MassSuicide();

            if (faction == localCommander.Faction)
            {
                audioPresenter.PlayDefeatSound();
                //ui.DisplayDefeatMessage(() => Manager.PopTo<MainMenuGameState>());
                return;
            }

            bool allEnemyFactionsDefeated = sender.Factions
                .Where(f => !localCommander.Faction.GetDiplomaticStance(f).HasFlag(DiplomaticStance.AlliedVictory))
                .All(f => f == faction || f.Status == FactionStatus.Defeated);
            if (!allEnemyFactionsDefeated) return;

            audioPresenter.PlayVictorySound();
            //ui.DisplayVictoryMessage(() => Manager.PopTo<MainMenuGameState>());
        }
        #endregion
    }
}
