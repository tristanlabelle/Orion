using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Commandment;
using Orion.Graphics;

namespace Orion.UserInterface.Actions
{
    public class CancelButton : ActionButton
    {
        public CancelButton(ActionFrame frame, UserInputManager manager, TextureManager textureManager)
            : base(frame, manager, "Cancel", Keys.Escape,textureManager)
        {
            Renderer = new TexturedFrameRenderer (textureManager.GetTexture("Cancel"));
          }

        protected override void OnPress()
        {
            inputManager.SelectedCommand = null;
            container.Restore();
        }
    }
}
