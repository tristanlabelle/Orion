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

        private static readonly Dictionary<UnitStat, string> statNames = new Dictionary<UnitStat, string>();

        private static readonly Font statsFont = new Font("Trebuchet MS", 10);
        private const float firstLineY = 160;

        private readonly Unit unit;
        private readonly TextureManager textureManager;
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

        public UnitFrameRenderer(Faction faction, Unit unit, TextureManager textureManager)
        {
            Argument.EnsureNotNull(unit, "unit");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.unit = unit;
            this.textureManager = textureManager;
            this.faction = faction;
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context, Rectangle bounds)
        {
            bool isTraining = false;

            if (unit.HasSkill<Orion.GameLogic.Skills.TrainSkill>() && unit.TaskQueue.Current is TrainTask && unit.Faction == faction)
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
                    Texture texture = textureManager.GetUnit(train.TraineeType.Name);

                    Rectangle rect2 = new Rectangle(firstStartingXPos - 8, firstStartingYPos - 8, 40, 40);
                    context.FillColor = Colors.Black;
                    context.Fill(rect2);
                    context.FillColor = Colors.White;
                    context.Stroke(rect2);

                    // Fills first rectangle with a character. 
                    Rectangle rect = new Rectangle(firstStartingXPos, firstStartingYPos, 26, 26);
                    context.Fill(rect, texture);
                    context.FillColor = unit.Faction.Color;
                    context.Stroke(rect);

                    // Draws a completion HealthBar
                    Rectangle healthRect = new Rectangle(152, 120, 186, 10);
                    TrainTask currentUnitBeingTrained = (TrainTask)unit.TaskQueue[0];
                    DrawCompletionRect(context, healthRect, currentUnitBeingTrained.Progress);

                    base.Draw(context, bounds);
                    firstStartingXPos += 50;
                    context.FillColor = Colors.Black;
                }

                string message = "En progrès...";
                context.Draw(message, new Vector2(150, 5));
            }

            context.Font = statsFont;
            context.FillColor = Colors.DarkBlue;

            context.Draw(unit.Type.Name, new Vector2(150, firstLineY));

            Text hp = new Text("HP: {0}/{1}".FormatInvariant((int)unit.Health, unit.MaxHealth), statsFont);
            context.Draw(hp, new Vector2(150, firstLineY - hp.Frame.Height));

            float y = firstLineY - hp.Frame.Height * 2;
            if (!isTraining)
            {
                foreach (UnitStat stat in statsToDisplay)
                {
                    if (stat == UnitStat.MaxHealth) continue;
                    int value = unit.GetStat(stat);
                    if (value == 0) continue;
                    string message = "{0}: {1} ".FormatInvariant(statNames[stat], value);
                    context.Draw(message, new Vector2(150, y));
                    y -= hp.Frame.Height;
                }
            }
            base.Draw(context, bounds);
        }

        private static void DrawCompletionRect(GraphicsContext context, Rectangle bounds, float progress)
        {
            context.FillColor = Colors.Black;
            context.Fill(bounds);

            Rectangle progressRect = new Rectangle(bounds.MinX, bounds.MinY, bounds.Width * progress, bounds.Height);
            context.FillColor = Colors.Gold;
            context.Fill(progressRect);

            context.StrokeColor = Colors.Black;
            context.Stroke(bounds);
        }
        #endregion
    }
}
