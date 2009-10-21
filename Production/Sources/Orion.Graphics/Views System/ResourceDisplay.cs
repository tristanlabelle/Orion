using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Graphics;
using Orion.GameLogic;
using Orion.Geometry;

using Color = System.Drawing.Color;
using OpenTK.Math;

namespace Orion.Graphics
{
    public class ResourceDisplay : View
    {
        #region Fields
        Faction faction;
        #endregion

        #region Constructor
        public ResourceDisplay(Rectangle frame, Faction faction)
            :base(frame)
        {
            this.faction = faction;
        }
        #endregion

        #region Methods
        protected override void Draw()
        {
            string resources = "Alladium: " + faction.AladdiumAmount + "  Alagene: " + faction.AlageneAmount;
            context.FillColor = Color.Black;
            context.Fill(Bounds);
            context.FillColor = Color.White;            context.DrawText(resources, new Vector2(0, Bounds.Height));
        }
        #endregion
    }
}
