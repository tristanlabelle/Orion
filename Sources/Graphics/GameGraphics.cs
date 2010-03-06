using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using System.IO;
using Orion.GameLogic;
using Orion.GameLogic.Technologies;

namespace Orion.Graphics
{
    /// <summary>
    /// Central point of access to game graphics. Used for rendering and resource creating.
    /// </summary>
    public sealed class GameGraphics
    {
        #region Fields
        private readonly GraphicsContext graphicsContext;
        private readonly TextureManager textureManager;
        #endregion

        #region Constructors
        public GameGraphics(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            this.graphicsContext = graphicsContext;
            string rootTexturePath = "..{0}..{0}..{0}Assets{0}Textures".FormatInvariant(Path.DirectorySeparatorChar);
            this.textureManager = new TextureManager(graphicsContext, rootTexturePath);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the graphics context which provides graphics to the game.
        /// </summary>
        public GraphicsContext GraphicsContext
        {
            get { return graphicsContext; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a texture for a miscellaneous game element. 
        /// </summary>
        /// <param name="name">The name of the game element.</param>
        /// <returns>The texture for that game element.</returns>
        public Texture GetMiscTexture(string name)
        {
            return textureManager.Get(name);
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
            return textureManager.Get(fullName);
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
            return textureManager.Get(name);
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
            return textureManager.Get(fullName);
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
            return textureManager.Get(fullName);
        }
        #endregion
    }
}
