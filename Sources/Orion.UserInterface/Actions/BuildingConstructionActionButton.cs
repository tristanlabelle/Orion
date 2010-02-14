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
            UnitType buildingType, Faction faction, TextureManager textureManager)
            : base(frame, manager, string.Empty, Keys.None, textureManager)
        {
            this.buildingType = buildingType;
            int aladdium = faction.GetStat(buildingType, UnitStat.AladdiumCost);
            int alagene = faction.GetStat(buildingType, UnitStat.AlageneCost);
            Name = "{0}\nAladdium: {1} / Alagene: {2}".FormatInvariant(buildingType.Name, aladdium, alagene);

            Texture texture = textureManager.GetUnit(buildingType.Name);
            base.Renderer = new TexturedFrameRenderer(texture);
        }
        #endregion

        #region Methods
        protected override void OnPress()
        {
            base.OnPress();
            inputManager.SelectedCommand = new BuildUserCommand(inputManager, TextureManager, buildingType);
            actionFrame.Push(new CancelActionProvider(actionFrame, inputManager, textureManager));
        }
        #endregion
    }
}
