using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using SysPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides means of loading texture and caches those.
    /// </summary>
    public sealed class TextureManager : IDisposable
    {
        #region Fields
        private readonly DirectoryInfo directory;
        private readonly Dictionary<string, Texture> textures
            = new Dictionary<string, Texture>();

        private readonly Texture defaulTexture;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="TextureManager"/> from the path of the
        /// directory containing the textures.
        /// </summary>
        /// <param name="directoryPath">The path of the directory containing the textures.</param>
        public TextureManager(string directoryPath)
        {
            Argument.EnsureNotNull(directoryPath, "directoryPath");
            directory = new DirectoryInfo(directoryPath);
            Debug.Assert(directory.Exists, "The textures directory {0} does not exist.");
            defaulTexture =  GetTexture("Default") ;
        }
        #endregion

        #region Methods
        public Texture GetTexture(string name)
        {
            Argument.EnsureNotNull(name, "name");

            Texture texture;
            if (textures.TryGetValue(name, out texture))
            {
                return texture ?? defaulTexture;
            }

            string filePath = GetPath(name);

            if (filePath == null)
            {
                textures.Add(name, null);
                return defaulTexture;
            }
            try
            {
                texture =  Texture.FromFile(filePath, true, false);
                textures.Add(name, texture);
                return texture;
            }
            catch (IOException)
            {
                textures.Add(name, null);
                return defaulTexture;
            }
        }

        public void Dispose()
        {
            foreach (Texture texture in textures.Values)
                texture.Dispose();
            textures.Clear();
        }

        private string GetPath(string name)
        {
            FileInfo[] candidateFiles = directory.GetFiles(name + ".*", SearchOption.TopDirectoryOnly);
            if (candidateFiles.Length == 0) return null;
            if (candidateFiles.Length > 1) Debug.Fail("Multiple textures named '{0}' found.".FormatInvariant(name));
            return candidateFiles[0].FullName;
        }
        #endregion
    }
}
