using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine.Graphics;
using Orion.Engine;
using System.Reflection;
using Orion.Engine.Gui2.Adornments;
using Font = System.Drawing.Font;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Applies Orion-specific style settings to GUI controls.
    /// </summary>
    public sealed class OrionGuiStyle
    {
        #region Fields
        private static readonly Font font = new Font("Trebuchet MS", 16, System.Drawing.GraphicsUnit.Pixel);
        private readonly GuiRenderer renderer;
        #endregion

        #region Constructors
        public OrionGuiStyle(GuiRenderer renderer)
        {
            Argument.EnsureNotNull(renderer, "renderer");

            this.renderer = renderer;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new control and initialises it with its style.
        /// </summary>
        /// <typeparam name="TControl">The type of control to be created.</typeparam>
        /// <returns>The control that was created.</returns>
        public TControl Create<TControl>() where TControl : Control, new()
        {
            TControl control = new TControl();
            ApplyStyle(control);
            return control;
        }

        /// <summary>
        /// Creates a new <see cref="UIManager"/> and styles it.
        /// </summary>
        /// <returns>The <see cref="UIManager"/> that was created.</returns>
        public UIManager CreateUIManager()
        {
            UIManager uiManager = new UIManager(renderer);
            ApplySpecificStyle(uiManager);
            return uiManager;
        }

        /// <summary>
        /// Creates a button containing text and styles it.
        /// </summary>
        /// <param name="text">The text of the button.</param>
        /// <returns>The button that was created.</returns>
        public Button CreateTextButton(string text)
        {
            Label label = new Label(text);
            label.HorizontalAlignment = Alignment.Center;
            label.VerticalAlignment = Alignment.Center;
            ApplySpecificStyle(label);

            Button button = new Button();
            ApplySpecificStyle(button);

            button.Content = label;

            return button;
        }

        /// <summary>
        /// Applies the Orion style to a given <see cref="Control"/>.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to be styled.</param>
        public void ApplyStyle(Control control)
        {
            Argument.EnsureNotNull(control, "control");

            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            GetType().InvokeMember("ApplySpecificStyle", bindingFlags, null, this, new[] { control });
        }

        private Texture GetTexture(string name)
        {
            return renderer.TryGetTexture(name);
        }

        private void ApplySpecificStyle(Control control) { }

        private void ApplySpecificStyle(UIManager uiManager)
        {
            uiManager.CursorTexture = GetTexture("Cursors/Default");
        }

        private void ApplySpecificStyle(Label label)
        {
            label.Font = font;
            label.MinHeight = (int)font.GetHeight();
        }

        private void ApplySpecificStyle(Button button)
        {
            Texture upTexture = GetTexture("Button_Up");

            var adornment = new OrionButtonAdornment(button, renderer);
            button.Adornment = adornment;
            button.Padding = adornment.Padding;
            button.MinSize = adornment.MinSize;
        }

        private void ApplySpecificStyle(CheckBox checkBox)
        {
            Texture uncheckedTexture = GetTexture("CheckBox_Unchecked");

            checkBox.Adornment = new TextureAdornment(uncheckedTexture);
            checkBox.MinSize = uncheckedTexture.Size;
        }
        #endregion
    }
}
