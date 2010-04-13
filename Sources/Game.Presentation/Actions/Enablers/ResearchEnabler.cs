using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions.UserCommands;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Technologies;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation.Actions.Enablers
{
    public sealed class ResearchEnabler : ActionEnabler
    {
        #region Constructors
        public ResearchEnabler(UserInputManager inputManager, ActionPanel actionPanel, GameGraphics gameGraphics)
            : base(inputManager, actionPanel, gameGraphics)
        { }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            ResearchSkill researchSkill = type.TryGetSkill<ResearchSkill>();
            if (researchSkill == null) return;

            var technologies = Match.TechnologyTree.Technologies
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
            ActionButton button = new ActionButton(actionPanel, userInputManager, string.Empty, Keys.None, graphics);

            button.Name = "{0}\nAladdium: {1} Alagene: {2}"
                .FormatInvariant(technology.Name, technology.AladdiumCost, technology.AlageneCost);

            Texture texture = graphics.GetTechnologyTexture(technology);
            button.Renderer = new TexturedRenderer(texture);

            button.Triggered += delegate(Button sender)
            {
                userInputManager.LaunchResearch(technology);
            };

            return button;
        }
        #endregion
    }
}
