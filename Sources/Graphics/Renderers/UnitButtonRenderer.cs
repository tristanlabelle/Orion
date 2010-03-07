using System;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics.Renderers
{
    public sealed class UnitButtonRenderer : FrameRenderer
    {
        #region Fields
        private static readonly ColorRgb FocusedStrokeColor = Colors.White;
        private static readonly ColorRgb UnfocusedStrokeColor = Colors.Black;
        private static readonly ColorRgb FocusedFillColor = ColorRgb.FromBytes(75, 75, 75);
        private static readonly ColorRgb UnfocusedFillColor = Colors.Black;

        private readonly Unit unit;
        private readonly Texture texture;
        private bool hasFocus;
        #endregion

        #region Constructors
        public UnitButtonRenderer(Unit unit, GameGraphics gameGraphics)
            : base(unit.Faction.Color)
        {
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.unit = unit;
            this.texture = gameGraphics.GetUnitTexture(unit);
        }
        #endregion

        #region Properties
        public Unit Unit
        {
            get { return unit; }
        }

        public bool HasFocus
        {
            get { return hasFocus; }
            set { hasFocus = value; }
        }

        private ColorRgb StrokeColor
        {
            get { return hasFocus ? FocusedStrokeColor : UnfocusedStrokeColor; }
        }

        private ColorRgb FillColor
        {
            get { return hasFocus ? FocusedFillColor : UnfocusedFillColor; }
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context, Rectangle bounds)
        {
            context.Fill(bounds, FillColor);
            context.Stroke(bounds, StrokeColor);

            float size = bounds.Width * 3 / 4;
            Rectangle rectangle = Rectangle.FromCenterSize(
                bounds.CenterX, bounds.Height * 5 / 8,
                size, size);

            context.Fill(rectangle, texture, unit.Faction.Color);

            float healthRatio = unit.Health / unit.MaxHealth;
            float yHealth = bounds.Height / 4;
            Vector2 start = new Vector2(bounds.Width / 5, yHealth - 0.25f);
            Vector2 end = new Vector2(bounds.Width / 5 * 4, yHealth + 0.25f);
            HealthBarRenderer.Draw(context, unit, new Rectangle(start, end - start));
        }
        #endregion
    }
}
