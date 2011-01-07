using System;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Engine.Gui2;
using Orion.Game.Presentation;
using Font = System.Drawing.Font;
using Orion.Engine.Gui2.Adornments;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// The user interface for the main menu of the game.
    /// </summary>
    public sealed class MainMenuUI : ContentControl
    {
        #region Fields
        private const string programmerNames = "Anthony Vallée-Dubois / Étienne-Joseph Charles / Félix Cloutier / François Pelletier / Mathieu Lavoie / Tristan Labelle";
        private const string artistName = "Guillaume Lacasse";
        #endregion

        #region Constructors
        public MainMenuUI(OrionGuiStyle style)
        {
            Argument.EnsureNotNull(style, "style");

            DockLayout dock = style.Create<DockLayout>();
            dock.Adornment = new TextureAdornment(style.GetTexture("Gui/MenuBackground"));
            dock.LastChildFill = true;
            Content = dock;

            ImageBox titleImageBox = style.Create<ImageBox>();
            titleImageBox.HorizontalAlignment = Alignment.Center;
            titleImageBox.Texture = style.GetTexture("Gui/Title");
            dock.Dock(titleImageBox, Direction.MinY);

            StackLayout buttonsStack = style.Create<StackLayout>();
            dock.Dock(buttonsStack, Direction.MaxX);
            buttonsStack.Direction = Direction.MaxY;
            buttonsStack.HorizontalAlignment = Alignment.Center;
            buttonsStack.VerticalAlignment = Alignment.Center;
            buttonsStack.MinWidth = 300;
            buttonsStack.MinChildSize = 50;
            buttonsStack.ChildGap = 10;

            StackButton(buttonsStack, style, "Monojoueur", () => SinglePlayerClicked);
            StackButton(buttonsStack, style, "Multijoueur", () => MultiplayerClicked);
            StackButton(buttonsStack, style, "Tower Defense", () => TowerDefenseClicked);
            StackButton(buttonsStack, style, "Typing Defense", () => TypingDefenseClicked);
            StackButton(buttonsStack, style, "Visionner une partie", () => ReplayClicked);
            StackButton(buttonsStack, style, "Options", () => OptionsClicked);
            StackButton(buttonsStack, style, "Crédits", () => CreditsClicked);
            StackButton(buttonsStack, style, "Quitter", () => QuitClicked);
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
        /// Raised when the user has chosen to launch a tower defense game.
        /// </summary>
        public event Action<MainMenuUI> TowerDefenseClicked;

        /// <summary>
        /// Raised when the user has chosen to launch a typing defense game.
        /// </summary>
        public event Action<MainMenuUI> TypingDefenseClicked;

        /// <summary>
        /// Raised when the user has chosen to view a replay.
        /// </summary>
        public event Action<MainMenuUI> ReplayClicked;

        /// <summary>
        /// Raised when the user has chosen to view the credits.
        /// </summary>
        public event Action<MainMenuUI> CreditsClicked;

        /// <summary>
        /// Raised when the user has chosen to view the options.
        /// </summary>
        public event Action<MainMenuUI> OptionsClicked;

        /// <summary>
        /// Raised when the user has chosen to quit the game.
        /// </summary>
        public event Action<MainMenuUI> QuitClicked;
        #endregion

        #region Methods
        private void StackButton(StackLayout stack, OrionGuiStyle style, string text, Func<Action<MainMenuUI>> eventGetter)
        {
            Button button = style.CreateTextButton(text);
            button.Clicked += (sender, mouseButton) => eventGetter().Raise(this);
            stack.Stack(button);
        }
        #endregion
    }
}
