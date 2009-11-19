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

            CommandPipeline pipeline = new CommandPipeline();
            pipeline.AddFilter(new CommandReplayLogger("replay.foo", world));
            pipeline.AddFilter(new CommandTextLogger());

            UserInputCommander userCommander = null;

            int colorIndex = 0;
            foreach (PlayerSlot slot in UserInterface.Players)
            {
                if (slot is ClosedPlayerSlot) continue;

                Color color = playerColors[colorIndex];
                Faction faction = world.CreateFaction(color.Name, color);
                colorIndex++;

                if (slot is LocalPlayerSlot)
                {
                    userCommander = new UserInputCommander(faction);
                    pipeline.AddCommander(userCommander);
                }
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

            return new Match(random, world, userCommander, pipeline);
        }
    }
}
