using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Matchmaking;
using Orion.Graphics;
using Orion.GameLogic.Skills;
using Orion.GameLogic;
using Orion.GameLogic.Technologies;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class ResearchEnabler : ActionEnabler
    {
        #region Constructors
        public ResearchEnabler(UserInputManager inputManager, ActionFrame frame, TextureManager textureManager)
            : base(inputManager, frame, textureManager)
        { }
        #endregion

        #region Methods
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

                buttonsArray[x, y] = CreateButton(technology);
            }
        }

        private ActionButton CreateButton(Technology technology)
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, string.Empty, Keys.None, textureManager);

            button.Name = "{0}\nAladdium: {1} Alagene: {2}"
                .FormatInvariant(technology.Name, technology.Requirements.AladdiumCost, technology.Requirements.AlageneCost);

            Texture texture = textureManager.GetTechnology(technology.Name);
            button.Renderer = new TexturedFrameRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                inputManager.LaunchResearch(technology);
            };

            return button;
        }
        #endregion
    }
}
