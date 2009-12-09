using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.GameLogic;
using Orion.GameLogic.Tasks;
using Orion.Geometry;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace Orion.Graphics
{
    public sealed class UnitFrameRenderer : FrameRenderer
    {
        #region Fields
        private static readonly UnitStat[] statsToDisplay = new[]
        {
            UnitStat.AttackPower, UnitStat.AttackRange,
            UnitStat.MeleeArmor, UnitStat.RangedArmor,
            UnitStat.MovementSpeed, UnitStat.SightRange
        };

        private static readonly Font statsFont = new Font("Consolas", 12);

        private readonly Unit unit;
        private readonly UnitsRenderer unitsRenderer;
        private bool unitBeingCreated;
        private const float textLineDistance = 25;
        private const float firstLineY = 160;

        #endregion

        #region Constructors
        public UnitFrameRenderer(Unit unit, UnitsRenderer unitsRenderer)
        {
            Argument.EnsureNotNull(unit, "unit");
            Argument.EnsureNotNull(unitsRenderer, "unitsRenderer");

            this.unit = unit;
            this.unitsRenderer = unitsRenderer;
            this.unitBeingCreated = false; 
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context)
        {
            if (unit.HasSkill<Orion.GameLogic.Skills.TrainSkill>() && unit.TaskQueue.Current != null)
            {
                unitBeingCreated = true;
                int firstStartingXPos = 360;
                int firstStartingYPos = 120;
                if (unit.TaskQueue.Count >= 1)
                {
                    for (int i = 0; i < unit.TaskQueue.Count; i++)
                    {
                        if (i+1 == 2)
                        {
                            firstStartingXPos = 159;
                            firstStartingYPos = 80;
                        }
                        if (i+1 == 6)
                        {
                            firstStartingXPos = 159;
                            firstStartingYPos = 35;
                        }

                        // Returns a Task, and finds the unit associated to that Task. 
                        TrainTask train = (TrainTask)unit.TaskQueue[i];
                        Texture texture = unitsRenderer.GetTypeTexture(train.TraineeType);

                        Rectangle rect2 = new Rectangle(firstStartingXPos - 8, firstStartingYPos - 8, 40, 40);
                        context.FillColor = Color.Black;
                        context.Fill(rect2);
                        context.FillColor = Color.White;
                        context.Stroke(rect2);

                        // Fills first rectangle with a character. 
                        Rectangle rect = new Rectangle(firstStartingXPos, firstStartingYPos, 26, 26);
                        context.Fill(rect, texture);
                        context.FillColor = unit.Faction.Color;
                        context.Stroke(rect);

                        // Draws a completion HealthBar
                        Rectangle healthRect = new Rectangle(152 ,120, 186, 10);
                        TrainTask currentUnitBeingTrained = (TrainTask)unit.TaskQueue.ElementAt(0);
                        DrawCompletionRect(context, healthRect,currentUnitBeingTrained.Progress);

                        base.Draw(context);
                        firstStartingXPos += 50;
                        context.FillColor = Color.Black;
                     
                    }
                    string message = "In progress...";
                    context.Draw(message, new Vector2(150, 5));
                }
            }
            if (unit.TaskQueue.IsEmpty)
                unitBeingCreated = false;
            context.Font = statsFont;
            context.FillColor = Color.DarkBlue;
            context.Draw(unit.Type.Name, new Vector2(150, firstLineY));
            string hp = "HP: {0}/{1}".FormatInvariant((int)unit.Health, unit.MaxHealth);
            context.Draw(hp, new Vector2(150, firstLineY - textLineDistance));
            float y = firstLineY - textLineDistance * 2;
            if (!unitBeingCreated)
            {
                foreach (UnitStat stat in statsToDisplay)
                {
                    if (stat == UnitStat.MaxHealth) continue;
                    int value = unit.GetStat(stat);
                    if (value == 0) continue;
                    string message = "{0}: {1}".FormatInvariant(Casing.CamelToWords(stat.ToStringInvariant()), value);
                    context.Draw(message, new Vector2(150, y));
                    y -= textLineDistance;
                }
            }
            base.Draw(context);
        }
        private static void DrawCompletionRect(GraphicsContext context, Rectangle bounds, float progress)
        {
            context.FillColor = Color.Black;
            context.Fill(bounds);

            Rectangle progressRect = new Rectangle(bounds.MinX, bounds.MinY, bounds.Width * progress, bounds.Height);
            context.FillColor = Color.Gold;
            context.Fill(progressRect);

            context.StrokeColor = Color.Black;
            context.Stroke(bounds);
        }
        #endregion
    }
}
