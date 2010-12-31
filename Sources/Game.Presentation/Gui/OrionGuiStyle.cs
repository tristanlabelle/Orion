﻿using System;
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

        #region Properties
        public override Region? ClippingRectangle
        {
            get
            {
                return graphicsContext.ScissorRegion;
            }
            set
            {
                graphicsContext.PopScissorRegion();
                graphicsContext.PushScissorRegion(value ?? graphicsContext.ScissorRegion);
            }
        }
        #endregion

        #region Methods
        #region GuiRenderer Implementation
        public override void Begin()
        {
            graphicsContext.PushScissorRegion(graphicsContext.ScissorRegion);
        }

        public override void End()
        {
            graphicsContext.PopScissorRegion();
        }

        public override Texture GetTexture(string name)
        {
            return textureManager.Get(name);
        }

        public override Size MeasureText(string text, ref TextRenderingOptions options)
        {
            return graphicsContext.Measure(text, ref options);
        }

        public override void DrawText(string text, ref TextRenderingOptions options)
        {
            graphicsContext.Draw(text, ref options);
        }

        public override void DrawSprite(ref GuiSprite sprite)
        {
            Rectangle normalizedTextureRectangle = Rectangle.Empty;
            if (sprite.Texture != null)
            {
                normalizedTextureRectangle = new Rectangle(
                    sprite.PixelRectangle.MinX / (float)sprite.Texture.Width,
                    sprite.PixelRectangle.MinY / (float)sprite.Texture.Height,
                    sprite.PixelRectangle.Width / (float)sprite.Texture.Width,
                    sprite.PixelRectangle.Height / (float)sprite.Texture.Height);
            }

            graphicsContext.Fill(sprite.Rectangle, sprite.Texture, normalizedTextureRectangle, sprite.Color);
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
            label.MinHeight = (int)font.GetHeight();
        }

        private void ApplySpecificStyle(TextField textField)
        {
            textField.Font = font;
            textField.MinHeight = (int)font.GetHeight();
        }

        private void ApplySpecificStyle(Button button)
        {
            Texture upTexture = GetGuiTexture("Button_Up");

            var adornment = new OrionButtonAdornment(button, this);
            button.Adornment = adornment;
            button.Padding = adornment.Padding;
            button.MinSize = adornment.MinSize;
        }

        private void ApplySpecificStyle(CheckBox checkBox)
        {
            Texture uncheckedTexture = GetGuiTexture("CheckBox_Unchecked");

            checkBox.Adornment = new TextureAdornment(uncheckedTexture);
            checkBox.MinSize = uncheckedTexture.Size;
        }
        #endregion
        #endregion
    }
}
