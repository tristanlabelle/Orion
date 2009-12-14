using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Orion.Geometry;
using Orion.Graphics;
using Orion.UserInterface.Widgets;
using OpenTK.Math;

namespace Orion.UserInterface
{
    public class MainMenuUI : UIDisplay
    {
        private const string creditsString = "Aladdin / Anthony Vallée-Dubois / Étienne-Joseph Charles / Félix Cloutier / François Pelletier / Mathieu Lavoie / Tristan Labelle";
        private const string thanks = "Merci à Guillaume Lacasse";

        public MainMenuUI()
        {
            Font impact = new Font("Impact", 48);
            Text orionText = new Text("Orion", impact);
            Label orionLabel = new Label(orionText);
            orionLabel.Color = Color.White;
            AddCentered(orionLabel, 600);

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

            Font trebuchet = new Font("Trebuchet MS", 10);
            Text creditsText = new Text(creditsString, trebuchet);
            Text thanksText = new Text(thanks, trebuchet);

            Label creditsLabel = new Label(creditsText);
            creditsLabel.Color = Color.White;
            Label thanksLabel = new Label(thanksText);
            thanksLabel.Color = Color.White;
            AddCentered(creditsLabel, 100);
            AddCentered(thanksLabel, 80);
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

        private void AddCentered(Label label, float y)
        {
            float x = (Bounds.Width - label.Frame.Width) / 2;
            label.Frame = label.Frame.TranslatedTo(x, y);
            Children.Add(label);
        }
    }
}
