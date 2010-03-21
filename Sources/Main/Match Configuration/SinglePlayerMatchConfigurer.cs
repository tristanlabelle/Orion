using System;
using System.Collections.Generic;
using System.Linq;
using Orion.Engine;
using Orion.GameLogic;
using Orion.Matchmaking;
using Orion.Matchmaking.Deathmatch;
using Orion.Matchmaking.Commands.Pipeline;
using Orion.UserInterface;

namespace Orion.Main
{
    sealed class SinglePlayerMatchConfigurer : MatchConfigurer
    {
        #region Fields
        private SinglePlayerMatchConfigurationUI ui;
        #endregion

        #region Constructors
        public SinglePlayerMatchConfigurer()
        {
            ui = new SinglePlayerMatchConfigurationUI();
            ui.PressedStartGame += PressStart;
            Seed = (int)Environment.TickCount;
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
            CreateWorld(UserInterface.MapSize);

            localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            int colorIndex = 0;
            foreach (PlayerSlot slot in UserInterface.Players)
            {
                if (slot is ClosedPlayerSlot) continue;
                if (slot is RemotePlayerSlot && !((RemotePlayerSlot)slot).HostEndPoint.HasValue) continue;

                ColorRgb color = Faction.Colors[colorIndex];
                Faction faction = world.CreateFaction(Colors.GetName(color), color);
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
                    throw new InvalidOperationException("Multiplayer games only support remote, local and AI players");
                }
            }

            WorldGenerator.Generate(world, random);
            match = new Match(random, world);
            match.IsPausable = true;

            CommandPipeline pipeline = new CommandPipeline(match);
            pipeline.PushFilter(new CheatCodeExecutor(CheatCodeManager.Default, match));
            TryPushReplayRecorderToPipeline(pipeline);

            aiCommanders.ForEach(commander => pipeline.AddCommander(commander));
            pipeline.AddCommander(localCommander);

            match.Updated += (sender, args) =>
                pipeline.Update(sender.LastSimulationStepNumber, args.TimeDeltaInSeconds);
        }
        #endregion
    }
}
