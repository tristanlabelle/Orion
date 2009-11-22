using System;
using System.Collections.Generic;
using System.Linq;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;
using Orion.UserInterface;
using Color = System.Drawing.Color;

namespace Orion.Main
{
    sealed class SinglePlayerMatchConfigurer : MatchConfigurer
    {
        private SinglePlayerMatchConfigurationUI ui;

        public SinglePlayerMatchConfigurer()
        {
            ui = new SinglePlayerMatchConfigurationUI();
            ui.PressedStartGame += PressStart;
            Seed = (int)Environment.TickCount;
        }

        private void PressStart(MatchConfigurationUI ui)
        {
            StartGame();
        }

        protected override MatchConfigurationUI AbstractUserInterface
        {
            get { return ui; }
        }

        public override Match Start()
        {
            CreateMap();

            UserInputCommander userCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            int colorIndex = 0;
            foreach (PlayerSlot slot in UserInterface.Players)
            {
                if (slot is ClosedPlayerSlot) continue;
                if (slot is RemotePlayerSlot && !((RemotePlayerSlot)slot).RemoteHost.HasValue) continue;

                Color color = playerColors[colorIndex];
                Faction faction = world.CreateFaction(color.Name, color);
                colorIndex++;

                if (slot is LocalPlayerSlot)
                {
                    userCommander = new UserInputCommander(faction);
                }
                else if (slot is AIPlayerSlot)
                {
                    Commander commander = new AICommander(faction, random);
                    aiCommanders.Add(commander);
                }
                else
                {
                    throw new InvalidOperationException("Multiplayer games only support remote, local and AI players");
                }
            }

            Match match = new Match(random, world, userCommander);

            ReplayWriter replayWriter = new ReplayWriter("replay.foo");
            replayWriter.AutoFlush = true;
            replayWriter.WriteHeader(Seed, world.Factions.Select(faction => faction.Name));

            CommandPipeline pipeline = new CommandPipeline(match);
            pipeline.AddFilter(new ReplayRecorder(replayWriter));
            CheatCodeFilter cheatCodes = new CheatCodeFilter(match);
            pipeline.AddFilter(cheatCodes);

            aiCommanders.ForEach(commander => pipeline.AddCommander(commander));
            pipeline.AddCommander(userCommander);

            match.Updated += pipeline.Update;

            return match;
        }
    }
}
