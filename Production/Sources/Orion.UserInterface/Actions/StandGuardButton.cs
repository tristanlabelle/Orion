using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Commandment;
using Orion.Graphics;

namespace Orion.UserInterface.Actions
{
    public class StandGuardButton : ActionButton
    {
        public StandGuardButton(ActionFrame frame, UserInputManager manager, TextureManager textureManager)
            : base(frame, manager, "Stand Guard", Keys.G, textureManager)
        {
            Texture texture = textureManager.GetAction("Stand Guard");
            base.Renderer = new TexturedFrameRenderer(texture);
        }

        protected override void OnPress()
        {
            inputManager.LaunchStandGuard();
        }
    }
}
