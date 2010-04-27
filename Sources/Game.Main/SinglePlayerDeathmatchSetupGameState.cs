using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking.Networking;

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
        private readonly PlayerSettings playerSettings;
        private readonly MatchConfigurationUI ui;
        #endregion

        #region Constructors
        public SinglePlayerDeathmatchSetupGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.matchSettings = new MatchSettings();
            this.matchSettings.AreCheatsEnabled = true;

            this.playerSettings = new PlayerSettings();
            this.playerSettings.AddPlayer(new LocalPlayer(playerSettings.AvailableColors.First()));

            List<PlayerBuilder> builders = new List<PlayerBuilder>();
            builders.Add(new PlayerBuilder("Harvesting Computer", (name, color) => new AIPlayer(name, color)));

            this.ui = new MatchConfigurationUI(matchSettings, playerSettings, builders);
            this.ui.AddPlayerPressed += (sender, player) => playerSettings.AddPlayer(player);
            
            this.ui.KickPlayerPressed += (sender, player) => playerSettings.RemovePlayer(player);
            this.ui.StartGamePressed += OnStartGamePressed;
            this.ui.PlayerColorChanged += OnPlayerColorChanged;
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
            match.AreRandomHeroesEnabled = matchSettings.AreRandomHeroesEnabled;

            SlaveCommander localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            foreach (Player player in playerSettings.Players)
            {
                Faction faction = world.CreateFaction(Colors.GetName(player.Color), player.Color);
                faction.AladdiumAmount = matchSettings.InitialAladdiumAmount;
                faction.AlageneAmount = matchSettings.InitialAlageneAmount;
                if (matchSettings.RevealTopology) faction.LocalFogOfWar.Reveal();

                if (player is LocalPlayer)
                {
                    localCommander = new SlaveCommander(match, faction);
                }
                else if (player is AIPlayer)
                {
                    Commander commander = new HarvestingAICommander(match, faction);
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

            ReplayRecorder replayRecorder = ReplayRecorder.TryCreate(matchSettings, playerSettings);
            if (replayRecorder != null) commandPipeline.PushFilter(replayRecorder);

            aiCommanders.ForEach(commander => commandPipeline.AddCommander(commander));
            commandPipeline.AddCommander(localCommander);

            GameState targetGameState = new DeathmatchGameState(
                Manager, graphics, match, commandPipeline, localCommander);
            Manager.Push(targetGameState);
        }

        private void OnPlayerColorChanged(MatchConfigurationUI ui, Player player, ColorRgb color)
        {
            player.Color = color;
        }

        private void OnExitPressed(MatchConfigurationUI sender)
        {
            Manager.Pop();
        }
        #endregion
    }
}
