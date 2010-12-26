using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui2;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// An implementation of the <see cref="Orion.Engine.Gui2.GuiRenderer"/> class for Orion's GUI.
    /// </summary>
    public sealed class OrionGuiRenderer : GuiRenderer
    {
        #region Fields
        private readonly GraphicsContext graphicsContext;
        private readonly TextureManager textureManager;
        #endregion

        #region Constructors
        public OrionGuiRenderer(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            this.graphicsContext = graphicsContext;
            this.textureManager = new TextureManager(graphicsContext, "../../../Assets/Textures/Gui");
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
        public override void Begin()
        {
            graphicsContext.PushScissorRegion(graphicsContext.ScissorRegion);
        }

        public override void End()
        {
            graphicsContext.PopScissorRegion();
        }

        public override Texture TryGetTexture(string name)
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

        public override void Fill(Region rectangle, Texture texture, Region textureRectangle, ColorRgba tint)
        {
            Rectangle normalizedTextureRectangle = Rectangle.Empty;
            if (texture != null)
            {
                normalizedTextureRectangle = new Rectangle(
                    textureRectangle.MinX / (float)texture.Width,
                    textureRectangle.MinY / (float)texture.Height,
                    textureRectangle.Width / (float)texture.Width,
                    textureRectangle.Height / (float)texture.Height);
            }

            graphicsContext.Fill(rectangle, texture, normalizedTextureRectangle, tint);
        }
        #endregion
    }
}
