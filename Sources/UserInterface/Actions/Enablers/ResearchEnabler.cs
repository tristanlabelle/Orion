using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Graphics;
using Orion.Graphics.Renderers;
using Orion.GameLogic;
using Orion.GameLogic.Technologies;
using Orion.Matchmaking;
using Orion.UserInterface.Widgets;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class ResearchEnabler : ActionEnabler
    {
        #region Constructors
        public ResearchEnabler(UserInputManager inputManager, ActionFrame frame, GameGraphics gameGraphics)
            : base(inputManager, frame, gameGraphics)
        { }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill(UnitSkill.Research)) return;

            var technologies = World.TechnologyTree.Technologies
                .Where(tech => LocalFaction.IsResearchable(tech) && type.CanResearch(tech));
                
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
            ActionButton button = new ActionButton(actionFrame, inputManager, string.Empty, Keys.None, gameGraphics);

            button.Name = "{0}\nAladdium: {1} Alagene: {2}"
                .FormatInvariant(technology.Name, technology.Requirements.AladdiumCost, technology.Requirements.AlageneCost);

            Texture texture = gameGraphics.GetTechnologyTexture(technology);
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
