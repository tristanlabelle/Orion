using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections; 

using OpenTK.Math;
using Orion.GameLogic;
using Orion.GameLogic.Tasks;

namespace Orion.Graphics
{
    public sealed class UnitFrameRenderer : FrameRenderer
    {
        #region Fields
        private static readonly UnitStat[] statsToDisplay = new[]
            { UnitStat.AttackPower, UnitStat.AttackRange, UnitStat.MovementSpeed, UnitStat.SightRange };

        private readonly Unit unit;
        private readonly UnitButtonRenderer buttonRenderer;
        private readonly UnitsRenderer unitRenderer;
        private readonly TextureManager textureManager;
        private Train trainTask; 
        
        #endregion

        #region Constructors
        public UnitFrameRenderer(Unit unit)
        {
            Argument.EnsureNotNull(unit, "unit");
            this.unit = unit;
            this.textureManager = new TextureManager(@"../../../Assets"); 
            this.unitRenderer= new UnitsRenderer(unit.World,unit.Faction,textureManager);
            this.trainTask = null; 
           // this.buttonRenderer = new UnitButtonRenderer(unitsRenderer, this.unit); 
            
        }
        #endregion

        #region Methods
        public override void Draw(GraphicsContext context)
        {
            if (unit.HasSkill<Orion.GameLogic.Skills.Train>() && unit.TaskQueue.Current != null)
            {

                int firstStartingXPos=150; 
                int firstStartingYPos=60;
                Queue<Task> queuesInFactory = new Queue<Task>(unit.TaskQueue);
                if(queuesInFactory.Count>=1)
                {
                    for (int i = 0; i < queuesInFactory.Count; i++)
                    {
                        if (i == 4)
                        {
                            firstStartingXPos = 150;
                            firstStartingYPos = 10;
                        }
                        // Returns a Task, and finds the unit assosiated to that Task. 
                        Task t = queuesInFactory.ElementAt(i);
                        if (t is Train)
                            trainTask = (Train)t;
                        Argument.EnsureNotNull(trainTask, "train");
                        Texture texture = this.unitRenderer.GetTypeTexture(trainTask.TraineeType);

                        Orion.Geometry.Rectangle rect2 = new Orion.Geometry.Rectangle(firstStartingXPos - 8, firstStartingYPos - 8, 48, 48);
                        context.FillColor = Color.Black;
                        context.Fill(rect2);
                        context.FillColor = Color.White;
                        context.Stroke(rect2);
                        
                        
                        // Fills first rectangle with a caracter. 
                        Orion.Geometry.Rectangle rect = new Orion.Geometry.Rectangle(firstStartingXPos, firstStartingYPos, 32, 32);
                        context.Fill(rect, texture);
                        context.FillColor = unit.Faction.Color;
                        context.Stroke(rect);


                        // Draws a completion HealthBar
                        Orion.Geometry.Rectangle healthRect = new Orion.Geometry.Rectangle(300, 150, 180* trainTask.Progress, 10);
                        context.FillColor = Color.Gold;
                        context.Fill(healthRect);
                        context.FillColor = Color.Black;
                        context.Stroke(healthRect);


                        base.Draw(context);
                        firstStartingXPos += 60;
                        
                    
                    }

                }
              
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
