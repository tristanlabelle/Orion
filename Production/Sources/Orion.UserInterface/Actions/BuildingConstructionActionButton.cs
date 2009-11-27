using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Orion.Graphics;

namespace Orion.UserInterface.Actions
{
    public sealed class BuildingConstructionActionButton : ActionButton
    {
        #region Fields
        private readonly UnitType buildingType;
        #endregion

        #region Constructor
        public BuildingConstructionActionButton(ActionFrame frame, UserInputManager manager,
            UnitType buildingType, Faction faction, Texture texture)
            : base(frame, manager, Keys.None)
        {
            this.buildingType = buildingType;
            int aladdium = faction.GetStat(buildingType, UnitStat.AladdiumCost);
            int alagene = faction.GetStat(buildingType, UnitStat.AlageneCost);
            Name = "{0}\nAladdium: {1} / Alagene: {2}".FormatInvariant(buildingType.Name, aladdium, alagene);
            if (texture != null) base.Renderer = new TexturedFrameRenderer(texture);
        }
        #endregion

        #region Methods
        protected override void OnPress()
        {
            inputManager.SelectedCommand = new BuildUserCommand(inputManager, buildingType);
            base.OnPress();
        }
        #endregion
    }
}
