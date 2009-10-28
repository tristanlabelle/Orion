using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Graphics;
using Orion.GameLogic;
using Orion.Geometry;

using Color = System.Drawing.Color;
using OpenTK.Math;

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
            string resources = "Alladium: " + faction.AladdiumAmount + "  Alagene: " + faction.AlageneAmount;
            context.FillColor = Color.Blue;
            context.Fill(Bounds);
            context.FillColor = Color.White;
            context.DrawText(resources, new Vector2(0, Bounds.Height));
        }
        #endregion
    }
}
