﻿using System;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;
using Orion.Game.Presentation;
using Font = System.Drawing.Font;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// The user interface for the main menu of the game.
    /// </summary>
    public sealed class MainMenuUI : MaximizedPanel
    {
        #region Fields
        private const string programmerNames = "Anthony Vallée-Dubois / Étienne-Joseph Charles / Félix Cloutier / François Pelletier / Mathieu Lavoie / Tristan Labelle";
        private const string artistName = "Guillaume Lacasse";

        private readonly Texture backgroundTexture;
        #endregion

        #region Constructors
        public MainMenuUI(GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.backgroundTexture = gameGraphics.GetMiscTexture("MenuBackground");

            Font titleFont = new Font("Impact", 48);
            AddCenteredLabel("Orion", titleFont, Colors.White, 700);

            CreateButton(0.45f, "Monojoueur", () => SinglePlayerSelected);
            CreateButton(0.38f, "Multijoueur", () => MultiplayerSelected);
            CreateButton(0.31f, "Tower Defense", () => TowerDefenseSelected);
            CreateButton(0.24f, "Visionner une partie", () => ViewReplaySelected);
            CreateButton(0.17f, "Quitter", () => QuitGameSelected);

            Font creditsFont = new Font("Trebuchet MS", 11);
            AddCenteredLabel("Programmeurs ", creditsFont, Colors.Orange, 90);
            AddCenteredLabel(programmerNames, creditsFont, Colors.Yellow, 70);
            AddCenteredLabel("Artiste audio-vidéo  ", creditsFont, Colors.Orange, 40);
            AddCenteredLabel(artistName, creditsFont, Colors.Yellow, 20);
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

        /// <summary>
        /// Raised when the user has chosen to quit the game.
        /// </summary>
        public event Action<MainMenuUI> QuitGameSelected;
        #endregion

        #region Methods
        /// <summary>
        /// Creates a menu button.
        /// </summary>
        /// <param name="y">The y position of the button.</param>
        /// <param name="caption">The caption text on the button.</param>
        /// <param name="eventGetter">
        /// A delegate to a method which retrieves the event to be raised when the button is clicked.
        /// </param>
        private void CreateButton(float y, string caption, Func<Action<MainMenuUI>> eventGetter)
        {
            Rectangle rectangle = Instant.CreateComponentRectangle(Bounds, Rectangle.FromCenterSize(0.5f, y, 0.25f, 0.06f));
            Button button = new Button(rectangle, caption);

            if (eventGetter != null)
                button.Triggered += sender => eventGetter().Raise(this);

            Children.Add(button);
        }

        private void AddCenteredLabel(string @string, Font font, ColorRgb color, float y)
        {
            Text text = new Text(@string, font);
            Label label = new Label(text);
            label.Color = color;
            float x = (Bounds.Width - label.Frame.Width) / 2;
            label.Frame = label.Frame.TranslatedTo(x, y);
            Children.Add(label);
        }

        protected override void Render(GraphicsContext graphicsContext)
        {
            graphicsContext.Fill(Bounds, backgroundTexture);

            base.Render(graphicsContext);
        }
        #endregion
    }
}