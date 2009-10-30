
using Orion.Geometry;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface
{
    public class MainMenuUI : UIDisplay
    {
        public MainMenuUI()
        {
            Button singlePlayerGame = new Button(new Rectangle(40, 10), "Single Player Game");
            singlePlayerGame.Frame = new Rectangle(300, 400, 400, 100);
            Children.Add(singlePlayerGame);
        }

        internal override void OnEnter(RootView enterOn)
        { }

        internal override void OnShadow(RootView hiddenOf)
        { }
    }
}
