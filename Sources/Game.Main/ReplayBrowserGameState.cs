using System;
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
using Orion.Game.Simluation;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialization, updating and cleanup logic of
    /// the menu which lists the available replays.
    /// </summary>
    public sealed class ReplayBrowserGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly ReplayBrowserUI ui;
        #endregion

        #region Constructors
        public ReplayBrowserGameState(GameStateManager manager)
            : base(manager)
        {
            this.graphics = manager.Graphics;
            this.ui = new ReplayBrowserUI();

            this.ui.ExitPressed += OnExitPressed;
            this.ui.StartPressed += OnStartPressed;
        }
        #endregion

        #region Properties
        public RootView RootView
        {
            get { return graphics.RootView; }
        }
        #endregion

        #region Methods
        #region Overrides
        protected internal override void OnEntered()
        {
            RootView.Children.Add(ui);
        }

        protected internal override void OnShadowed()
        {
            RootView.Children.Remove(ui);
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            graphics.UpdateRootView(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            RootView.Draw(graphics.Context);
        }

        public override void Dispose()
        {
            ui.Dispose();
        }
        #endregion

        private void OnExitPressed(ReplayBrowserUI sender)
        {
            Manager.Pop();
        }

        private void OnStartPressed(ReplayBrowserUI sender, string replayFilePath)
        {
            ReplayReader replayReader = new ReplayReader(replayFilePath);
            MatchSettings matchSettings = replayReader.MatchSettings;
            PlayerSettings playerSettings = replayReader.PlayerSettings;

            Random random = new MersenneTwister(matchSettings.RandomSeed);

            WorldGenerator generator = new RandomWorldGenerator(random, !matchSettings.StartNomad);
            Terrain terrain = generator.GenerateTerrain(matchSettings.MapSize);
            World world = new World(terrain, random, matchSettings.FoodLimit);

            Match match = new Match(Manager.AssetsDirectory, world, random);
            match.AreRandomHeroesEnabled = matchSettings.AreRandomHeroesEnabled;

            Faction localFaction = world.CreateSpectatorFaction();
            SlaveCommander localCommander = new SlaveCommander(match, localFaction);

            int colorIndex = 0;
            foreach (Player player in playerSettings.Players)
            {
                ColorRgb color = player.Color;
                string factionName = player.Name;
                ++colorIndex;

                Faction faction = world.CreateFaction(factionName, color);
                faction.AladdiumAmount = matchSettings.InitialAladdiumAmount;
                faction.AlageneAmount = matchSettings.InitialAlageneAmount;
                if (matchSettings.RevealTopology) faction.LocalFogOfWar.Reveal();
            }

            generator.PrepareWorld(world, match.UnitTypes);

            CommandPipeline commandPipeline = new CommandPipeline(match);
            if (matchSettings.AreCheatsEnabled) commandPipeline.PushFilter(new CheatCodeExecutor(match));
            commandPipeline.PushFilter(new ReplayPlayer(replayReader));

            GameState targetGameState = new DeathmatchGameState(Manager, graphics,
                match, commandPipeline, localCommander);
            Manager.Push(targetGameState);
        }
        #endregion
    }
}
