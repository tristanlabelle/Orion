using Orion.Geometry;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public class MainMenuUI : UIDisplay
    {
        public MainMenuUI(GenericEventHandler<Button> beginSinglePlayerGameDelegate, GenericEventHandler<Button> beginMultiplayerGameDelegate)
        {
            Button singleplayerGame = new Button(new Rectangle(300, 400, 400, 100), "Single Player Game");
            singleplayerGame.Triggered += beginSinglePlayerGameDelegate;
            Children.Add(singleplayerGame);

            Button multiplayerGame = new Button(new Rectangle(300, 250, 400, 100), "Multiplayer Game");
            multiplayerGame.Triggered += beginMultiplayerGameDelegate;
            Children.Add(multiplayerGame);
        }

        protected internal override void Render()
        {
            base.Render();
        }
    }
}
