using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Matchmaking.Deathmatch;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialization, updating and cleanup logic of
    /// the single player menus to setup a deatchmatch.
    /// </summary>
    public sealed class SinglePlayerDeathmatchSetupGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly MatchSettings matchSettings;
        private readonly MatchConfigurationUI ui;
        #endregion

        #region Constructors
        public SinglePlayerDeathmatchSetupGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.matchSettings = new MatchSettings();
            this.matchSettings.AddPlayer(new LocalPlayer(Faction.Colors.First()));
            IEnumerable<ColorRgb> colors = Faction.Colors.Except(matchSettings.Players.Select(p => p.Color));
            this.ui = new MatchConfigurationUI(matchSettings, colors);
            this.ui.StartGamePressed += OnStartGamePressed;
            this.ui.ExitPressed += OnExitPressed;
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

        private void OnStartGamePressed(MatchConfigurationUI sender)
        {
            Random random = new MersenneTwister(matchSettings.RandomSeed);

            Terrain terrain = Terrain.Generate(matchSettings.MapSize, random);
            World world = new World(terrain, random, matchSettings.FoodLimit);

            Match match = new Match(world, random);

            SlaveCommander localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            int colorIndex = 0;
            foreach (Player playerSlot in matchSettings.Players)
            {
                ColorRgb color = Faction.Colors[colorIndex];
                colorIndex++;

                Faction faction = world.CreateFaction(Colors.GetName(color), color);
                faction.AladdiumAmount = matchSettings.InitialAladdiumAmount;
                faction.AlageneAmount = matchSettings.InitialAlageneAmount;
                if (matchSettings.RevealTopology) faction.LocalFogOfWar.Reveal();

                if (playerSlot is LocalPlayer)
                {
                    localCommander = new SlaveCommander(match, faction);
                }
                else if (playerSlot is AIPlayer)
                {
                    Commander commander = new AgressiveAICommander(match, faction);
                    aiCommanders.Add(commander);
                }
                else
                {
                    throw new InvalidOperationException("Single-player games only support local and AI players");
                }
            }

            Debug.Assert(localCommander != null, "No local player slot.");

            WorldGenerator.Generate(world, match.UnitTypes, random, !matchSettings.StartNomad);

            CommandPipeline commandPipeline = new CommandPipeline(match);
            if (matchSettings.AreCheatsEnabled) commandPipeline.PushFilter(new CheatCodeExecutor(match));
            if (matchSettings.AreRandomHeroesEnabled) commandPipeline.PushFilter(new RandomHeroTrainer(match));

            ReplayRecorder replayRecorder = ReplayRecorder.TryCreate(matchSettings, world);
            if (replayRecorder != null) commandPipeline.PushFilter(replayRecorder);

            aiCommanders.ForEach(commander => commandPipeline.AddCommander(commander));
            commandPipeline.AddCommander(localCommander);

            GameState targetGameState = new DeathmatchGameState(
                Manager, graphics, match, commandPipeline, localCommander);
            Manager.Push(targetGameState);
        }

        private void OnExitPressed(MatchConfigurationUI sender)
        {
            Manager.Pop();
        }
        #endregion
    }
}
