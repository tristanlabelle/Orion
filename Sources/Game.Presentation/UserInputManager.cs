using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
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
            this.commander.Faction.World.Entities.Removed += OnEntityRemoved;
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
                mouseCommand = null;
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
            if (args.Key == Keys.ShiftKey) shiftKeyPressed = false;
        }

        private void OnEntityRemoved(EntityManager source, Entity entity)
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
            mouseCommand = null;
        }

        public void LaunchDefaultCommand(Vector2 target)
        {
            if (Selection.Type != SelectionType.Units) return;

            bool otherFactionOnlySelection = Selection.Units.All(unit => unit.Faction != LocalFaction);
            if (otherFactionOnlySelection) return;

            Point point = (Point)target;
            if (!World.IsWithinBounds(point) || LocalFaction.GetTileVisibility(point) == TileVisibility.Undiscovered)
            {
                LaunchMove(target);
                return;
            }

            Entity targetEntity = World.Entities.GetTopmostEntityAt(point);
            if (targetEntity is Unit)
            {
                LaunchDefaultCommand((Unit)targetEntity);
            }
            else if (targetEntity is ResourceNode)
            {
                ResourceNode targetResourceNode = (ResourceNode)targetEntity;
                if (Selection.Units.All(unit => unit.Type.IsBuilding && unit.Type.HasSkill<TrainSkill>()))
                    LaunchChangeRallyPoint(targetResourceNode.Center);
                else
                    LaunchDefaultCommand(targetResourceNode);
            }
            else
            {
                if (Selection.Units.All(unit => unit.Type.IsBuilding && unit.Type.HasSkill<TrainSkill>()))
                    LaunchChangeRallyPoint(target);
                else
                    LaunchMove(target);
            }
        }

        private void LaunchDefaultCommand(Unit target)
        {
            if (Selection.Type != SelectionType.Units) return;

            if (target.Faction == commander.Faction)
            {
                if (target.HasSkill<ExtractAlageneSkill>())
                {
                    if (Selection.Units.All(unit => unit.Type.IsBuilding && unit.Type.HasSkill<TrainSkill>()))
                        LaunchChangeRallyPoint(target.Center);
                    else
                    {
                        ResourceNode alageneNode = World.Entities
                            .OfType<ResourceNode>()
                            .FirstOrDefault(node => node.Position == target.Position);
                        if (alageneNode != null && alageneNode.IsHarvestableByFaction(LocalFaction))
                            LaunchHarvest(alageneNode);
                    }
                }
                else if (target.IsBuilding)
                    LaunchRepair(target);
                else if (!target.IsBuilding && target.Damage > 0)
                    LaunchHeal(target);
                else
                    LaunchMove(target.Position);
            }
            else
            {
                if (LocalFaction.GetDiplomaticStance(target.Faction) == DiplomaticStance.Ally)
                    LaunchMove(target.Center);
                else
                    LaunchAttack(target);
            }
        }

        private void LaunchDefaultCommand(ResourceNode target)
        {
            if (target.IsHarvestableByFaction(LocalFaction))
                LaunchHarvest(target);
            else
                LaunchMove(target.Position);
        }
        #endregion

        #region Launching individual commands
        public void Cancel()
        {
            commander.LaunchCancel(Selection.Units);
        }

        public void LaunchBuild(Point location, UnitType buildingType)
        {
            IEnumerable<Unit> builders = Selection.Units
                .Where(unit => unit.Faction == commander.Faction
                    && unit.Type.CanBuild(buildingType));

            commander.LaunchBuild(builders, buildingType, location);
        }

        public void LaunchAttack(Unit target)
        {
            IEnumerable<Unit> selection = Selection.Units.Where(unit => unit.Faction == commander.Faction);
            // Those who can attack do so, the others simply move to the target's position
            commander.LaunchAttack(selection.Where(unit => unit.HasSkill<AttackSkill>()), target);
            commander.LaunchMove(selection.Where(unit => !unit.HasSkill<AttackSkill>() && unit.HasSkill<MoveSkill>()), target.Position);
        }

        public void LaunchZoneAttack(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = Selection.Units
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<MoveSkill>());
            // Those who can attack do so, the others simply move to the destination
            commander.LaunchZoneAttack(movableUnits.Where(unit => unit.HasSkill<AttackSkill>()), destination);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<AttackSkill>()), destination);
        }

        public void LaunchHarvest(ResourceNode node)
        {
            IEnumerable<Unit> movableUnits = Selection.Units
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<MoveSkill>());
            // Those who can harvest do so, the others simply move to the resource's position
            commander.LaunchHarvest(movableUnits.Where(unit => unit.HasSkill<HarvestSkill>()), node);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<HarvestSkill>()), node.Position);
        }

        public void LaunchMove(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = Selection.Units
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<MoveSkill>());
            commander.LaunchMove(movableUnits, destination);
        }

        public void LaunchChangeRallyPoint(Vector2 at)
        {
            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<TrainSkill>()
                && unit.IsBuilding);
            commander.LaunchChangeRallyPoint(targetUnits, at);
        }

        public void LaunchRepair(Unit building)
        {
            if (building.Faction != LocalFaction || !building.IsBuilding) return;
           
            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == LocalFaction && unit.HasSkill<BuildSkill>());
            commander.LaunchRepair(targetUnits, building);
        }

        public void LaunchHeal(Unit hurtUnit)
        {
            if (hurtUnit.Type.IsBuilding) return;

            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<HealSkill>());
            if (targetUnits.Any(unit => unit.Faction != hurtUnit.Faction)) return;
            commander.LaunchHeal(targetUnits, hurtUnit);
        }

        public void LaunchTrain(UnitType unitType)
        {
            IEnumerable<Unit> trainers = Selection.Units
                .Where(unit => unit.Faction == commander.Faction
                    && !unit.IsUnderConstruction
                    && unit.Type.CanTrain(unitType));

            commander.LaunchTrain(trainers, unitType);
        }

        public void LaunchResearch(Technology technology)
        {
            Unit researcher = Selection.Units
                .FirstOrDefault(unit => unit.Faction == commander.Faction
                    && !unit.IsUnderConstruction
                    && unit.IsIdle
                    && unit.Type.CanResearch(technology));

            commander.LaunchResearch(researcher, technology);
        }

        public void LaunchSuicide()
        {
            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == commander.Faction);
            commander.LaunchSuicide(targetUnits);
        }

        public void LaunchStandGuard()
        {
            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == commander.Faction)
                .Where(unit => unit.HasSkill<MoveSkill>());
            commander.LaunchStandGuard(targetUnits);
        }

        public void LaunchCancel()
        {
            IEnumerable<Unit> targetUnits = Selection.Units
                .Where(unit => unit.Faction == commander.Faction);
            commander.LaunchCancel(targetUnits);
        }

        public void LaunchChangeDiplomacy(Faction targetFaction)
        {
            Argument.EnsureNotNull(targetFaction, "targetFaction");

            DiplomaticStance newStance = LocalFaction.GetDiplomaticStance(targetFaction) == DiplomaticStance.Ally
                ? DiplomaticStance.Enemy : DiplomaticStance.Ally;
            LaunchChangeDiplomacy(targetFaction, newStance);
        }

        public void LaunchChangeDiplomacy(Faction targetFaction, DiplomaticStance newStance)
        {
            Argument.EnsureNotNull(targetFaction, "targetFaction");

            if (LocalFaction.GetDiplomaticStance(targetFaction) == newStance) return;

            commander.LaunchChangeDiplomacy(targetFaction);
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
        #endregion
        #endregion
    }
}
