using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK.Math;
using Orion.GameLogic;


namespace Orion.Graphics
{
    public class UnitFrameRenderer : FrameRenderer
    {
        private Unit unit;

        public UnitFrameRenderer(Unit unit)
        {
            this.unit = unit;
        }

        public override void Draw(GraphicsContext context)
        {
            // If a factory is selected and that it is currently executing a task. 
            // if (unit.Type.Name=="Factory" && unit.Task!=null)
            //{
             /* Orion.Geometry.Rectangle rect = new Orion.Geometry.Rectangle(150,10,60,100);
                context.FillColor = Color.Black; 
                context.Fill(rect);
                context.FillColor = Color.White;
                context.Stroke(rect);
                base.Draw(context); 
            // context.Draw(
             */  
            // }
            string hp = "HP: {0}/{1}".FormatInvariant(unit.Health, unit.MaxHealth);
            context.FillColor = Color.DarkBlue;
            context.Draw(unit.Type.Name, new Vector2(150, 155));
            context.Draw(hp, new Vector2(150, 130));
            base.Draw(context);
        }
    }
}
