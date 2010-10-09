using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;
using Font = System.Drawing.Font;

namespace Orion.Game.Presentation.Renderers
{
    public sealed class UnitPanelRenderer : IViewRenderer
    {
        #region Fields
        private static readonly ColorRgb progressBarColor = Colors.Gold;
        private static readonly ColorRgb progressBarBorderColor = Colors.Black;
        private static readonly ColorRgb progressBarBackgroundColor = Colors.Black;
        private static readonly ColorRgb borderColor = Colors.Gray;
        private static readonly Font statsFont = new Font("Trebuchet MS", 10);
        private static readonly UnitStat[] statsToDisplay = new[]
        {
            AttackSkill.PowerStat, AttackSkill.RangeStat,
            BasicSkill.MeleeArmorStat, BasicSkill.RangedArmorStat,
            MoveSkill.SpeedStat, BasicSkill.SightRangeStat
        };
        private const float firstLineY = 180;

        private readonly Unit unit;
        private readonly GameGraphics gameGraphics;
        private readonly Faction faction;
        #endregion

        #region Constructors
        public UnitPanelRenderer(Faction faction, Unit unit, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(unit, "unit");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.unit = unit;
            this.gameGraphics = gameGraphics;
            this.faction = faction;
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            bool isTraining = false;

            if (unit.HasSkill<TrainSkill>() && unit.TaskQueue.Current is TrainTask && unit.Faction == faction)
            {
                isTraining = true;

                int firstStartingXPos = 360;
                int firstStartingYPos = 120;

                for (int i = 0; i < unit.TaskQueue.Count; i++)
                {
                    if (i + 1 == 2)
                    {
                        firstStartingXPos = 159;
                        firstStartingYPos = 80;
                    }

                    if (i + 1 == 6)
                    {
                        firstStartingXPos = 159;
                        firstStartingYPos = 35;
                    }

                    // Returns a Task, and finds the unit associated to that Task. 
                    TrainTask train = (TrainTask)unit.TaskQueue[i];
                    Texture texture = gameGraphics.GetUnitTexture(train.TraineeType);

                    Rectangle rect = new Rectangle(firstStartingXPos - 8, firstStartingYPos - 8, 40, 40);
                    context.Fill(rect, Colors.Black);
                    context.Fill(rect, texture, unit.Faction.Color);
                    context.Stroke(rect, Colors.White);

                    // Draws a completion HealthBar
                    Rectangle healthRect = new Rectangle(152, 120, 186, 10);
                    TrainTask currentUnitBeingTrained = (TrainTask)unit.TaskQueue[0];
                    DrawCompletionRect(context, healthRect, currentUnitBeingTrained.Progress);

                    context.Stroke(bounds, borderColor);
                    firstStartingXPos += 50;
                }
            }

            context.Font = statsFont;

            context.Draw(unit.Type.Name, new Vector2(150, firstLineY), Colors.Black);

            Text hp = new Text("HP: {0}/{1}".FormatInvariant((int)unit.Health, unit.MaxHealth), statsFont);
            context.Draw(hp, new Vector2(150, firstLineY - hp.Frame.Height), Colors.Black);

            float y = firstLineY - hp.Frame.Height * 2;
            if (!isTraining)
            {
                foreach (UnitStat stat in statsToDisplay)
                {
                    if (!unit.Type.HasSkill(stat.SkillType)) continue;

                    int value = unit.GetStat(stat);
                    if (value == 0) continue;
                    string message = "{0}: {1} ".FormatInvariant(stat.Description, value);
                    context.Draw(message, new Vector2(150, y), Colors.Black);
                    y -= hp.Frame.Height;
                }
            }

            context.Stroke(bounds, borderColor);
        }

        private static void DrawCompletionRect(GraphicsContext context, Rectangle bounds, float progress)
        {
            context.Fill(bounds, progressBarBackgroundColor);
            Rectangle progressRect = new Rectangle(bounds.MinX, bounds.MinY, bounds.Width * progress, bounds.Height);
            context.Fill(progressRect, progressBarColor);
            context.Stroke(bounds, progressBarBorderColor);
        }
        #endregion
    }
}
