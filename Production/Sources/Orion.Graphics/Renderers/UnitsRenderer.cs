using System;
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
    public sealed class UnitsRenderer
    {
        #region Static
        #region Fields
        private static readonly Color lowLifeColor = Color.Red;
        private static readonly Color middleLifeColor = Color.Yellow;
        private static readonly Color fullLifeColor = Color.ForestGreen;
        #endregion

        #region Methods

        private static Color Interpolate(Color first, Color second, float completion)
        {
            float opposite = 1 - completion;
            return Color.FromArgb(
                (int)(first.R * opposite + second.R * completion),
                (int)(first.G * opposite + second.G * completion),
                (int)(first.B * opposite + second.B * completion));
        }

        #endregion
        #endregion

        #region Fields
        private readonly World world;
        private readonly Dictionary<string, LinePath> typeShapes = new Dictionary<string, LinePath>();
        private readonly LinePath defaultShape = LinePath.Circle;
        private bool drawHealthBars;
        private FogOfWar fogOfWar;
        #endregion

        #region Constructors
        public UnitsRenderer(World world, FogOfWar fogOfWar)
        {
            Argument.EnsureNotNull(world, "world");

            this.world = world;
            this.fogOfWar = fogOfWar;
            SetTypeShape("Harvester", LinePath.Circle);
            SetTypeShape("Builder", LinePath.Diamond);
            SetTypeShape("Scout", LinePath.CreateCircle(0.5f, 8));
            SetTypeShape("MeleeAttacker", LinePath.Triangle);
            SetTypeShape("RangedAttacker", LinePath.Cross);
            SetTypeShape("Factory", LinePath.Pentagon);
            SetTypeShape("Tower", LinePath.Square);
        }
        #endregion

        #region Properties
        public bool DrawHealthBars
        {
            get { return drawHealthBars; }
            set { drawHealthBars = value; }
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

            DrawUnits(graphics);
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
                if (fogOfWar.GetTileVisibility(unit.Position) == TileVisibility.Visible)
                {
                    context.FillColor = unit.Faction.Color;
                    context.Fill(unitRect.TranslatedBy(unit.Position));
                }
            }
        }

        private void DrawUnits(GraphicsContext graphics)
        {
            foreach (Unit unit in world.Entities.OfType<Unit>())
            {
                if (fogOfWar.GetTileVisibility(unit.Position) == TileVisibility.Visible)
                {
                    string unitTypeName = unit.Type.Name;

                    LinePath shape;
                    if (!typeShapes.TryGetValue(unitTypeName, out shape))
                        shape = defaultShape;

                    if (unit.Faction == null) graphics.StrokeColor = Color.White;
                    else graphics.StrokeColor = unit.Faction.Color;

                    if (unit.Faction == null) graphics.FillColor = Color.White;
                    else graphics.FillColor = unit.Faction.Color;

                    using (graphics.Transform(new Transform(unit.Position, unit.Angle, unit.BoundingRectangle.Size)))
                    {
                        graphics.Stroke(shape, Vector2.Zero);
                    }

                    if (DrawHealthBars)
                    {
                        DrawHealthBar(graphics, unit);
                    }
                }
            }
        }

        public void DrawHealthBar(GraphicsContext context, Unit unit)
        {
            float healthbarWidth = (float)Math.Log(unit.MaxHealth);
            float y = unit.Position.Y + unit.BoundingRectangle.Height * 0.75f;
            float x = unit.Position.X - healthbarWidth / 2f;

            if (fogOfWar.GetTileVisibility(unit.Position) == TileVisibility.Visible)
            {
                DrawHealthBar(context, unit, new Vector2(x, y));
            }
        }

        public void DrawHealthBar(GraphicsContext context, Unit unit, Vector2 origin)
        {
            float healthbarWidth = (float)Math.Log(unit.MaxHealth);
            float leftHealthWidth = unit.Health * 0.1f;

            if (fogOfWar.GetTileVisibility(unit.Position) == TileVisibility.Visible)
            {
                DrawHealthBar(context, unit, new Rectangle(origin, new Vector2(healthbarWidth, 0.15f)));
            }
        }

        public void DrawHealthBar(GraphicsContext context, Unit unit, Rectangle into)
        {
            float leftHealthWidth = into.Width * (unit.Health / unit.MaxHealth);
            Vector2 origin = into.Min;

            if (fogOfWar.GetTileVisibility(unit.Position) == TileVisibility.Visible)
            {
                if (unit.Health > unit.MaxHealth / 2)
                {
                    float lifeFraction = (unit.Health - unit.MaxHealth / 2f) / (unit.MaxHealth / 2f);
                    context.FillColor = Interpolate(middleLifeColor, fullLifeColor, lifeFraction);
                }
                else
                {
                    float lifeFraction = unit.Health / (unit.MaxHealth / 2f);
                    context.FillColor = Interpolate(lowLifeColor, middleLifeColor, lifeFraction);
                }

                Rectangle lifeRect = new Rectangle(origin.X, origin.Y, leftHealthWidth, into.Height);
                context.Fill(lifeRect);
                Rectangle damageRect = new Rectangle(origin.X + leftHealthWidth, origin.Y, into.Width - leftHealthWidth, into.Height);
                context.FillColor = Color.DarkGray;
                context.Fill(damageRect);
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
                graphics.StrokeLineStrip(path.Points);
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
