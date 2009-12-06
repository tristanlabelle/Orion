using Orion.Commandment;
using Orion.GameLogic;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;
using Orion.Graphics;

namespace Orion.UserInterface.Actions
{
    public sealed class TechnologyResearchActionButton : ActionButton
    {
        #region Fields
        private readonly Technology technology;
        #endregion

        #region Constructor
        public TechnologyResearchActionButton(ActionFrame frame, UserInputManager manager,
            Technology technology, Faction faction, TextureManager textureManager)
            : base(frame, manager, Keys.None,textureManager)
        {
            this.technology = technology;
            int aladdium = technology.Requirements.AladdiumCost;
            int alagene = technology.Requirements.AlageneCost;
            Name = "{0}\nAladdium: {1} / Alagene: {2}".FormatInvariant(technology.Name, aladdium, alagene);

            Texture texture = textureManager.GetTechnology(technology.Name);
            base.Renderer = new TexturedFrameRenderer(texture);
        }
        #endregion

        #region Methods
        protected override void OnPress()
        {
            //inputManager.SelectedCommand = new BuildUserCommand(inputManager, buildingType);
            base.OnPress();
        }
        #endregion
    }
}
