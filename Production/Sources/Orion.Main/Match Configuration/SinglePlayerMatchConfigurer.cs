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

            CommandPipeline pipeline = new SinglePlayerCommandPipeline(world);
            UserInputCommander userCommander = null;

            int colorIndex = 0;
            foreach (PlayerSlot slot in UserInterface.Players)
            {
                if (slot is ClosedPlayerSlot) continue;

                Commander commander;
                Color color = playerColors[colorIndex];
                Faction faction = world.CreateFaction(color.Name, color);
                colorIndex++;

                if (slot is LocalPlayerSlot)
                {
                    userCommander = new UserInputCommander(faction);
                    commander = userCommander;
                }
                else if (slot is AIPlayerSlot)
                    commander = new AgressiveAICommander(faction, random);
                else
                    throw new InvalidOperationException("Local games only support local players and AI players");

                commander.AddToPipeline(pipeline);
            }

            return new Match(random, world, userCommander, pipeline);
        }
    }
}
