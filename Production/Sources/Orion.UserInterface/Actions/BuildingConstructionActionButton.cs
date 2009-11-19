using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Skills = Orion.GameLogic.Skills;
using Orion.Commandment;
using Orion.GameLogic;
using OpenTK.Math;

namespace Orion.UserInterface.Actions
{
    public class BuildingConstructionActionButton : ActionButton
    {
        #region Nested Types
        private class BuildUserCommand : UserInputCommand
        {
            private UnitType buildingType;
            private UserInputManager inputManager;

            public BuildUserCommand(UserInputManager manager, UnitType type)
            {
                buildingType = type;
                inputManager = manager;
            }

            public override void Execute(Entity entity)
            {
                if (entity is ResourceNode && buildingType.HasSkill<Skills.ExtractAlagene>())
                    if (((ResourceNode)entity).Type == ResourceType.Alagene)
                        inputManager.LaunchBuild(((ResourceNode)entity).Position, buildingType);

            }

            public override void Execute(Vector2 at)
            {
                inputManager.LaunchBuild(at, buildingType);
            }
        }
        #endregion

        #region Fields
        private UnitType builtType;
        #endregion

        #region Constructor
        public BuildingConstructionActionButton(ActionFrame frame, UserInputManager manager, UnitType builtType, Faction faction)
            : base(frame, manager, Keys.None)
        {
            this.builtType = builtType;
            int aladdium = faction.GetStat(builtType, UnitStat.AladdiumCost);
            int alagene = faction.GetStat(builtType, UnitStat.AlageneCost);
            Name = "{0}\nAladdium: {1} / Alagene: {2}".FormatInvariant(builtType.Name, aladdium, alagene);
        }
        #endregion

        #region Methods
        protected override void OnPress()
        {
            inputManager.SelectedCommand = new BuildUserCommand(inputManager, builtType);
            base.OnPress();
        }
        #endregion
    }
}
