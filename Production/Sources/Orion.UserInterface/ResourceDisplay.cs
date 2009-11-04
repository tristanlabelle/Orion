
using OpenTK.Math;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.Graphics;
using Color = System.Drawing.Color;

namespace Orion.UserInterface
{
    public class ResourceDisplay : View
    {
        #region Fields
        Faction faction;
        #endregion

        #region Constructor
        public ResourceDisplay(Rectangle frame, Faction faction)
            : base(frame)
        {
            this.faction = faction;
        }
        #endregion

        #region Methods
        protected internal override void Draw(GraphicsContext context)
        {
            Text text = new Text("Aladdium: {0}    Alagene: {1}".FormatInvariant(faction.AladdiumAmount, faction.AlageneAmount));
            context.FillColor = Color.Blue;
            context.Fill(Bounds);
            context.FillColor = Color.White;
            context.Draw(text);
        }
        #endregion
    }
}
