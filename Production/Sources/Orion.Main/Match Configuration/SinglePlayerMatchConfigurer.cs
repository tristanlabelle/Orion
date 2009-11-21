using System;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;
using Color = System.Drawing.Color;
using Orion.UserInterface;

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

            Color color = playerColors[0];
            Faction userFaction = world.CreateFaction(color.Name, color);
            UserInputCommander userCommander = new UserInputCommander(userFaction);

            Match match = new Match(random, world, userCommander);

            CommandPipeline pipeline = new CommandPipeline(match);
            pipeline.AddFilter(new CommandReplayLogger("replay.foo", world));
            pipeline.AddFilter(new CommandTextLogger());

            pipeline.AddCommander(userCommander);

            int colorIndex = 1;
            foreach (PlayerSlot slot in UserInterface.Players)
            {
                if (slot is ClosedPlayerSlot) continue;

                color = playerColors[colorIndex];
                Faction faction = world.CreateFaction(color.Name, color);
                colorIndex++;

                if (slot is LocalPlayerSlot) continue;
                else if (slot is AIPlayerSlot)
                {
                    Commander commander = new AgressiveAICommander(faction, random);
                    pipeline.AddCommander(commander);
                }
                else
                {
                    throw new InvalidOperationException("Local games only support local players and AI players");
                }
            }

            match.Updated += pipeline.Update;

            return match;
        }
    }
}
