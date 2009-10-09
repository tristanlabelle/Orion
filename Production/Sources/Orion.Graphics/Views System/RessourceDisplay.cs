﻿using System;
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
    public class RessourceDisplay : View
    {
        #region Fields
        Faction faction;
        #endregion

        #region Constructor
        public RessourceDisplay(Rectangle frame, Faction faction)
            :base(frame)
        {
            this.faction = faction;
        }
        #endregion

        #region Methods
        protected override void Draw()
        {
            string ressources = "Alladium: " + faction.AladdiumAmount + "  Allagene: " + faction.AllageneAmount;
            context.FillColor = Color.White;
            context.DrawText(ressources, new Vector2(0,Bounds.Height));
        }
        #endregion
    }
}
