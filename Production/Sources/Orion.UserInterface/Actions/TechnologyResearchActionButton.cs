using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Technologies;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions
{
    public sealed class TechnologyResearchActionButton : ActionButton
    {
        #region Fields
        private readonly ImmediateUserCommand command;
        #endregion

        #region Constructor
        public TechnologyResearchActionButton(ActionFrame frame, UserInputManager manager,
            ImmediateUserCommand command, Faction faction, TextureManager textureManager, string technologyName)
            : base(frame, manager, Keys.None,textureManager)
        {
            this.command = command;
            Texture texture = textureManager.GetTechnology(technologyName);
            base.Renderer = new TexturedFrameRenderer(texture);
        }
        #endregion

        #region Methods
        protected override void OnPress()
        {
            command.Execute();
        }
        #endregion
    }
}
