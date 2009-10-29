using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;

using Color = System.Drawing.Color;
using Orion.GameLogic.Tasks;
using Orion.GameLogic.Pathfinding;
using OpenTK.Math;

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

        public LinePath GetTypeShape(UnitType type)
        {
            if (!typeShapes.ContainsKey(type.Name)) return LinePath.UnitCircle;
            return typeShapes[type.Name];
        }

        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            DrawPaths(graphics);
            DrawUnits(graphics);
            DrawAttackLines(graphics);
        }

        private void DrawUnits(GraphicsContext graphics)
        {
            foreach (Unit unit in world.Units)
            {
                string unitTypeName = unit.Type.Name;

                LinePath shape;
                if (!typeShapes.TryGetValue(unitTypeName, out shape))
                    shape = defaultShape;

                if (unit.Faction == null) graphics.StrokeColor = Color.White;
                else graphics.StrokeColor = unit.Faction.Color;

                if (unit.Faction == null) graphics.FillColor = Color.White;
                else graphics.FillColor = unit.Faction.Color;

                if (unit.Type.IsBuilding)
                    graphics.Fill(unit.Circle);
                else
                    graphics.Stroke(shape, unit.Position);
            }
        }

        private void DrawPaths(GraphicsContext graphics)
        {
            var paths = world.Units.Select(unit => unit.Task)
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
            var attacks = world.Units.Select(unit => unit.Task).OfType<Attack>();

            graphics.StrokeColor = Color.Orange;
            foreach (Attack attack in attacks)
                graphics.StrokeLineStrip(attack.Attacker.Position, attack.Target.Position);
        }
        #endregion
    }
}
