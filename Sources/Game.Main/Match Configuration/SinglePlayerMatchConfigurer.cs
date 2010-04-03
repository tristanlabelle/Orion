using System;
using System.Collections.Generic;
using System.Linq;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Deathmatch;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Presentation;

namespace Orion.Game.Main
{
    sealed class SinglePlayerMatchConfigurer : MatchConfigurer
    {
        #region Fields
        private SinglePlayerMatchConfigurationUI ui;
        #endregion

        #region Constructors
        public SinglePlayerMatchConfigurer()
        {
            settings.AreCheatsEnabled = true;

            ui = new SinglePlayerMatchConfigurationUI(settings);
            ui.StartGamePressed += PressStart;
        }
        #endregion

        #region Methods
        private void PressStart(MatchConfigurationUI ui)
        {
            StartGame();
        }

        protected override MatchConfigurationUI AbstractUserInterface
        {
            get { return ui; }
        }

        public override void Start(out Match match, out SlaveCommander localCommander)
        {
            CreateWorld(settings.MapSize);

            localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            int colorIndex = 0;
            foreach (PlayerSlot slot in UserInterface.Players)
            {
                if (slot is ClosedPlayerSlot) continue;
                if (slot is RemotePlayerSlot && !((RemotePlayerSlot)slot).HostEndPoint.HasValue) continue;

                ColorRgb color = Faction.Colors[colorIndex];
                Faction faction = world.CreateFaction(Colors.GetName(color), color, settings.InitialAladdiumAmount, settings.InitialAlageneAmount);
                if (settings.RevealTopology) faction.LocalFogOfWar.Reveal();
                colorIndex++;

                if (slot is LocalPlayerSlot)
                {
                    localCommander = new SlaveCommander(faction);
                }
                else if (slot is AIPlayerSlot)
                {
                    Commander commander = new AgressiveAICommander(faction, random);
                    aiCommanders.Add(commander);
                }
                else
                {
                    throw new InvalidOperationException("Single-player games only support local and AI players");
                }
            }

            WorldGenerator.Generate(world, random, !settings.IsNomad);
            match = new Match(world);
            match.IsPausable = true;

            CommandPipeline pipeline = new CommandPipeline(match);
            TryPushCheatCodeExecutor(pipeline, match);
            TryPushReplayRecorder(pipeline);

            aiCommanders.ForEach(commander => pipeline.AddCommander(commander));
            pipeline.AddCommander(localCommander);

            match.Updated += (sender, args) =>
                pipeline.Update(sender.LastSimulationStepNumber, args.TimeDeltaInSeconds);
        }
        #endregion
    }
}
