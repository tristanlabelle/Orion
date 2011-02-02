using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Orion.Engine;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
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
            this.playerSettings.AddPlayer(new LocalPlayer(Environment.MachineName, playerSettings.AvailableColors.First()));

            this.ui = new MatchConfigurationUI(graphics.GuiStyle)
            {
                NeedsReadying = false,
            };

            this.ui.Players.Add(playerSettings.Players.First());
            this.ui.AddSettings(matchSettings);

            this.ui.AddAIBuilder("Ramasseur", () =>
            {
                if (!playerSettings.AvailableColors.Any()) return;
                AIPlayer player = new AIPlayer("Ramasseur", playerSettings.AvailableColors.First());
                playerSettings.AddPlayer(player);
                ui.Players.Add(player, true);
            });

            this.ui.PlayerKicked += (sender, player) =>
            {
                playerSettings.RemovePlayer(player);
                ui.Players.Remove(player);
            };

            this.ui.PlayerColorChanged += (sender, player, newColor) =>
            {
                if (!playerSettings.AvailableColors.Contains(newColor)) return;
                player.Color = newColor;
            };

            this.ui.MatchStarted += sender => StartGame();
            this.ui.Exited += sender => Manager.Pop();
        }
        #endregion

        #region Methods
        #region Overrides
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
            graphics.UpdateGui(timeDeltaInSeconds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            graphics.DrawGui();
        }
        #endregion

        private void StartGame()
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
        #endregion
    }
}
