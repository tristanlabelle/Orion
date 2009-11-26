using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Collections; 

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
            // If a baraque is selected and that it is currently executing a task. 
            if (unit.Type.Name == "Baraque" && unit.Task != null)
            {
               /*
                //UnitButtonRenderer buttonRenderer = new UnitButtonRenderer(, unit);
                //Button unitButton = new Button(new Rectangle(10, 10, 130, 175), "", buttonRenderer);

                int firstStartingXPos=150; 
                int firstStartingYPos=60;
                Queue<Task> queusInFactory = new Queue<Task>(unit.TaskQueue);
                if (queusInFactory.Count == 5)
                {
                    firstStartingXPos = 150;
                    firstStartingYPos = 10;
                }
                foreach(Task t in queusInFactory)
                {
                    Orion.Geometry.Rectangle rect = new Orion.Geometry.Rectangle(firstStartingXPos, firstStartingYPos, 50, 50);
                    context.FillColor = Color.Black;
                    context.Fill(rect);
                    context.FillColor = Color.White;
                    context.Stroke(rect);
                    base.Draw(context);
                    firstStartingXPos += 70;
                }
                * */

            }
     
            string hp = "HP: {0}/{1}".FormatInvariant(unit.Health, unit.MaxHealth);
            context.FillColor = Color.DarkBlue;
            context.Draw(unit.Type.Name, new Vector2(150, 155));
            context.Draw(hp, new Vector2(150, 130));
            base.Draw(context);
        }
    }
}
