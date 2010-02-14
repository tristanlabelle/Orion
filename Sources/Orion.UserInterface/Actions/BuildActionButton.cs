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
        private ActionButton[,] buildingButtons = new ActionButton[4, 4];
        #endregion

        #region Constructor
        public BuildActionButton(ActionFrame frame, UserInputManager manager, UnitType type,
            UnitTypeRegistry registry, TextureManager textureManager)
            : base(frame, manager, "Build", Keys.B, textureManager)
        {
            Texture texture = textureManager.GetAction("Build");
            base.Renderer = new TexturedFrameRenderer(texture);
            base.ActionProvider = new BuildActionProvider(frame, manager, type, registry, textureManager);
        }
        #endregion
    }
}
