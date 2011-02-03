using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;
using Orion.Engine.Input;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Technologies;
using Orion.Game.Simulation.Utilities;
using Keys = System.Windows.Forms.Keys;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Handles the keyboard and mouse input on the world.
    /// </summary>
    public sealed class UserInputManager
    {
        #region Fields
        private static readonly float SingleClickMaxRectangleArea = 0.1f;

        private readonly Match match;
        private readonly SlaveCommander commander;
        private readonly UnderAttackMonitor underAttackMonitor;
        private readonly SelectionManager selectionManager;
        private Unit hoveredUnit;
        private UserInputCommand mouseCommand;
        private Vector2? selectionStart;
        private Vector2? selectionEnd;
        private bool shiftKeyPressed;
        private bool launchedCommandsWithShift;
        #endregion

        #region Constructors
        public UserInputManager(Match match, SlaveCommander commander)
        {
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(commander, "commander");

            this.match = match;
            this.commander = commander;
            this.underAttackMonitor = new UnderAttackMonitor(commander.Faction);
            this.selectionManager = new SelectionManager(commander.Faction);
            this.commander.Faction.World.EntityRemoved += OnEntityRemoved;
        }
        #endregion

        #region Properties
        public Match Match
        {
            get { return match; }
        }

        public Commander LocalCommander
        {
            get { return commander; }
        }

        public Faction LocalFaction
        {
            get { return commander.Faction; }
        }

        public World World
        {
            get { return match.World; }
        }

        public UnderAttackMonitor UnderAttackMonitor
        {
            get { return underAttackMonitor; }
        }

        public SelectionManager SelectionManager
        {
            get { return selectionManager; }
        }

        public Selection Selection
        {
            get { return selectionManager.Selection; }
        }

        public Unit HoveredUnit
        {
            get { return hoveredUnit; }
            set { hoveredUnit = value; }
        }

        public Rectangle? SelectionRectangle
        {
            get
            {
                if (selectionStart.HasValue && selectionEnd.HasValue)
                    return new Rectangle(selectionStart.Value, selectionEnd.Value - selectionStart.Value);
                return null;
            }
        }

        public UserInputCommand SelectedCommand
        {
            get { return mouseCommand; }
            set { mouseCommand = value; }
        }
        #endregion

        #region Methods
        #region Direct Event Handling
        public void HandleMouseDown(MouseEventArgs args)
        {
            if (mouseCommand != null)
            {
                if (args.Button == MouseButton.Left)
                    LaunchMouseCommand(args.Position);

                if (!shiftKeyPressed)
                    mouseCommand = null;
                else
                    launchedCommandsWithShift = true;

                return;
            }

            if (args.Button == MouseButton.Left)
            {
                if (args.ClickCount > 1)
                    HandleMouseMultiClick(args);
                else
                    selectionStart = args.Position;
            }
            else if (args.Button == MouseButton.Right)
            {
                LaunchDefaultCommand(args.Position);
            }
        }

        public void HandleMouseUp(MouseEventArgs args)
        {
            if (!this.selectionStart.HasValue) return;
            if (args.Button != MouseButton.Left) return;

            this.selectionEnd = args.Position;
            Vector2 selectionRectangleStart = this.selectionStart.Value;
            Vector2 selectionRectangleEnd = this.selectionEnd.Value;
            Rectangle selectionRectangle = this.SelectionRectangle.Value;
            this.selectionStart = null;
            this.selectionEnd = null;

            if (selectionRectangle.Area < SingleClickMaxRectangleArea)
            {
                HandleMouseClick(selectionRectangleStart);
            }
            else
            {
                if (shiftKeyPressed)
                    Selection.AddUnitsInRectangle(selectionRectangleStart, selectionRectangleEnd);
                else
                    Selection.SetToRectangle(selectionRectangleStart, selectionRectangleEnd);
            }
        }

        private void HandleMouseClick(Vector2 position)
        {
            Point point = (Point)position;
            Entity clickedEntity = World.IsWithinBounds(point)
                ? World.Entities.Intersecting(position).WithMaxOrDefault(e => e.CollisionLayer)
                : null;

            if (clickedEntity == null)
            {
                if (!shiftKeyPressed) Selection.Clear();
                return;
            }

            if (shiftKeyPressed)
                Selection.Toggle(clickedEntity);
            else
                Selection.Set(clickedEntity);
        }

        private void HandleMouseMultiClick(MouseEventArgs args)
        {
            if (args.Button != MouseButton.Left) return;

            selectionStart = null;
            selectionEnd = null;

            Point point = (Point)args.Position;
            if (!World.IsWithinBounds(point) || LocalFaction.GetTileVisibility(point) != TileVisibility.Visible)
            {
                Selection.Clear();
                return;
            }

            Unit clickedUnit = World.Entities.GetTopmostUnitAt(point);
            if (clickedUnit == null)
            {
                Selection.Clear();
                return;
            }

            if (clickedUnit.Faction == LocalFaction)
                Selection.SetToNearbySimilar(clickedUnit);
            else
                Selection.Set(clickedUnit);
        }

        public void HandleMouseMove(MouseEventArgs args)
        {
            if (mouseCommand != null)
            {
                mouseCommand.OnMouseMoved(args.Position);
                return;
            }

            if (selectionStart.HasValue) selectionEnd = args.Position;
            else
            {
                Point point = (Point)args.Position;
                hoveredUnit = World.IsWithinBounds(point) ? World.Entities.GetTopmostUnitAt(point) : null;
            }
        }

        public void HandleKeyDown(KeyboardEventArgs args)
        {
            switch (args.Key)
            {
                case Keys.Escape: mouseCommand = null; break;
                case Keys.ShiftKey: shiftKeyPressed = true; break;
                case Keys.Delete: LaunchSuicide(); break;
            }

            if (args.Key >= Keys.D0 && args.Key <= Keys.D9)
            {
                int groupNumber = args.Key - Keys.D0;
                if (args.IsControlModifierDown)
                    selectionManager.SaveSelectionGroup(groupNumber);
                else
                    selectionManager.TryLoadSelectionGroup(groupNumber);
            }
        }

        public void HandleKeyUp(KeyboardEventArgs args)
        {
            if (args.Key == Keys.ShiftKey)
            {
                shiftKeyPressed = false;
                if (launchedCommandsWithShift)
                {
                    launchedCommandsWithShift = false;
                    mouseCommand = null;
                }
            }
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            if (unit == hoveredUnit) hoveredUnit = null;
        }
        #endregion

        #region Launching dynamically chosen commands
        public void LaunchMouseCommand(Vector2 location)
        {
            mouseCommand.OnClick(location);
            if (!shiftKeyPressed)
                mouseCommand = null;
            else
                launchedCommandsWithShift = true;
        }

        public void LaunchDefaultCommand(Vector2 target)
        {
            if (Selection.Type != SelectionType.Units) return;
            if (!Selection.Units.Any(unit => IsUnitControllable(unit))) return;

            Point point = (Point)target;
            if (!World.IsWithinBounds(point))
            {
                LaunchMove(target);
                return;
            }
            
            if (LocalFaction.GetTileVisibility(point) == TileVisibility.Undiscovered)
            {
                if (Selection.Units.All(unit => unit.Type.IsBuilding))
                    LaunchChangeRallyPoint(target);
                else
                    LaunchMove(target);
                return;
            }

            Entity targetEntity = World.Entities.GetTopmostEntityAt(point);
            if (targetEntity is Unit)
            {
                LaunchDefaultCommand((Unit)targetEntity);
            }
            else if (targetEntity != null && targetEntity.HasComponent<Harvestable>())
            {
                Spatial position = targetEntity.GetComponent<Spatial>();
                if (Selection.Units.All(unit => unit.Type.IsBuilding))
                    LaunchChangeRallyPoint(position.Center);
                else
                    LaunchDefaultCommand(targetEntity);
            }
            else
            {
                Debug.Assert(targetEntity == null);
                if (Selection.Units.All(unit => unit.Type.IsBuilding))
                    LaunchChangeRallyPoint(target);
                else
                    LaunchMove(target);
            }
        }

        private void LaunchDefaultCommand(Unit target)
        {
            if (Selection.Type != SelectionType.Units) return;

            if (LocalFaction.GetDiplomaticStance(target.Faction) == DiplomaticStance.Enemy)
            {
                LaunchAttack(target);
                return;
            }

            if (target.HasSkill<ExtractAlageneSkill>())
            {
                if (Selection.Units.All(unit => unit.Type.IsBuilding))
                {
                    LaunchChangeRallyPoint(target.Center);
                    return;
                }

                Entity alageneNode = World.Entities
                    .Intersecting(Rectangle.FromCenterSize(target.Position, Vector2.One))
                    .Where(e => e.HasComponent<Harvestable>())
                    .FirstOrDefault(node => node.Position == target.Position);
                if (alageneNode != null && LocalFaction.CanHarvest(alageneNode))
                {
                    LaunchHarvest(alageneNode);
                    return;
                }
            }
            else if (target.HasSkill<TransportSkill>())
            {
                LaunchEmbark(target);
                return;
            }
            else if (target.Damage > 0)
            {
                if (target.IsBuilding) LaunchRepair(target);
                else LaunchHeal(target);

                return;
            }
            
            LaunchMove(target.Position);
        }

        private void LaunchDefaultCommand(Entity target)
        {
            Debug.Assert(target.HasComponent<Harvestable>(), "Target is not harvestable!");
            if (LocalFaction.CanHarvest(target))
                LaunchHarvest(target);
            else
                LaunchMove(target.Position);
        }
        #endregion

        #region Launching individual commands
        private bool IsUnitControllable(Unit unit)
        {
            return LocalFaction.GetDiplomaticStance(unit.Faction).HasFlag(DiplomaticStance.SharedControl);
        }

        private void OverrideIfNecessary()
        {
            if (!shiftKeyPressed)
            {
                IEnumerable<Unit> units = Selection.Units
                    .Where(unit => IsUnitControllable(unit) && !unit.Type.IsBuilding);
                commander.LaunchCancel(units);
            }
        }

        public void LaunchBuild(Point location, Unit buildingType)
        {
            IEnumerable<Unit> builders = Selection.Units
                .Where(unit => IsUnitControllable(unit) && unit.Type.CanBuild(buildingType));

            OverrideIfNecessary();
            commander.LaunchBuild(builders, buildingType, location);
        }

        public void LaunchAttack(Unit target)
        {
            IEnumerable<Unit> selection = Selection.Units.Where(unit => IsUnitControllable(unit));
            OverrideIfNecessary();
            // Those who can attack do so, the others simply move to the target's position
            commander.LaunchAttack(selection.Where(unit => unit.HasSkill<AttackSkill>()), target);
            commander.LaunchMove(selection.Where(unit => !unit.HasSkill<AttackSkill>() && unit.HasSkill<MoveSkill>()), target.Position);
        }

        public void LaunchZoneAttack(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = Selection.Units
                .Where(unit => IsUnitControllable(unit) && unit.HasSkill<MoveSkill>());
            // Those who can attack do so, the others simply move to the destination
            OverrideIfNecessary();
            commander.LaunchZoneAttack(movableUnits.Where(unit => unit.HasSkill<AttackSkill>()), destination);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<AttackSkill>()), destination);
        }

        public void LaunchHarvest(Entity node)
        {
            Debug.Assert(node.HasComponent<Harvestable>(), "Node is not a resource node!");
            IEnumerable<Unit> movableUnits = Selection.Units
                .Where(unit => IsUnitControllable(unit) && unit.HasSkill<MoveSkill>());
            // Those who can harvest do so, the others simply move to the resource's position
            OverrideIfNecessary();
            commander.LaunchHarvest(movableUnits.Where(unit => unit.HasSkill<HarvestSkill>()), node);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<HarvestSkill>()), node.Position);
        }

        public void LaunchMove(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = Selection.Units
                .Where(unit => IsUnitControllable(unit) && unit.HasSkill<MoveSkill>());
            OverrideIfNecessary();
            commander.LaunchMove(movableUnits, destination);
        }

        public void LaunchChangeRallyPoint(Vector2 at)
        {
            IEnumerable<Unit> targets = Selection.Units
                .Where(unit => unit.Faction == LocalFaction
                    && unit.IsBuilding
                    && unit.HasSkill<TrainSkill>());
            OverrideIfNecessary();
            commander.LaunchChangeRallyPoint(targets, at);
        }

        public void LaunchRepair(Unit building)
        {
            Argument.EnsureNotNull(building, "building");

            if (!building.IsBuilding) return;
           
            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == LocalFaction && unit.HasSkill<BuildSkill>());
            OverrideIfNecessary();
            commander.LaunchRepair(targetUnits, building);
        }

        public void LaunchHeal(Unit target)
        {
            Argument.EnsureNotNull(target, "target");
            if (target.Type.IsBuilding) return;

            IEnumerable<Unit> healers = Selection.Units
                .Where(unit => unit.Faction == LocalFaction && unit.HasSkill<HealSkill>());
            if (healers.Any(unit => unit.Faction != target.Faction)) return;
            OverrideIfNecessary();
            commander.LaunchHeal(healers, target);
        }

        public void LaunchTrain(Unit unitType)
        {
            IEnumerable<Unit> trainers = Selection.Units
                .Where(unit => IsUnitControllable(unit)
                    && !unit.IsUnderConstruction
                    && unit.Type.CanTrain(unitType));

            OverrideIfNecessary();
            commander.LaunchTrain(trainers, unitType);
        }

        public void LaunchResearch(Technology technology)
        {
            Unit researcher = Selection.Units
                .FirstOrDefault(unit => unit.Faction == commander.Faction // I unilaterally decided you can't research from buildings that aren't yours
                    && !unit.IsUnderConstruction
                    && unit.IsIdle
                    && unit.Type.CanResearch(technology));

            OverrideIfNecessary();
            commander.LaunchResearch(researcher, technology);
        }

        public void LaunchSuicide()
        {
            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == LocalFaction && unit.Type.IsSuicidable);
            OverrideIfNecessary();
            commander.LaunchSuicide(targetUnits);
        }

        public void LaunchStandGuard()
        {
            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == LocalFaction)
                .Where(unit => unit.HasSkill<MoveSkill>());
            OverrideIfNecessary();
            commander.LaunchStandGuard(targetUnits);
        }

        public void LaunchSell()
        {
            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == LocalFaction)
                .Where(unit => unit.HasSkill<SellableSkill>());
            commander.LaunchSuicide(targetUnits);
        }

        public void LaunchCancel()
        {
            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == LocalFaction);
            commander.LaunchCancel(targetUnits);
        }

        public void LaunchChangeDiplomacy(Faction targetFaction, DiplomaticStance newStance)
        {
            Argument.EnsureNotNull(targetFaction, "targetFaction");

            if (LocalFaction.GetDiplomaticStance(targetFaction) == newStance) return;
            
            commander.LaunchChangeDiplomacy(targetFaction, newStance);
        }

        public void LaunchChatMessage(string text)
        {
            Argument.EnsureNotNull(text, "text");

            commander.SendMessage(text);
        }

        public void LaunchAllyChatMessage(string text)
        {
            Argument.EnsureNotNull(text, "text");

            commander.SendAllyMessage(text);
        }

        public void LaunchUpgrade(Unit targetType)
        {
            Argument.EnsureNotNull(targetType, "targetType");

            var targetUnits = Selection.Units
                .Where(unit => unit.Faction == LocalFaction && unit.Type.Upgrades.Any(upgrade => upgrade.Target == targetType.Name));
            commander.LaunchUpgrade(targetUnits, targetType);
        }

        public void LaunchDisembark()
        {
            var transporters = Selection.Units
                .Where(unit => unit.Faction == LocalFaction && unit.HasSkill<TransportSkill>());
            OverrideIfNecessary();
            commander.LaunchDisembark(transporters);
        }

        public void LaunchEmbark(Unit transporter)
        {
            Argument.EnsureNotNull(transporter, "transporter");

            var embarkers = Selection.Units
                .Where(unit => unit.Faction == LocalFaction && transporter.Type.CanTransport(unit.Type));
            OverrideIfNecessary();
            commander.LaunchEmbark(embarkers, transporter);
        }
        #endregion
        #endregion
    }
}
