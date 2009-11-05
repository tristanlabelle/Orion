using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.GameLogic;
using Orion.GameLogic.Pathfinding;
using Orion.GameLogic.Tasks;
using Orion.Geometry;
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
        private readonly LinePath defaultShape = LinePath.Circle;
        #endregion

        #region Constructors
        public UnitRenderer(World world)
        {
            Argument.EnsureNotNull(world, "world");

            this.world = world;
            SetTypeShape("Harvester", LinePath.Circle);
            SetTypeShape("Builder", LinePath.Diamond);
            SetTypeShape("MeleeAttacker", LinePath.Triangle);
            SetTypeShape("RangedAttacker", LinePath.Cross);
            SetTypeShape("Factory", LinePath.Pentagon);
            SetTypeShape("Tower", LinePath.Square);
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

        public LinePath GetTypeShape(UnitType type)
        {
            if (!typeShapes.ContainsKey(type.Name)) return LinePath.Circle;
            return typeShapes[type.Name];
        }

        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            DrawPaths(graphics);
            DrawUnits(graphics);
            DrawAttackLines(graphics);
        }

        public void DrawMiniature(GraphicsContext context)
        {
            DrawMiniatureUnits(context);
        }

        private void DrawMiniatureUnits(GraphicsContext context)
        {
            Rectangle unitRect = new Rectangle(3, 3);
            foreach (Unit unit in world.Entities.OfType<Unit>())
            {
                context.FillColor = unit.Faction.Color;
                context.Fill(unitRect.TranslatedBy(unit.Position));
            }
        }

        private void DrawUnits(GraphicsContext graphics)
        {
            foreach (Unit unit in world.Entities.OfType<Unit>())
            {
                string unitTypeName = unit.Type.Name;

                LinePath shape;
                if (!typeShapes.TryGetValue(unitTypeName, out shape))
                    shape = defaultShape;

                if (unit.Faction == null) graphics.StrokeColor = Color.White;
                else graphics.StrokeColor = unit.Faction.Color;

                if (unit.Faction == null) graphics.FillColor = Color.White;
                else graphics.FillColor = unit.Faction.Color;

                graphics.Stroke(shape, unit.Position);
            }
        }

        private void DrawPaths(GraphicsContext graphics)
        {
            var paths = world.Entities
                .OfType<Unit>()
                .Select(unit => unit.Task)
                .OfType<Move>()
                .Select(task => task.Path)
                .Where(path => path != null);

            graphics.StrokeColor = Color.Gray;
            foreach (Path path in paths)
                graphics.StrokeLineStrip(path.Points.Select(p => new Vector2(p.X, p.Y)));
        }

        private void DrawPathfindingDebugInfo(GraphicsContext graphics, Pathfinder pathfinder)
        {
            graphics.StrokeColor = Color.Yellow;
            foreach (PathNode node in pathfinder.ClosedNodes)
                if (node.ParentNode != null)
                    graphics.StrokeLineStrip(node.ParentNode.Position, node.Position);

            graphics.StrokeColor = Color.Lime;
            foreach (PathNode node in pathfinder.OpenNodes)
                if (node.ParentNode != null)
                    graphics.StrokeLineStrip(node.ParentNode.Position, node.Position);
        }

        private void DrawAttackLines(GraphicsContext graphics)
        {
            var attacks = world.Entities
                .OfType<Unit>()
                .Select(unit => unit.Task)
                .OfType<Attack>();

            graphics.StrokeColor = Color.Orange;
            foreach (Attack attack in attacks)
                graphics.StrokeLineStrip(attack.Attacker.Position, attack.Target.Position);
        }
        #endregion
    }
}
