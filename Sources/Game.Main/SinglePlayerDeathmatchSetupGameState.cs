using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Matchmaking.Networking;
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
        private readonly MatchConfigurationUI2 ui;
        #endregion

        #region Constructors
        public SinglePlayerDeathmatchSetupGameState(GameStateManager manager, GameGraphics graphics)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.matchSettings = new MatchSettings();
            this.matchSettings.AreCheatsEnabled = true;

            this.ui = new MatchConfigurationUI2(graphics.GuiStyle)
            {
                NeedsReadying = false,
            };

            this.ui.AddBooleanSetting("Codes de triche", () => matchSettings.AreCheatsEnabled);
            this.ui.AddBooleanSetting("H�ros al�atoires", () => matchSettings.AreRandomHeroesEnabled);
            this.ui.AddBooleanSetting("Topologie r�v�l�e", () => matchSettings.RevealTopology);

            //this.ui.AddPlayerPressed += (sender, player) => playerSettings.AddPlayer(player);
            //this.ui.KickPlayerPressed += (sender, player) => playerSettings.RemovePlayer(player);
            //this.ui.PlayerColorChanged += OnPlayerColorChanged;
            this.ui.MatchStarted += OnStartGamePressed;
            this.ui.Exited += OnExitPressed;
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

        private void OnStartGamePressed(MatchConfigurationUI2 sender)
        {
            Random random = new MersenneTwister(matchSettings.RandomSeed);

            Terrain terrain = Terrain.Generate(matchSettings.MapSize, random);
            World world = new World(terrain, random, matchSettings.FoodLimit);

            Match match = new Match(world, random);
            match.AreRandomHeroesEnabled = matchSettings.AreRandomHeroesEnabled;

            SlaveCommander localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();

            PlayerSettings playerSettings = new PlayerSettings();
            playerSettings.AddPlayer(new LocalPlayer(playerSettings.AvailableColors.First()));
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

        private void OnExitPressed(MatchConfigurationUI2 sender)
        {
            Manager.Pop();
        }
        #endregion
    }
}
