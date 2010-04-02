using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Technologies;
using Orion.Engine.Gui;
using Orion.Engine.Geometry;
using System.Windows.Forms;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Central point of access to game graphics. Used for rendering and resource creation.
    /// </summary>
    public sealed class GameGraphics : IDisposable
    {
        #region Fields
        private readonly IGameWindow window;
        private readonly RootView rootView;
        private readonly TextureManager textureManager;
        #endregion

        #region Constructors
        public GameGraphics()
        {
            this.window = new WindowsFormsGameWindow("Orion", WindowMode.Windowed, new Size(1024, 768));

            Rectangle rootViewFrame = new Rectangle(window.ClientAreaSize.Width, window.ClientAreaSize.Height);
            this.rootView = new RootView(rootViewFrame, RootView.ContentsBounds);

            this.textureManager = new TextureManager(window.GraphicsContext, "../../../Assets/Textures");
        }
        #endregion

        #region Properties
        public IGameWindow Window
        {
            get { return window; }
        }

        /// <summary>
        /// Gets the graphics context which provides graphics to the game.
        /// </summary>
        public GraphicsContext GraphicsContext
        {
            get { return window.GraphicsContext; }
        }

        public TextureManager TextureManager
        {
            get { return textureManager; }
        }

        public RootView RootView
        {
            get { return rootView; }
        }

        public Texture DefaultTexture
        {
            get { return textureManager.Get("Default"); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Causes the window to refresh itself.
        /// </summary>
        public void Refresh()
        {
            Application.DoEvents();
            if (window.WasClosed) return;

            rootView.Draw(GraphicsContext);
            GraphicsContext.Present();
        }

        #region Textures
        /// <summary>
        /// Gets a texture for a miscellaneous game element. 
        /// </summary>
        /// <param name="name">The name of the game element.</param>
        /// <returns>The texture for that game element.</returns>
        public Texture GetMiscTexture(string name)
        {
            return GetTexture(name);
        }

        /// <summary>
        /// Gets a texture representing a unit.
        /// </summary>
        /// <param name="unitTypeName">The name of a type of unit.</param>
        /// <returns>The texture for that unit type.</returns>
        public Texture GetUnitTexture(string unitTypeName)
        {
            Argument.EnsureNotNull(unitTypeName, "unitTypeName");

            string fullName = Path.Combine("Units", unitTypeName);
            return GetTexture(fullName);
        }

        /// <summary>
        /// Gets a texture representing a unit.
        /// </summary>
        /// <param name="unitType">The type of a unit.</param>
        /// <returns>The texture for that unit type.</returns>
        public Texture GetUnitTexture(UnitType unitType)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            return GetUnitTexture(unitType.Name);
        }

        /// <summary>
        /// Gets a texture representing a unit.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <returns>The texture for that unit.</returns>
        public Texture GetUnitTexture(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            return GetUnitTexture(unit.Type.Name);
        }

        /// <summary>
        /// Gets a texture representing a resource.
        /// </summary>
        /// <param name="type">The type of resource for which to retrieve a texture.</param>
        /// <returns>The texture for that resource type.</returns>
        public Texture GetResourceTexture(ResourceType type)
        {
            string name = type.ToStringInvariant();
            return GetTexture(name);
        }

        /// <summary>
        /// Gets a texture representing a resource.
        /// </summary>
        /// <param name="node">The resource node for which to retrieve a texture.</param>
        /// <returns>The texture for that resource node.</returns>
        public Texture GetResourceTexture(ResourceNode node)
        {
            Argument.EnsureNotNull(node, "node");
            return GetResourceTexture(node.Type);
        }

        /// <summary>
        /// Gets a texture representing an action in the UI.
        /// </summary>
        /// <param name="actionName">The name of the UI action.</param>
        /// <returns>The texture for that action.</returns>
        public Texture GetActionTexture(string actionName)
        {
            string fullName = Path.Combine("Actions", actionName);
            return GetTexture(fullName);
        }

        /// <summary>
        /// Gets a texture representing a technology.
        /// </summary>
        /// <param name="actionName">The technology.</param>
        /// <returns>The texture for that technology.</returns>
        public Texture GetTechnologyTexture(Technology technology)
        {
            Argument.EnsureNotNull(technology, "technology");
            string fullName = Path.Combine("Technologies", technology.Name);
            return GetTexture(fullName);
        }

        private Texture GetTexture(string name)
        {
            Texture texture = textureManager.Get(name);
            if (texture == textureManager.DefaultTexture)
                texture = DefaultTexture;
            return texture;
        }
        #endregion

        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            textureManager.Dispose();
            window.Dispose();
        }
        #endregion
    }
}
