using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Engine.Gui.Adornments;
using Font = System.Drawing.Font;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// An implementation of the <see cref="Orion.Engine.Gui.GuiRenderer"/> class for Orion's GUI.
    /// </summary>
    public sealed class OrionGuiStyle : GuiRenderer
    {
        #region Fields
        private static readonly Font font = new Font("Trebuchet MS", 16, System.Drawing.GraphicsUnit.Pixel);
        private static readonly ColorRgb enabledTextColor = Colors.Black;
        private static readonly ColorRgb disabledTextColor = Colors.Gray;

        private readonly GameGraphics graphics;
        private readonly OrionButtonAdornment buttonAdornment;
        private readonly OrionCheckBoxButtonAdornment checkBoxButtonAdornment;
        private readonly BorderTextureAdornment comboBoxAdornment;
        #endregion

        #region Constructors
        public OrionGuiStyle(GameGraphics graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.buttonAdornment = new OrionButtonAdornment(graphics);
            this.checkBoxButtonAdornment = new OrionCheckBoxButtonAdornment(graphics);
            this.comboBoxAdornment = new BorderTextureAdornment(graphics.GetGuiTexture("ComboBox_Border_Up"));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the font used for in-game text.
        /// </summary>
        public Font Font
        {
            get { return font; }
        }
        
        private GraphicsContext GraphicsContext
        {
        	get { return graphics.Context; }
        }
        #endregion

        #region Methods
        #region GuiRenderer Implementation
        protected override void PushTransformImpl(Transform transform)
        {
            GraphicsContext.PushTransform(transform);
        }

        protected override void PopTransformImpl()
        {
            GraphicsContext.PopTransform();
        }

        protected override void PushClippingRectangleImpl(Region rectangle)
        {
            GraphicsContext.PushScissorRegion(rectangle);
        }

        protected override void PopClippingRectangleImpl()
        {
            GraphicsContext.PopScissorRegion();
        }
        
        public override Size MeasureText(Substring text, ref TextRenderingOptions options)
        {
            return GraphicsContext.Measure(text, ref options);
        }

        public override void DrawText(Substring text, ref TextRenderingOptions options)
        {
            GraphicsContext.Draw(text, ref options);
        }

        public override void DrawSprite(ref GuiSprite sprite)
        {
            if (sprite.Texture == null)
            {
                GraphicsContext.Fill(sprite.Rectangle, sprite.Color);
            }
            else
            {
                Rectangle normalizedTextureRectangle = new Rectangle(
                    sprite.PixelRectangle.MinX / (float)sprite.Texture.Width,
                    sprite.PixelRectangle.MinY / (float)sprite.Texture.Height,
                    sprite.PixelRectangle.Width / (float)sprite.Texture.Width,
                    sprite.PixelRectangle.Height / (float)sprite.Texture.Height);
                GraphicsContext.Fill(sprite.Rectangle, sprite.Texture, normalizedTextureRectangle, sprite.Color);
            }
        }
        #endregion

        #region Control Creation
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
            UIManager uiManager = new UIManager(this);
            ApplySpecificStyle(uiManager);
            return uiManager;
        }

        public Label CreateLabel(string text)
        {
            Label label = new Label(text);
            ApplySpecificStyle(label);
            return label;
        }

        /// <summary>
        /// Creates a button containing text and styles it.
        /// </summary>
        /// <param name="text">The text of the button.</param>
        /// <returns>The button that was created.</returns>
        public Button CreateTextButton(string text)
        {
            Label label = CreateLabel(text);
            label.HorizontalAlignment = Alignment.Center;
            label.VerticalAlignment = Alignment.Center;

            Button button = new Button();
            ApplySpecificStyle(button);

            button.Content = label;

            return button;
        }

        /// <summary>
        /// Creates a <see cref="CheckBox"/> whose content is a text label.
        /// </summary>
        /// <param name="text">The check box label text.</param>
        /// <returns>The newly created <see cref="CheckBox"/>.</returns>
        public CheckBox CreateTextCheckBox(string text)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Content = CreateLabel(text);
            ApplySpecificStyle(checkBox);
            return checkBox;
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

        private void ApplySpecificStyle(Control control) { }

        private void ApplySpecificStyle(UIManager uiManager)
        {
            uiManager.CursorTexture = graphics.GetGuiCursorTexture("Default");
        }

        private void ApplySpecificStyle(Label label)
        {
            label.Font = font;
            label.TextColor = Colors.Black;
            label.DisabledTextColor = Colors.Gray;
        }

        private void ApplySpecificStyle(TextField textField)
        {
            textField.Adornment = OrionTextFieldAdornment.Instance;
            textField.Padding = OrionTextFieldAdornment.Instance.Padding;
            textField.Font = font;
            textField.TextColor = Colors.Black;
            textField.DisabledTextColor = Colors.Gray;
        }

        private void ApplySpecificStyle(Button button)
        {
            button.Adornment = buttonAdornment;
            button.Padding = buttonAdornment.Padding;
            button.MinSize = buttonAdornment.MinSize;
        }

        private void ApplySpecificStyle(CheckBox checkBox)
        {
            checkBox.Button.Adornment = checkBoxButtonAdornment;
            checkBox.Button.SetSize(20, 20);
        }
        
        private void ApplySpecificStyle(ListBox listBox)
        {
            listBox.HighlightColor = new ColorRgba(Colors.LightBlue, 0.3f);
            listBox.SelectionColor = new ColorRgba(Colors.LightBlue, 0.6f);
            listBox.ItemGap = 4;
        }

        private void ApplySpecificStyle(ComboBox comboBox)
        {
            Button button = comboBox.Button;
            button.Adornment = comboBoxAdornment;
            button.MinWidth = 20;
            button.MinHeight = 20;
            button.Content = new ImageBox()
            {
                Stretch = Stretch.None,
                Texture = graphics.GetGuiTexture("Down_Arrow")
            };
            button.PreDrawing += OnComboBoxButtonPreDrawing;

            ControlViewport selectedItemViewport = comboBox.SelectedItemViewport;
            selectedItemViewport.Adornment = comboBoxAdornment;
            selectedItemViewport.Padding = 4;
            selectedItemViewport.MinWidth = 20;

            comboBox.DropDown.Adornment = comboBoxAdornment;
            comboBox.DropDown.Padding = 4;
            ApplySpecificStyle(comboBox.ListBox);
        }

        private void ApplySpecificStyle(ScrollPanel scrollPanel)
        {
            ApplySpecificStyle(scrollPanel.VerticalScrollBar);
        }

        private void OnComboBoxButtonPreDrawing(Control control, GuiRenderer renderer)
        {
            Button button = (Button)control;
            ImageBox imageBox = (ImageBox)button.Content;
            imageBox.Tint = button.IsUnderMouse ? Colors.Red : Colors.White;
        }
        #endregion
        #endregion
    }
}
