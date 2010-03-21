using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using SysPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Provides means of loading texture and caches those.
    /// </summary>
    public sealed class TextureManager : IDisposable
    {
        #region Fields
        private readonly GraphicsContext graphicsContext;
        private readonly DirectoryInfo directory;
        private readonly Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
        private readonly Texture defaultTexture;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="TextureManager"/> from a <see cref="GraphicsContext"/>.
        /// </summary>
        /// <param name="graphicsContext">The <see cref="GraphicsContext"/> to be used.</param>
        /// <param name="rootPath">The path to the root directory from which to load textures.</param>
        public TextureManager(GraphicsContext graphicsContext, string rootPath)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            Argument.EnsureNotNull(rootPath, "rootPath");

            this.graphicsContext = graphicsContext;
            this.directory = new DirectoryInfo(rootPath);
            Debug.Assert(this.directory.Exists, "Warning: The textures directory {0} does not exist.");

            this.defaultTexture = graphicsContext.CreateCheckerboardTexture(new Size(4, 4), Colors.Yellow, Colors.Pink);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the texture that is returned when no texture is found.
        /// </summary>
        public Texture DefaultTexture
        {
            get { return defaultTexture; }
        }

        /// <summary>
        /// Gets the base directory from which textures are loaded.
        /// </summary>
        public DirectoryInfo Directory
        {
            get { return directory; }
        }
        #endregion

        #region Methods
        public Texture Get(string name)
        {
            Argument.EnsureNotNull(name, "name");

            Texture texture;
            if (textures.TryGetValue(name, out texture))
                return texture ?? defaultTexture;

            if (!directory.Exists)
            {
                textures.Add(name, null);
                return defaultTexture;
            }

            string filePath = GetPath(name);

            if (filePath == null)
            {
                textures.Add(name, null);
                return defaultTexture;
            }

            try
            {
                texture = graphicsContext.CreateTextureFromFile(filePath);

                texture.SetSmooth(true);
                texture.SetRepeat(false);

                textures.Add(name, texture);
                return texture;
            }
            catch (IOException)
            {
                textures.Add(name, null);
                return defaultTexture;
            }
        }

        [Obsolete("Superseded by GameGraphics.GetUnitTexture")]
        public Texture GetUnit(string unitTypeName)
        {
            return Get(Path.Combine("Units", unitTypeName));
        }

        [Obsolete("Superseded by GameGraphics.GetActionTexture")]
        public Texture GetAction(string actionName)
        {
            return Get(Path.Combine("Actions", actionName));
        }

        [Obsolete("Superseded by GameGraphics.GetTechnologyTexture")]
        public Texture GetTechnology(string technologyName)
        {
            return Get(Path.Combine("Technologies", technologyName));
        }

        /// <summary>
        /// Disposes all textures loaded by this texture manager.
        /// </summary>
        public void Dispose()
        {
            bool defaultTextureWasDisposed = false;
            foreach (Texture texture in textures.Values)
            {
                if (texture != null)
                {
                    if (texture == defaultTexture) defaultTextureWasDisposed = true;
                    texture.Dispose();
                }
            }
            textures.Clear();

            if (!defaultTextureWasDisposed && defaultTexture != null)
                defaultTexture.Dispose();
        }

        private string GetPath(string name)
        {
            FileInfo[] candidateFiles = directory.GetFiles(name + ".*", SearchOption.TopDirectoryOnly);
            if (candidateFiles.Length == 0) return null;
            if (candidateFiles.Length > 1) Debug.Fail("Multiple textures found for '{0}'.".FormatInvariant(name));
            return candidateFiles[0].FullName;
        }
        #endregion
    }
}
