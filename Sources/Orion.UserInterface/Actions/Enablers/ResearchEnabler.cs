using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Commandment;
using Orion.Graphics;
using Orion.GameLogic.Skills;
using Orion.GameLogic;
using Orion.GameLogic.Technologies;
using Orion.UserInterface.Actions.UserCommands;

namespace Orion.UserInterface.Actions.Enablers
{
    public class ResearchEnabler : ActionEnabler
    {
        public ResearchEnabler(UserInputManager inputManager, ActionFrame frame, TextureManager textureManager)
            : base(inputManager, frame, textureManager)
        {}

        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            ResearchSkill researchSkill = type.GetSkill<ResearchSkill>();
            if (researchSkill == null) return;

            var technologies = World.TechnologyTree.Technologies
                .Where(tech => LocalFaction.IsResearchable(tech) && researchSkill.Supports(tech));
                
            int x = 0;
            int y = 3;
            foreach (Technology technology in technologies)
            {
                // find an empty slot
                while (buttonsArray[x, y] != null)
                {
                    x++;
                    if (x == 4)
                    {
                        x = 0;
                        y--;
                    }
                }

                ImmediateUserCommand command = new ResearchUserCommand(inputManager, technology);
                ResearchActionButton button = new ResearchActionButton(container, inputManager, command, textureManager, technology.Name);
                button.Name = "{0}\nAladdium: {1} Alagene: {2}"
                    .FormatInvariant(technology.Name, technology.Requirements.AladdiumCost, technology.Requirements.AlageneCost);
                buttonsArray[x, y] = button;
            }
        }
    }
}
