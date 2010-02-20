using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Matchmaking;
using Orion.GameLogic.Skills;
using Orion.Graphics;
using Orion.UserInterface.Actions.UserCommands;
using Orion.UserInterface.Widgets;
using Orion.GameLogic;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Actions.Enablers
{
    public sealed class TransportEnabler : ActionEnabler
    {
        #region Constructors
        public TransportEnabler(UserInputManager manager, ActionFrame frame, TextureManager textureManager)
            : base(manager, frame, textureManager)
        {

        }
        #endregion

        #region Methods
        public override void LetFill(UnitType type, ActionButton[,] buttonsArray)
        {
            if (!type.HasSkill<TransportSkill>()) return;

            AddEmbarkButton(buttonsArray);
            AddDisembarkButton(buttonsArray);
        }

        private void AddEmbarkButton(ActionButton[,] buttonsArray)
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, "Embark", Keys.None, textureManager);
            Texture texture = textureManager.GetAction("Embark");
            button.Renderer = new TexturedFrameRenderer(texture);
            button.Triggered += OnEmbarkButtonPressed;

            buttonsArray[1, 3] = button;
        }

        private void AddDisembarkButton(ActionButton[,] buttonsArray)
        {
            ActionButton button = new ActionButton(actionFrame, inputManager, "Disembark", Keys.None, textureManager);
            Texture texture = textureManager.GetAction("Disembark");
            button.Renderer = new TexturedFrameRenderer(texture);
            button.Triggered += OnDisembarkButtonPressed;

            buttonsArray[2, 3] = button;
        }

        private void OnEmbarkButtonPressed(Button sender)
        {
            inputManager.SelectedCommand = new EmbarkUserCommand(inputManager);
        }

        private void OnDisembarkButtonPressed(Button sender)
        {
            inputManager.LaunchDisembark();
        }
        #endregion
    }
}
