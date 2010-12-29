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

            DockPanel dockPanel = style.Create<DockPanel>();
            dockPanel.Adornment = new TextureAdornment(style.GetTexture("Gui/MenuBackground"));
            dockPanel.LastChildFill = true;
            Content = dockPanel;

            ImageBox titleImageBox = style.Create<ImageBox>();
            titleImageBox.HorizontalAlignment = Alignment.Center;
            titleImageBox.Texture = style.GetTexture("Gui/Title");
            dockPanel.Dock(titleImageBox, Direction.MaxY);

            StackPanel buttonsStackPanel = style.Create<StackPanel>();
            buttonsStackPanel.HorizontalAlignment = Alignment.Center;
            buttonsStackPanel.VerticalAlignment = Alignment.Center;
            buttonsStackPanel.MinWidth = 300;
            buttonsStackPanel.MinChildSize = 50;
            buttonsStackPanel.ChildGap = 10;
            dockPanel.Dock(buttonsStackPanel, Direction.MinX);

            StackButton(buttonsStackPanel, style, "Monojoueur", () => SinglePlayerClicked);
            StackButton(buttonsStackPanel, style, "Multijoueur", () => MultiplayerClicked);
            StackButton(buttonsStackPanel, style, "Tower Defense", () => TowerDefenseClicked);
            StackButton(buttonsStackPanel, style, "Typing Defense", () => TypingDefenseClicked);
            StackButton(buttonsStackPanel, style, "Visionner une partie", () => ReplayClicked);
            StackButton(buttonsStackPanel, style, "Options", () => OptionsClicked);
            StackButton(buttonsStackPanel, style, "Crédits", () => CreditsClicked);
            StackButton(buttonsStackPanel, style, "Quitter", () => QuitClicked);
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
        private void StackButton(StackPanel stackPanel, OrionGuiStyle style, string text, Func<Action<MainMenuUI>> eventGetter)
        {
            Button button = style.CreateTextButton(text);
            button.Clicked += sender => eventGetter().Raise(this);
            stackPanel.Stack(button);
        }
        #endregion
    }
}
