using Orion.Geometry;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public class MainMenuUI : UIDisplay
    {
        public MainMenuUI(GenericEventHandler<Button> beginSinglePlayerGameDelegate, GenericEventHandler<Button> beginMultiplayerGameDelegate)
        {
            Button singleplayerGame = new Button(new Rectangle(300, 400, 400, 100), "Single Player Game");
            singleplayerGame.Pressed += beginSinglePlayerGameDelegate;
            Children.Add(singleplayerGame);

            Button multiplayerGame = new Button(new Rectangle(300, 250, 400, 100), "Multiplayer Game");
            multiplayerGame.Pressed += beginMultiplayerGameDelegate;
            Children.Add(multiplayerGame);
        }

        protected internal override void Render()
        {
            base.Render();
        }

        internal override void OnEnter(RootView enterOn)
        { }

        internal override void OnShadow(RootView hiddenOf)
        { }
    }
}
