using System;
using Orion.Geometry;
using Orion.Graphics;
using Orion.UserInterface.Widgets;
using OpenTK.Math;
using Font = System.Drawing.Font;

namespace Orion.UserInterface
{
    public class MainMenuUI : UIDisplay
    {
        // HACK: Add whitespace to then end of strings because the last word ends up clipped otherwise.
        private const string programmerNames = "Anthony Vallée-Dubois / Étienne-Joseph Charles / Félix Cloutier / François Pelletier / Mathieu Lavoie / Tristan Labelle       ";
        private const string artistName = "Guillaume Lacasse ";

        public MainMenuUI()
        {
            Font titleFont = new Font("Impact", 48);
            AddCenteredLabel("Orion", titleFont, 600);

            Rectangle singlePlayerButtonRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.165f, 0.505f), new Vector2(0.475f, 0.66f));
            Button singleplayerGame = new Button(singlePlayerButtonRect, "Un joueur");
            singleplayerGame.Triggered += BeginSinglePlayer;
            Children.Add(singleplayerGame);

            Rectangle multiplayerButtonRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.525f, 0.505f), new Vector2(0.835f, 0.66f));
            Button multiplayerGame = new Button(multiplayerButtonRect, "Multijoueur");
            multiplayerGame.Triggered += BeginMultiplayerGame;
            Children.Add(multiplayerGame);

            Rectangle viewReplayButtonRect = Instant.CreateComponentRectangle(Bounds, new Vector2(0.345f, 0.33f), new Vector2(0.655f, 0.485f));
            Button replayGame = new Button(viewReplayButtonRect, "Visionner une partie");
            replayGame.Triggered += BeginViewReplay;
            Children.Add(replayGame);

            Font creditsFont = new Font("Trebuchet MS", 10);
            AddCenteredLabel("Programmeurs ", creditsFont, 110);
            AddCenteredLabel(programmerNames, creditsFont, 90);
            AddCenteredLabel("Artiste audio-vidéo  ", creditsFont, 60);
            AddCenteredLabel(artistName, creditsFont, 40);
        }

        public event Action<MainMenuUI> LaunchedSinglePlayerGame;
        public event Action<MainMenuUI> LaunchedMultiplayerGame;
        public event Action<MainMenuUI> LaunchedReplayViewer;

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

        private void AddCenteredLabel(string @string, Font font, float y)
        {
            Text text = new Text(@string, font);
            Label label = new Label(text);
            label.Color = Colors.White;
            float x = (Bounds.Width - label.Frame.Width) / 2;
            label.Frame = label.Frame.TranslatedTo(x, y);
            Children.Add(label);
        }
    }
}
