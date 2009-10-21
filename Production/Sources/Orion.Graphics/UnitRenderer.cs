using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;

using Color = System.Drawing.Color;

namespace Orion.Graphics
{
    /// <summary>
    /// Provides functionality to draw <see cref="Unit"/>s on-screen.
    /// </summary>
    public sealed class UnitRenderer
    {
        #region Fields
        private readonly World world;
        private readonly Dictionary<string, LinePath> typeShapes = new Dictionary<string, LinePath>();
        private readonly LinePath defaultShape = LinePath.UnitCircle;
        #endregion

        #region Constructors
        public UnitRenderer(World world)
        {
            Argument.EnsureNotNull(world, "world");

            this.world = world;
            SetTypeShape("Archer", LinePath.Cross);
            SetTypeShape("Jedi", LinePath.Diamond);
        }
        #endregion

        #region Methods
        public void SetTypeShape(string typeName, LinePath shape)
        {
            Argument.EnsureNotNullNorEmpty(typeName, "typeName");
            Argument.EnsureNotNull(shape, "shape");

            typeShapes[typeName] = shape;
        }

        public void SetTypeShape(UnitType type, LinePath shape)
        {
            Argument.EnsureNotNull(type, "type");
            SetTypeShape(type.Name, shape);
        }

        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            foreach (Unit unit in world.Units)
            {
                string unitTypeName = unit.Type.Name;
                
                LinePath shape;
                if (!typeShapes.TryGetValue(unitTypeName, out shape))
                    shape = defaultShape;

                if (unit.Faction == null) graphics.StrokeColor = Color.White;
                else graphics.StrokeColor = unit.Faction.Color;

                graphics.Stroke(shape, unit.Position);
            }
        }
        #endregion
    }
}
