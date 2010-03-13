using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.GameLogic.Tasks;
using Orion.Geometry;
using Font = System.Drawing.Font;
using Orion.GameLogic.Skills;

namespace Orion.Graphics.Renderers
{
    public sealed class UnitFrameRenderer : FrameRenderer
    {
        #region Fields
        private static readonly ColorRgb progressBarColor = Colors.Gold;
        private static readonly ColorRgb progressBarBorderColor = Colors.Black;
        private static readonly ColorRgb progressBarBackgroundColor = Colors.Black;
        private static readonly Font statsFont = new Font("Trebuchet MS", 10);
        private static readonly UnitStat[] statsToDisplay = new[]
        {
            UnitStat.AttackPower, UnitStat.AttackRange,
            UnitStat.MeleeArmor, UnitStat.RangedArmor,
            UnitStat.MovementSpeed, UnitStat.SightRange
        };
        private static readonly Dictionary<UnitStat, string> statNames = new Dictionary<UnitStat, string>();
        private const float firstLineY = 180;

        private readonly Unit unit;
        private readonly GameGraphics gameGraphics;
        private readonly Faction faction;
        #endregion

        #region Constructors
        static UnitFrameRenderer()
        {
            statNames[UnitStat.AladdiumCost] = "Coût d'Aladdium";
            statNames[UnitStat.AlageneCost] = "Coût d'Alagène";
            statNames[UnitStat.AttackDelay] = "Délai d'attaque";
            statNames[UnitStat.AttackPower] = "Puissance d'attaque";
            statNames[UnitStat.AttackRange] = "Portée d'attaque";
            statNames[UnitStat.BuildingSpeed] = "Vitesse de construction";
            statNames[UnitStat.ExtractingSpeed] = "Vitesse d'extraction";
            statNames[UnitStat.FoodStorageCapacity] = "Capacité de stockage";
            statNames[UnitStat.HealSpeed] = "Vitesse de soin";
            statNames[UnitStat.MeleeArmor] = "Armure au corps-à-corps";
            statNames[UnitStat.RangedArmor] = "Armure à distance";
            statNames[UnitStat.MovementSpeed] = "Vitesse de mouvement";
            statNames[UnitStat.SightRange] = "Portée de vision";
        }

        public UnitFrameRenderer(Faction faction, Unit unit, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(unit, "unit");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.unit = unit;
            this.gameGraphics = gameGraphics;
            this.faction = faction;
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context, Rectangle bounds)
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

                    base.Draw(context, bounds);
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
                    if (stat == UnitStat.MaxHealth) continue;
                    int value = unit.GetStat(stat);
                    if (value == 0) continue;
                    string message = "{0}: {1} ".FormatInvariant(statNames[stat], value);
                    context.Draw(message, new Vector2(150, y), Colors.Black);
                    y -= hp.Frame.Height;
                }
            }

            base.Draw(context, bounds);
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
