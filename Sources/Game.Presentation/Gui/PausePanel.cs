using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;
using Orion.Engine;
using Orion.Engine.Gui.Adornments;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Provides the user interface of the pause menu.
    /// </summary>
    public sealed class PausePanel : ContentControl
    {
        #region Constructors
        public PausePanel(OrionGuiStyle style)
        {
            Argument.EnsureNotNull(style, "style");

            MinWidth = 200;
            Adornment = new ColorAdornment(Colors.Gray);
            Padding = 5;

            StackLayout stack = new StackLayout()
            {
                ChildGap = 10,
            };

            stack.Stack(CreateButton(style, "Reprendre", () => Resumed));
            stack.Stack(CreateButton(style, "Quitter", () => Exited));

            Content = stack;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the user chooses the resume the game.
        /// </summary>
        public event Action<PausePanel> Resumed;

        /// <summary>
        /// Raised when the user chooses the exit the game.
        /// </summary>
        public event Action<PausePanel> Exited;
        #endregion

        #region Methods
        private Button CreateButton(OrionGuiStyle style, string text, Func<Action<PausePanel>> eventGetter)
        {
            Button button = style.CreateTextButton(text);
            button.MinHeight = 50;
            button.Clicked += (sender, @event) => eventGetter().Raise(this);
            return button;
        }
        #endregion
    }
}
