using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Graphics;

using Orion.Geometry;
using System.Runtime.InteropServices;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides methods to draw a <see cref="Terrain"/> on-screen.
    /// </summary>
    public sealed class TerrainRenderer
    {
        #region Fields
        private readonly Terrain terrain;
        private int textureID;
        #endregion

        #region Constructors
        public TerrainRenderer(Terrain terrain)
        {
            Argument.EnsureNotNull(terrain, "terrain");

            this.terrain = terrain;
            this.textureID = GL.GenTexture();

            try
            {
                GL.BindTexture(TextureTarget.Texture2D, textureID);

                byte[] pixels = new byte[terrain.Width * terrain.Height];
                for (int y = 0; y < terrain.Height; ++y)
                {
                    for (int x = 0; x < terrain.Width; ++x)
                    {
                        int pixelIndex = y * terrain.Width + x;
                        byte luminance = terrain.IsWalkable(x, y) ? (byte)0 : (byte)255;
                        pixels[pixelIndex] = luminance;
                    }
                }

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Luminance,
                    terrain.Width, terrain.Height, 0, PixelFormat.Luminance, PixelType.UnsignedByte,
                    pixels);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Nearest);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int)TextureWrapMode.Repeat);

                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                    (int)TextureEnvMode.Modulate);

                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            catch
            {
                GL.DeleteTexture(textureID);
                throw;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the OpenGL identifier of this texture.
        /// </summary>
        public int TextureID
        {
            get { return textureID; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphics)
        {
            Rectangle terrainBounds = new Rectangle(0, 0, terrain.Width, terrain.Height);
            graphics.FillTextured(terrainBounds, textureID);
        }

        public void Dispose()
        {
            if (textureID == 0) throw new ObjectDisposedException(null);
            GL.DeleteTexture(textureID);
            textureID = 0;
        }
        #endregion
    }
}
