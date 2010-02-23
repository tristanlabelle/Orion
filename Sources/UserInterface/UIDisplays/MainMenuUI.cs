using System;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;
using Orion.UserInterface.Widgets;
using Font = System.Drawing.Font;

namespace Orion.UserInterface
{
    public class MainMenuUI : UIDisplay
    {
        #region Fields
        // HACK: Add whitespace to then end of strings because the last word ends up clipped otherwise.
        private const string programmerNames = "Anthony Vallée-Dubois / Étienne-Joseph Charles / Félix Cloutier / François Pelletier / Mathieu Lavoie / Tristan Labelle       ";
        private const string artistName = "Guillaume Lacasse ";
        #endregion

        #region Constructors
        public MainMenuUI()
        {
            Font titleFont = new Font("Impact", 48);
            AddCenteredLabel("Orion", titleFont, 600);

            CreateButton(0.6f, "Monojoueur", BeginSinglePlayer);
            CreateButton(0.5f, "Multijoueur", BeginMultiplayerGame);
            CreateButton(0.4f, "Tower Defense", null);
            CreateButton(0.3f, "Visionner une partie", BeginViewReplay);

            Font creditsFont = new Font("Trebuchet MS", 10);
            AddCenteredLabel("Programmeurs ", creditsFont, 110);
            AddCenteredLabel(programmerNames, creditsFont, 90);
            AddCenteredLabel("Artiste audio-vidéo  ", creditsFont, 60);
            AddCenteredLabel(artistName, creditsFont, 40);
        }
        #endregion

        #region Events
        public event Action<MainMenuUI> LaunchedSinglePlayerGame;
        public event Action<MainMenuUI> LaunchedMultiplayerGame;
        public event Action<MainMenuUI> LaunchedReplayViewer;
        #endregion

        #region Methods
        private void CreateButton(float y, string caption, Action clickHandler)
        {
            Rectangle rectangle = Instant.CreateComponentRectangle(Bounds, Rectangle.FromCenterSize(0.5f, y, 0.25f, 0.08f));
            Button button = new Button(rectangle, caption);
            Children.Add(button);
            if (clickHandler != null) button.Triggered += (sender) => clickHandler();
        }

        private void BeginSinglePlayer()
        {
            if (LaunchedSinglePlayerGame != null)
                LaunchedSinglePlayerGame(this);
        }

        private void BeginMultiplayerGame()
        {
            if (LaunchedMultiplayerGame != null)
                LaunchedMultiplayerGame(this);
        }

        private void BeginViewReplay()
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
        #endregion
    }
}
