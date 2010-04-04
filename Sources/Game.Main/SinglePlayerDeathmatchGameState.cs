using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Matchmaking.Deathmatch;
using Orion.Game.Presentation;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialisation, updating and clean up of the state of the game when
    /// a single player deathmatch is being played.
    /// </summary>
    public sealed class SinglePlayerDeathmatchGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly Random random;
        private readonly SlaveCommander localCommander;
        private readonly Match match;
        private readonly CommandPipeline commandPipeline;
        private readonly MatchUI ui;
        #endregion

        #region Constructors
        public SinglePlayerDeathmatchGameState(GameStateManager manager, GameGraphics graphics,
            IEnumerable<PlayerSlot> playerSlots, MatchSettings settings)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(playerSlots, "playerSlots");
            Argument.EnsureNotNull(settings, "settings");

            this.graphics = graphics;
            this.random = new MersenneTwister(settings.RandomSeed);

            Terrain terrain = Terrain.Generate(settings.MapSize, random);
            World world = new World(terrain, random, settings.FoodLimit);

            List<Commander> aiCommanders = new List<Commander>();
            int colorIndex = 0;
            foreach (PlayerSlot playerSlot in playerSlots)
            {
                if (playerSlot is ClosedPlayerSlot) continue;

                ColorRgb color = Faction.Colors[colorIndex];
                colorIndex++;

                Faction faction = world.CreateFaction(Colors.GetName(color), color,
                    settings.InitialAladdiumAmount, settings.InitialAlageneAmount);

                if (settings.RevealTopology) faction.LocalFogOfWar.Reveal();

                if (playerSlot is LocalPlayerSlot)
                {
                    localCommander = new SlaveCommander(faction);
                }
                else if (playerSlot is AIPlayerSlot)
                {
                    Commander commander = new AgressiveAICommander(faction, random);
                    aiCommanders.Add(commander);
                }
                else
                {
                    throw new InvalidOperationException("Single-player games only support local and AI players");
                }
            }

            WorldGenerator.Generate(world, random, !settings.StartNomad);

            match = new Match(world, random);
            match.IsPausable = true;

            commandPipeline = new CommandPipeline(match);
            if (settings.AreCheatsEnabled)
                commandPipeline.PushFilter(new CheatCodeExecutor(CheatCodeManager.Default, match));

            ReplayRecorder replayRecorder = ReplayRecorder.TryCreate(settings, world);
            if (replayRecorder != null) commandPipeline.PushFilter(replayRecorder);

            aiCommanders.ForEach(commander => commandPipeline.AddCommander(commander));
            commandPipeline.AddCommander(localCommander);

            match.Updated += (sender, args) =>
                commandPipeline.Update(sender.LastSimulationStepNumber, args.TimeDeltaInSeconds);

            ui = new MatchUI(graphics, match, localCommander);

            match.Start();
        }
        #endregion

        #region Properties
        public RootView RootView
        {
            get { return graphics.RootView; }
        }
        #endregion

        #region Methods
        protected internal override void OnEntered()
        {
            RootView.PushDisplay(ui);
        }

        protected internal override void OnShadowed()
        {
            RootView.PopDisplayWithoutDisposing(ui);
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDelta)
        {
            graphics.DispatchInputEvents();
            RootView.Update(timeDelta);
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
    }
}
