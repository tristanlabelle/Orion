using Color = System.Drawing.Color;
using OpenTK.Math;
using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics
{
    public sealed class UnitButtonRenderer : FrameRenderer
    {
        #region Fields
        private readonly UnitsRenderer unitRenderer;
        private bool hasFocus;
        public readonly Unit unit;
        #endregion

        #region Constructors
        public UnitButtonRenderer(UnitsRenderer unitRenderer, Unit unit)
            : base(unit.Faction.Color)
        {
            this.unitRenderer = unitRenderer;
            
            this.unit = unit;
        }
        #endregion

        #region Properties
        public bool HasFocus
        {
            get { return hasFocus; }
            set { hasFocus = value; }
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context)
        {
            context.StrokeColor = hasFocus ? Color.White : Color.Black;
            context.FillColor = hasFocus ? Color.FromArgb(75, 75, 75) : Color.Black;
            context.Fill(context.CoordinateSystem);
            context.Stroke(context.CoordinateSystem);

            context.StrokeColor = StrokeColor;

            float size = context.CoordinateSystem.Width * 3 / 4;
            Rectangle rectangle = Rectangle.FromCenterSize(
                context.CoordinateSystem.CenterX, context.CoordinateSystem.Height * 5 / 8,
                size, size);

            Texture texture = unitRenderer.GetTypeTexture(unit.Type);
            context.Fill(rectangle, texture, unit.Faction.Color);

            float healthRatio = unit.Health / unit.MaxHealth;
            float yHealth = context.CoordinateSystem.Height / 4;
            Vector2 start = new Vector2(context.CoordinateSystem.Width / 5, yHealth - 0.25f);
            Vector2 end = new Vector2(context.CoordinateSystem.Width / 5 * 4, yHealth + 0.25f);
            unitRenderer.DrawHealthBar(context, unit, new Rectangle(start, end - start));
        }
        #endregion
    }
}
