using System;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Engine.Gui.Adornments;
using Orion.Game.Presentation;
using Font = System.Drawing.Font;
using Orion.Engine.Localization;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// The user interface for the main menu of the game.
    /// </summary>
    public sealed class MainMenuUI : ContentControl
    {
        #region Constructors
        public MainMenuUI(GameGraphics graphics, Localizer localizer)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(localizer, "localizer");

            var style = graphics.GuiStyle;
            
            DockLayout dock = style.Create<DockLayout>();
            dock.Adornment = new TextureAdornment(graphics.GetGuiTexture("MenuBackground"));
            dock.LastChildFill = true;
            Content = dock;

            ImageBox titleImageBox = style.Create<ImageBox>();
            titleImageBox.HorizontalAlignment = Alignment.Center;
            titleImageBox.Texture = graphics.GetGuiTexture("Title");
            dock.Dock(titleImageBox, Direction.NegativeY);

            StackLayout buttonsStack = new StackLayout()
            {
                Direction = Direction.PositiveY,
                HorizontalAlignment = Alignment.Center,
                VerticalAlignment = Alignment.Center,
                MinWidth = 300,
                ChildGap = 10
            };
            dock.Dock(buttonsStack, Direction.PositiveX);

            StackButton(buttonsStack, style, localizer.GetNoun("SinglePlayer"), () => SinglePlayerClicked);
            StackButton(buttonsStack, style, localizer.GetNoun("Multiplayer"), () => MultiplayerClicked);
            StackButton(buttonsStack, style, localizer.GetNoun("ViewReplay"), () => ReplayClicked);
            StackButton(buttonsStack, style, localizer.GetNoun("Credits"), () => CreditsClicked);
            StackButton(buttonsStack, style, localizer.GetNoun("Quit"), () => QuitClicked);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the user has chosen to launch a single-player game.
        /// </summary>
        public event Action<MainMenuUI> SinglePlayerClicked;

        /// <summary>
        /// Raised when the user has chosen to launch a multiplayer game.
        /// </summary>
        public event Action<MainMenuUI> MultiplayerClicked;

        /// <summary>
        /// Raised when the user has chosen to view a replay.
        /// </summary>
        public event Action<MainMenuUI> ReplayClicked;

        /// <summary>
        /// Raised when the user has chosen to view the credits.
        /// </summary>
        public event Action<MainMenuUI> CreditsClicked;

        /// <summary>
        /// Raised when the user has chosen to quit the game.
        /// </summary>
        public event Action<MainMenuUI> QuitClicked;
        #endregion

        #region Methods
        private void StackButton(StackLayout stack, OrionGuiStyle style, string text, Func<Action<MainMenuUI>> eventGetter)
        {
            Button button = style.CreateTextButton(text);
            button.MinHeight = 50;
            button.Clicked += (sender, @event) => eventGetter().Raise(this);
            stack.Stack(button);
        }
        #endregion
    }
}
