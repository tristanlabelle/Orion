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
        private readonly Unit unit;
        private readonly TextureManager textureManager;
        private bool hasFocus;
        #endregion

        #region Constructors
        public UnitButtonRenderer(Unit unit, TextureManager textureManager)
            : base(unit.Faction.Color)
        {
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.unit = unit;
            this.textureManager = textureManager;
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
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context, Rectangle bounds)
        {
            context.StrokeColor = hasFocus ? Colors.White : Colors.Black;
            context.FillColor = hasFocus ? ColorRgb.FromBytes(75, 75, 75) : Colors.Black;
            context.Fill(bounds);
            context.Stroke(bounds);

            context.StrokeColor = StrokeColor;

            float size = bounds.Width * 3 / 4;
            Rectangle rectangle = Rectangle.FromCenterSize(
                bounds.CenterX, bounds.Height * 5 / 8,
                size, size);

            Texture texture = textureManager.GetUnit(unit.Type.Name);
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
