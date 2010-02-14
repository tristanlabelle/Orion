using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions
{
    public sealed class BuildActionButton : ActionButton
    {
        #region Fields
        private readonly UnitType unitType;
        private readonly UnitTypeRegistry unitTypeRegistry;
        #endregion

        #region Constructor
        public BuildActionButton(ActionFrame frame, UserInputManager manager, UnitType unitType,
            UnitTypeRegistry unitTypeRegistry, TextureManager textureManager)
            : base(frame, manager, "Build", Keys.B, textureManager)
        {
            this.unitType = unitType;
            this.unitTypeRegistry = unitTypeRegistry;
            Texture texture = textureManager.GetAction("Build");
            base.Renderer = new TexturedFrameRenderer(texture);
        }
        #endregion

        #region Methods
        protected override void OnPress()
        {
            base.OnPress();
            actionFrame.Push(new BuildActionProvider(actionFrame, inputManager, unitType, unitTypeRegistry, textureManager));
        }
        #endregion
    }
}
