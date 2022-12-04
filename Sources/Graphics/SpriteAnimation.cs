using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using System.IO;
using System.Collections.ObjectModel;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides a series of textures which animate a fire.
    /// </summary>
    public sealed class SpriteAnimation
    {
        #region Fields
        private readonly float secondsPerFrame;
        private readonly ReadOnlyCollection<Texture> textures;
        #endregion

        #region Constructors
        public SpriteAnimation(GameGraphics gameGraphics, string name, float secondsPerFrame)
        {
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");
            Argument.EnsureNotNull(name, "name");

            this.secondsPerFrame = secondsPerFrame;

            string directory = Path.Combine(gameGraphics.TextureManager.Directory.FullName, name);
            this.textures = Directory.GetFiles(directory, name + ".*.*")
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .OrderBy(filename => filename)
                .Select(filename => Path.Combine("Fire", filename))
                .Select(fullTextureName => gameGraphics.TextureManager.Get(fullTextureName))
                .Where(texture => texture != gameGraphics.TextureManager.DefaultTexture)
                .DefaultIfEmpty(gameGraphics.DefaultTexture)
                .ToList()
                .AsReadOnly();
        }
        #endregion

        #region Methods
        public Texture GetTextureFromTime(float time)
        {
            int frameIndex = (int)(time / secondsPerFrame) % textures.Count;
            return textures[frameIndex];
        }
        #endregion
    }
}
