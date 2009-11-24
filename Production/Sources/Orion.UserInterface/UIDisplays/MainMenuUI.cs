using Orion.Geometry;
using Orion.UserInterface.Widgets;
using OpenTK.Math;

namespace Orion.UserInterface
{
    public class MainMenuUI : UIDisplay
    {
        public MainMenuUI()
        {
            Rectangle singlePlayerButtonRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.165f, 0.505f), new Vector2(0.475f, 0.66f));
            Button singleplayerGame = new Button(singlePlayerButtonRect, "Single Player Game");
            singleplayerGame.Triggered += BeginSinglePlayer;
            Children.Add(singleplayerGame);

            Rectangle multiplayerButtonRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.525f, 0.505f), new Vector2(0.835f, 0.66f));
            Button multiplayerGame = new Button(multiplayerButtonRect, "Multiplayer Game");
            multiplayerGame.Triggered += BeginMultiplayerGame;
            Children.Add(multiplayerGame);

            Rectangle viewReplayButtonRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.345f, 0.33f), new Vector2(0.655f, 0.485f));
            Button replayGame = new Button(viewReplayButtonRect, "View Replay");
            replayGame.Triggered += BeginViewReplay;
            Children.Add(replayGame);
        }

        public event GenericEventHandler<MainMenuUI> LaunchedSinglePlayerGame;
        public event GenericEventHandler<MainMenuUI> LaunchedMultiplayerGame;
        public event GenericEventHandler<MainMenuUI> LaunchedReplayViewer;

        private void BeginSinglePlayer(Button sender)
        {
            if (LaunchedSinglePlayerGame != null)
                LaunchedSinglePlayerGame(this);
        }

        private void BeginMultiplayerGame(Button sender)
        {
            if (LaunchedMultiplayerGame != null)
                LaunchedMultiplayerGame(this);
        }

        private void BeginViewReplay(Button sender)
        {
            if (LaunchedReplayViewer != null)
                LaunchedReplayViewer(this);
        }
    }
}
