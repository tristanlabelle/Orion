using Color = System.Drawing.Color;
using OpenTK.Math;
using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics
{
    public class UnitButtonRenderer : FrameRenderer
    {
        private readonly UnitsRenderer renderer;
        private bool hasFocus;
        public readonly Texture texture;
        public readonly Unit unit;

        public UnitButtonRenderer(UnitsRenderer unitRenderer, Unit unit)
        {
            renderer = unitRenderer;
            texture = unitRenderer.GetTypeTexture(unit.Type);
            this.unit = unit;
            StrokeColor = unit.Faction.Color;
        }

        public bool HasFocus
        {
            get { return hasFocus; }
            set { hasFocus = value; }
        }

        public override void Draw(GraphicsContext context)
        {
            context.StrokeColor = hasFocus ? Color.White : Color.Black;
            context.FillColor = hasFocus ? Color.FromArgb(75, 75, 75) : Color.Black;
            context.Fill(context.CoordinateSystem);
            context.Stroke(context.CoordinateSystem);

            context.StrokeColor = StrokeColor;

            float width = context.CoordinateSystem.Width/ 4 *3;
            float height = context.CoordinateSystem.Height / 4*3;
            float x =  context.CoordinateSystem.Width / 8;
            float y = context.CoordinateSystem.Height / 4;
            context.Fill(new Rectangle(x,y , width, height), texture, unit.Faction.Color);

            float healthRatio = unit.Health / unit.MaxHealth;
            float yHealth = context.CoordinateSystem.Height / 4;
            Vector2 start = new Vector2(context.CoordinateSystem.Width / 5, yHealth - 0.25f);
            Vector2 end = new Vector2(context.CoordinateSystem.Width / 5 * 4, yHealth + 0.25f);
            renderer.DrawHealthBar(context, unit, new Rectangle(start, end - start));
        }
    }
}
