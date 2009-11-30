using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections; 

using OpenTK.Math;
using Orion.GameLogic;

namespace Orion.Graphics
{
    public sealed class UnitFrameRenderer : FrameRenderer
    {
        #region Fields
        private static readonly UnitStat[] statsToDisplay = new[]
            { UnitStat.AttackPower, UnitStat.AttackRange, UnitStat.MovementSpeed, UnitStat.SightRange };

        private readonly Unit unit;
        #endregion

        #region Constructors
        public UnitFrameRenderer(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            this.unit = unit;
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context)
        {
            // If a baraque is selected and that it is currently executing a task. 
            if (unit.Type.Name == "Baraque" && unit.CurrentTask != null)
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

            const float textLineDistance = 25;
            const float firstLineY = 160;

            context.FillColor = Color.DarkBlue;
            context.Draw(unit.Type.Name, new Vector2(150, firstLineY));
            string hp = "HP: {0}/{1}".FormatInvariant((int)unit.Health, unit.MaxHealth);
            context.Draw(hp, new Vector2(150, firstLineY - textLineDistance));

            float y = firstLineY - textLineDistance * 2;
            foreach (UnitStat stat in statsToDisplay)
            {
                if (stat == UnitStat.MaxHealth) continue;
                int value = unit.GetStat(stat);
                if (value == 0) continue;
                string message = "{0}: {1}".FormatInvariant(Casing.CamelToWords(stat.ToStringInvariant()), value);
                context.Draw(message, new Vector2(150, y));
                y -= textLineDistance;
            }

            base.Draw(context);
        }
        #endregion
    }
}
