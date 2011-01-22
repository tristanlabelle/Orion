using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui2;
using Font = System.Drawing.Font;
using System.Reflection;
using Orion.Engine.Gui2.Adornments;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// An implementation of the <see cref="Orion.Engine.Gui2.GuiRenderer"/> class for Orion's GUI.
    /// </summary>
    public sealed class OrionGuiStyle : GuiRenderer
    {
        #region Fields
        private static readonly Font font = new Font("Trebuchet MS", 16, System.Drawing.GraphicsUnit.Pixel);
        private readonly GraphicsContext graphicsContext;
        private readonly TextureManager textureManager;
        #endregion

        #region Constructors
        public OrionGuiStyle(GraphicsContext graphicsContext, TextureManager textureManager)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.graphicsContext = graphicsContext;
            this.textureManager = textureManager;
        }
        #endregion

        #region Methods
        #region GuiRenderer Implementation
        protected override void PushTransformImpl(Transform transform)
        {
            graphicsContext.PushTransform(transform);
        }

        protected override void PopTransformImpl()
        {
            graphicsContext.PopTransform();
        }

        protected override void PushClippingRectangleImpl(Region rectangle)
        {
            graphicsContext.PushScissorRegion(rectangle);
        }

        protected override void PopClippingRectangleImpl()
        {
            graphicsContext.PopScissorRegion();
        }

        public override Texture GetTexture(string name)
        {
            return textureManager.Get(name);
        }

        public override Size MeasureText(Substring text, ref TextRenderingOptions options)
        {
            return graphicsContext.Measure(text, ref options);
        }

        public override void DrawText(Substring text, ref TextRenderingOptions options)
        {
            graphicsContext.Draw(text, ref options);
        }

        public override void DrawSprite(ref GuiSprite sprite)
        {
            if (sprite.Texture == null)
            {
                graphicsContext.Fill(sprite.Rectangle, sprite.Color);
            }
            else
            {
                Rectangle normalizedTextureRectangle = new Rectangle(
                    sprite.PixelRectangle.MinX / (float)sprite.Texture.Width,
                    sprite.PixelRectangle.MinY / (float)sprite.Texture.Height,
                    sprite.PixelRectangle.Width / (float)sprite.Texture.Width,
                    sprite.PixelRectangle.Height / (float)sprite.Texture.Height);
                graphicsContext.Fill(sprite.Rectangle, sprite.Texture, normalizedTextureRectangle, sprite.Color);
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

        private Texture GetGuiTexture(string name)
        {
            return textureManager.Get("Gui/" + name);
        }

        private void ApplySpecificStyle(Control control) { }

        private void ApplySpecificStyle(UIManager uiManager)
        {
            uiManager.CursorTexture = GetGuiTexture("Cursors/Default");
        }

        private void ApplySpecificStyle(Label label)
        {
            label.Font = font;
        }

        private void ApplySpecificStyle(TextField textField)
        {
            OrionTextFieldAdornment adornment = OrionTextFieldAdornment.Instance;
            textField.Adornment = adornment;
            textField.Padding = adornment.Padding;
            textField.Font = font;
            textField.TextColor = Colors.Black;
        }

        private void ApplySpecificStyle(Button button)
        {
            Texture upTexture = GetGuiTexture("Button_Up");

            var adornment = new OrionButtonAdornment(this);
            button.Adornment = adornment;
            button.Padding = adornment.Padding;
            button.MinSize = adornment.MinSize;
        }

        private void ApplySpecificStyle(CheckBox checkBox)
        {
            checkBox.Button.Adornment = new OrionCheckBoxButtonAdornment(this);
            checkBox.Button.SetSize(20, 20);
        }

        private void ApplySpecificStyle(ComboBox comboBox)
        {
            var adornment = new BorderTextureAdornment(GetGuiTexture("ComboBox_Border_Up"));

            comboBox.Button.Adornment = adornment;
            comboBox.Button.MinWidth = 20;
            comboBox.Button.MinHeight = 20;

            ImageBox buttonImageBox = new ImageBox()
            {
                Stretch = Stretch.None,
                Texture = GetGuiTexture("Down_Arrow")
            };
            comboBox.Button.Content = buttonImageBox;
            comboBox.Button.PreDrawing += OnComboBoxButtonPreDrawing;

            comboBox.SelectedItemViewport.Adornment = adornment;
            comboBox.SelectedItemViewport.Padding = 4;
            comboBox.SelectedItemViewport.MinWidth = 20;

            comboBox.DropDown.HighlightColor = new ColorRgba(Colors.LightBlue, 0.5f);
            comboBox.DropDown.Adornment = adornment;
            comboBox.DropDown.ItemGap = 4;
            comboBox.DropDown.Padding = 4;
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
