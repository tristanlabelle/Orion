using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SysPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// Provides means of loading texture and caches those.
    /// </summary>
    public sealed class TextureManager : IDisposable
    {
        private sealed class Item : IDisposable
        {
            private readonly GraphicsContext graphicsContext;
            private Task<Image> loadingTask;
            private Texture texture;

            public Item(GraphicsContext graphicsContext, string filePath, CancellationToken cancellationToken)
            {
                Argument.EnsureNotNull(graphicsContext, "graphicsContext");
                Argument.EnsureNotNull(filePath, "filePath");

                this.graphicsContext = graphicsContext;

                loadingTask = Task.Factory.StartNew<Image>(() => Image.FromFile(filePath), cancellationToken);
            }

            public Item(GraphicsContext graphicsContext, Texture texture)
            {
                Argument.EnsureNotNull(graphicsContext, "graphicsContext");

                this.graphicsContext = graphicsContext;
                this.texture = texture;
            }

            public Texture Texture
            {
                get
                {
                    if (loadingTask != null)
                    {
                        loadingTask.Wait();

                        if (loadingTask.Status == TaskStatus.RanToCompletion)
                        {
                            using (Image image = loadingTask.Result)
                            {
                                texture = graphicsContext.CreateTexture(image);
                                texture.SetSmooth(true);
                                texture.SetRepeat(false);
                            }
                        }

                        loadingTask = null;
                    }

                    return texture;
                }
            }

            public void Dispose()
            {
                if (loadingTask != null)
                {
                    loadingTask.ContinueWith(imageTask => imageTask.Result.Dispose());
                }

                if (texture != null) texture.Dispose();
            }
        }

        #region Fields
        private readonly GraphicsContext graphicsContext;
        private readonly DirectoryInfo directory;
        private readonly CancellationTokenSource canceller = new CancellationTokenSource();
        private readonly Dictionary<string, Item> textures = new Dictionary<string, Item>();
        private readonly Texture defaultTexture;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="TextureManager"/> from a <see cref="GraphicsContext"/>.
        /// </summary>
        /// <param name="graphicsContext">The <see cref="GraphicsContext"/> to be used.</param>
        /// <param name="assets">The path to the root directory from which to load textures.</param>
        public TextureManager(GraphicsContext graphicsContext, AssetsDirectory assets)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            Argument.EnsureNotNull(assets, "rootPath");

            this.graphicsContext = graphicsContext;
            this.directory = new DirectoryInfo(assets.GetDirectoryPath("Textures"));
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
        public void PreloadByExtension(string extension)
        {
            foreach (FileInfo fileInfo in Directory.GetFiles("*." + extension, SearchOption.AllDirectories))
            {
                int directoryPathPartLength = directory.FullName.Length + 1;
                int extensionPathPartLength = extension.Length + 1;
                string filePath = fileInfo.FullName;
                string name = filePath.Substring(directoryPathPartLength, filePath.Length - directoryPathPartLength - extensionPathPartLength);

                if (textures.ContainsKey(name)) continue;

                Item item = new Item(graphicsContext, filePath, canceller.Token);
                textures.Add(name, item);
            }
        }

        public Texture Get(string name)
        {
            Argument.EnsureNotNull(name, "name");

            Item item;
            if (textures.TryGetValue(name, out item))
                return item == null || item.Texture == null ? defaultTexture : item.Texture;

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
                Texture texture = graphicsContext.CreateTextureFromFile(filePath);

                texture.SetSmooth(true);
                texture.SetRepeat(true);

                textures.Add(name, new Item(graphicsContext, texture));

                return texture;
            }
            catch (IOException)
            {
                textures.Add(name, null);
                return defaultTexture;
            }
        }

        /// <summary>
        /// Disposes all textures loaded by this texture manager.
        /// </summary>
        public void Dispose()
        {
            canceller.Cancel();

            foreach (Item item in textures.Values)
            {
                if (item != null) item.Dispose();
            }
            textures.Clear();

            if (defaultTexture != null)
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
