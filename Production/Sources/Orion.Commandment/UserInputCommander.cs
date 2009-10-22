using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.Graphics;
using OpenTK.Math;
using Orion.Commandment.Commands;
using Orion.Commandment;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Graphics
{
    /// <summary>
    /// A <see cref="Commander"/> which gives <see cref="Command"/>s based on user input.
    /// </summary>
    public sealed class UserInputCommander : Commander
    {
        #region Fields
        private SelectionManager selectionManager;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor For a commander that can listen input to create commands
        /// </summary>
        /// <param name="faction">the faction of the player.</param>
        public UserInputCommander(Faction faction)
            : base(faction)
        {
            this.selectionManager = new SelectionManager(faction);
        }
        #endregion

        #region Proprieties
        /// <summary>
        /// Gets the <see cref="SelectionManager"/> this <see cref="UserInputCommander"/>
        /// uses internally to detect selection state.
        /// </summary>
        public SelectionManager SelectionManager
        {
            get { return selectionManager; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Listen of Mouse Event, implemented the right click associated to the move task
        /// </summary>
        /// <param name="position">The location of the cursor when the click occured.</param>
        /// <param name="button">The mouse button that was pressed or released.</param>
        /// <param name="pressed">True if the button was pressed, false otherwise.</param>
        public void OnMouseButton(Vector2 position, MouseButton button, bool pressed)
        {
            selectionManager.OnMouseButton(position, button, pressed);
            if (button == MouseButton.Right && selectionManager.CtrlKeyPressed && pressed)
            {
                HandleCtrlRightClick(position);
            }
            
            else if (button == MouseButton.Middle && pressed)
            {
                HandleMiddleClick(position);
            }
            else if (button == MouseButton.Left && selectionManager.CtrlKeyPressed && pressed)
            {
                HandleCtrlLeftClick(position);
            }
            else if (button == MouseButton.Right && pressed)
            {
                HandleRightClick(position);
            }

        }



        /// <summary>
        /// The Function Called When a right mouse click happend
        /// </summary>
        /// <param name="position"></param>
        private void HandleRightClick(Vector2 position)
        {
            if (selectionManager.SelectedUnits.Count() != 0)
            {
                List<Unit> unitsToAssignTask = selectionManager.SelectedUnits.Where(unit => unit.Faction == Faction).ToList();
                if (unitsToAssignTask.Count() != 0)
                {
                    Unit enemy = World.Units.FirstOrDefault(unit => unit.Circle.ContainsPoint(position));
                    ResourceNode node = World.ResourceNodes.FirstOrDefault(resourceNode => resourceNode.Circle.ContainsPoint(position));
                    Command command;
                    if (enemy != null && enemy.Faction != this.Faction)// TODO: CHECK IF Its Not Either an ally.
                    {
                        command = new Attack(Faction, unitsToAssignTask, enemy);
                    }
                    // Assigns a gathering task
                    else if (node != null)
                    {
                        if (!node.IsHarvestable)
                            return;
                        command = new Harvest(Faction, unitsToAssignTask, node);
                    }
                    else
                    {
                        command = new Move(Faction, unitsToAssignTask, position);
                    }
                    GenerateCommand(command);
                }
            }
        }

        /// <summary>
        /// The Function Called When a Middle mouse Click happend
        /// </summary>
        /// <param name="position"></param>
        private void HandleMiddleClick(Vector2 position)
        {
            if (selectionManager.SelectedUnits.Count() != 0)
            {
                List<Unit> unitsToAssignTask = selectionManager.SelectedUnits.Where(unit => unit.Faction == Faction).ToList();
                if (unitsToAssignTask.Count() != 0)
                {
                    GenerateCommand(new ZoneAttack(Faction, unitsToAssignTask, position));
                }
            }
        }

        /// <summary>
        /// The Function Called When a Ctrl + Left Click happend
        /// </summary>
        /// <param name="position"></param>
        private void HandleCtrlLeftClick(Vector2 position)
        { 
            // Build Command
            if (World.Terrain.IsWalkable((int)position.X, (int)position.Y))
            {
                Unit builder = selectionManager.SelectedUnits.FirstOrDefault(unit => unit.Faction == Faction);
                
                if (builder != null)
                {
                    UnitType unitToBuild = new UnitType("building");
                    if (Faction.AladdiumAmount >= unitToBuild.AladdiumPrice && Faction.AlageneAmount >= unitToBuild.AlagenePrice)
                    {
                        GenerateCommand(new Build(builder, position,unitToBuild));
                    }
                }

            }
        }

        private void HandleCtrlRightClick(Vector2 position)
        {
            ResourceNode node = World.ResourceNodes.FirstOrDefault(resourceNode => resourceNode.Circle.ContainsPoint(position));
            Unit builder = selectionManager.SelectedUnits.FirstOrDefault(unit => unit.Faction == Faction);

            if (builder != null && node != null && node.ResourceType == ResourceType.Alagene)
            {
                UnitType extractor = new UnitType("extractor");
                if (Faction.AladdiumAmount >= extractor.AladdiumPrice && Faction.AlageneAmount >= extractor.AlagenePrice)
                {
                    GenerateCommand(new Build(builder, node.Circle.Center, extractor));
                    node.IsHarvestable = true;
                }
            }
        }

        public override void Update(float timeDelta)
        {
            (commandsEntryPoint as CommandSink).Flush();
        }

        public override void AddToPipeline(CommandPipeline pipeline)
        {
            pipeline.AddCommander(this);

            commandsEntryPoint = new CommandOptimizer(pipeline.UserCommandmentEntryPoint);
            commandsEntryPoint = new CommandAggregator(commandsEntryPoint);
        }

        /// <summary>
        /// Parses KeyDown events to capture those whose key has a special meaning.
        /// </summary>
        /// <param name="key">
        /// The <see cref="Keys"/> that were pressedd
        /// </param>
        public void OnKeyDown(Keys key)
        {
            if (key == Keys.S)
            {
                if (selectionManager.SelectedUnits.Count() != 0)
                {
                    List<Unit> unitsToAssignTask = selectionManager.SelectedUnits.Where(unit => unit.Faction == Faction).ToList();
                    if (unitsToAssignTask.Count() != 0)
                    {
                        Command command = new Cancel(Faction, unitsToAssignTask);
                        GenerateCommand(command);
                    }
                }
            }
            else if (key == Keys.ControlKey)
            {
                selectionManager.OnCtrlKeyChanged(true);
            }
        }

        /// <summary>
        /// Parses KeyUp events to capture those whose key has a special meaning.
        /// </summary>
        /// <param name="key">
        /// The <see cref="Keys"/> that were pressedd
        /// </param>
        public void OnKeyUp(Keys key)
        {
            if (key == Keys.ControlKey)
            {
                selectionManager.OnCtrlKeyChanged(false);
            }
        }

        /// <summary>
        /// Parses a MouseMove event.
        /// </summary>
        /// <param name="position">
        /// The position in form of a <see cref="Vector2"/>
        /// </param>
        public void OnMouseMove(Vector2 position)
        {
            selectionManager.OnMouseMove(position);
        }
        #endregion


    }
}
