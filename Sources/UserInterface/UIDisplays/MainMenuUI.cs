using System;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;
using Orion.UserInterface.Widgets;
using Font = System.Drawing.Font;

namespace Orion.UserInterface
{
    /// <summary>
    /// The user interface for the main menu of the game.
    /// </summary>
    public sealed class MainMenuUI : UIDisplay
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

            CreateButton(0.6f, "Monojoueur", () => SinglePlayerSelected);
            CreateButton(0.5f, "Multijoueur", () => MultiplayerSelected);
            CreateButton(0.4f, "Tower Defense", () => TowerDefenseSelected);
            CreateButton(0.3f, "Visionner une partie", () => ViewReplaySelected);

            Font creditsFont = new Font("Trebuchet MS", 10);
            AddCenteredLabel("Programmeurs ", creditsFont, 110);
            AddCenteredLabel(programmerNames, creditsFont, 90);
            AddCenteredLabel("Artiste audio-vidéo  ", creditsFont, 60);
            AddCenteredLabel(artistName, creditsFont, 40);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the user has chosen to launch a single-player game.
        /// </summary>
        public event Action<MainMenuUI> SinglePlayerSelected;

        /// <summary>
        /// Raised when the user has chosen to launch a multiplayer game.
        /// </summary>
        public event Action<MainMenuUI> MultiplayerSelected;

        /// <summary>
        /// Raised when the user has chosen to launch a tower defense game.
        /// </summary>
        public event Action<MainMenuUI> TowerDefenseSelected;

        /// <summary>
        /// Raised when the user has chosen to view a replay.
        /// </summary>
        public event Action<MainMenuUI> ViewReplaySelected;
        #endregion

        #region Methods
        /// <summary>
        /// Creates a menu button.
        /// </summary>
        /// <param name="y">The y position of the button.</param>
        /// <param name="caption">The caption text on the button.</param>
        /// <param name="eventGetter">
        /// A delegate to a method which retreives the event to be raised when the button is clicked.
        /// </param>
        private void CreateButton(float y, string caption, Func<Action<MainMenuUI>> eventGetter)
        {
            Rectangle rectangle = Instant.CreateComponentRectangle(Bounds, Rectangle.FromCenterSize(0.5f, y, 0.25f, 0.08f));
            Button button = new Button(rectangle, caption);

            if (eventGetter != null)
            {
                button.Triggered += sender => 
                {
                    Action<MainMenuUI> @event = eventGetter();
                    if (@event != null) @event(this);
                };
            }

            Children.Add(button);
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
