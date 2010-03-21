using System;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.GameLogic;
using Orion.Graphics;

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
        protected override void Draw(GraphicsContext context)
        {
            Text text = new Text("Aladdium: {0}    Alagene: {1}    Population: {2}/{3}"
                                .FormatInvariant(faction.AladdiumAmount, faction.AlageneAmount, faction.UsedFoodAmount, faction.MaxFoodAmount));
            context.Fill(Bounds, Colors.Blue);
            context.Draw(text, Colors.White);
        }
        #endregion
    }
}
