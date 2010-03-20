using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Geometry;
using Orion.GameLogic;
using Orion.GameLogic.Technologies;
using Keys = System.Windows.Forms.Keys;
using Orion.GameLogic.Utilities;

namespace Orion.Matchmaking
{
    public class UserInputManager
    {
        #region Fields
        private static readonly float SingleClickMaxRectangleArea = 0.1f;

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
        public UserInputManager(SlaveCommander commander)
        {
            Argument.EnsureNotNull(commander, "commander");

            this.commander = commander;
            this.underAttackMonitor = new UnderAttackMonitor(commander.Faction);
            this.selectionManager = new SelectionManager(commander.Faction);
            this.commander.Faction.World.Entities.Removed += OnEntityRemoved;
        }
        #endregion

        #region Properties
        [Obsolete("To be fully encapsulated by this UserInputManager")]
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
            get { return LocalFaction.World; }
        }

        public UnderAttackMonitor UnderAttackMonitor
        {
            get { return underAttackMonitor; }
        }

        public SelectionManager SelectionManager
        {
            get { return selectionManager; }
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
                if (args.ButtonPressed == MouseButton.Left)
                    LaunchMouseCommand(args.Position);
                mouseCommand = null;
                return;
            }

            if (args.ButtonPressed == MouseButton.Left)
                selectionStart = args.Position;
            else if (args.ButtonPressed == MouseButton.Right)
                LaunchDefaultCommand(args.Position);
        }

        public void HandleMouseUp(MouseEventArgs args)
        {
            if (!this.selectionStart.HasValue) return;
            if (args.ButtonPressed != MouseButton.Left) return;

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
                    selectionManager.AddRectangleToSelection(selectionRectangleStart, selectionRectangleEnd);
                else
                    selectionManager.SelectInRectangle(selectionRectangleStart, selectionRectangleEnd);
            }
        }

        private void HandleMouseClick(Vector2 position)
        {
            Point point = (Point)position;
            Unit clickedUnit = World.IsWithinBounds(point)
                ? World.Entities.GetTopmostUnitAt(point)
                : null;

            if (clickedUnit == null)
            {
                if (!shiftKeyPressed) selectionManager.ClearSelection();
                return;
            }

            if (shiftKeyPressed)
                selectionManager.ToggleSelection(clickedUnit);
            else
                selectionManager.SetSelection(clickedUnit);
        }

        public void HandleMouseDoubleClick(MouseEventArgs args)
        {
            if ((args.ButtonPressed & MouseButton.Left) == 0) return;

            selectionStart = null;
            selectionEnd = null;

            Point point = (Point)args.Position;
            if (!World.IsWithinBounds(point) || LocalFaction.GetTileVisibility(point) != TileVisibility.Visible)
            {
                selectionManager.ClearSelection();
                return;
            }

            Unit clickedUnit = World.Entities.GetTopmostUnitAt(point);
            if (clickedUnit == null)
            {
                selectionManager.ClearSelection();
                return;
            }

            if (clickedUnit.Faction == LocalFaction)
                selectionManager.SelectNearbyUnitsOfType(clickedUnit.Center, clickedUnit.Type);
            else
                selectionManager.SetSelection(clickedUnit);
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
                if (args.HasControl)
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
            bool otherFactionOnlySelection = selectionManager.SelectedUnits.All(unit => unit.Faction != LocalFaction);
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
                if (selectionManager.SelectedUnits.All(unit => unit.Type.IsBuilding && unit.Type.HasSkill(UnitSkill.Train)))
                    LaunchChangeRallyPoint(targetResourceNode.Center);
                else
                    LaunchDefaultCommand(targetResourceNode);
            }
            else
            {
                if (selectionManager.SelectedUnits.All(unit => unit.Type.IsBuilding && unit.Type.HasSkill(UnitSkill.Train)))
                    LaunchChangeRallyPoint(target);
                else
                    LaunchMove(target);
            }
        }

        private void LaunchDefaultCommand(Unit target)
        {
            if (target.Faction == commander.Faction)
            {
                if (target.HasSkill(UnitSkill.ExtractAlagene))
                {
                    if (selectionManager.SelectedUnits.All(unit => unit.Type.IsBuilding && unit.Type.HasSkill(UnitSkill.Train)))
                        LaunchChangeRallyPoint(target.Center);
                    else
                    {
                        ResourceNode alageneNode = World.Entities
                            .OfType<ResourceNode>()
                            .First(node => node.Position == target.Position);
                        if (alageneNode.IsHarvestableByFaction(LocalFaction))
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
            commander.LaunchCancel(selectionManager.SelectedUnits);
        }

        public void LaunchBuild(Point location, UnitType buildingType)
        {
            IEnumerable<Unit> builders = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction
                    && unit.Type.HasSkill(UnitSkill.Build)
                    && unit.Type.CanBuild(buildingType));

            commander.LaunchBuild(builders, buildingType, location);
        }

        public void LaunchAttack(Unit target)
        {
            IEnumerable<Unit> selection = selectionManager.SelectedUnits.Where(unit => unit.Faction == commander.Faction);
            // Those who can attack do so, the others simply move to the target's position
            commander.LaunchAttack(selection.Where(unit => unit.HasSkill(UnitSkill.Attack)), target);
            commander.LaunchMove(selection.Where(unit => !unit.HasSkill(UnitSkill.Attack) && unit.HasSkill(UnitSkill.Move)), target.Position);
        }

        public void LaunchZoneAttack(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill(UnitSkill.Move));
            // Those who can attack do so, the others simply move to the destination
            commander.LaunchZoneAttack(movableUnits.Where(unit => unit.HasSkill(UnitSkill.Attack)), destination);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill(UnitSkill.Attack)), destination);
        }

        public void LaunchHarvest(ResourceNode node)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill(UnitSkill.Move));
            // Those who can harvest do so, the others simply move to the resource's position
            commander.LaunchHarvest(movableUnits.Where(unit => unit.HasSkill(UnitSkill.Harvest)), node);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill(UnitSkill.Harvest)), node.Position);
        }

        public void LaunchMove(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill(UnitSkill.Move));
            commander.LaunchMove(movableUnits, destination);
        }

        public void LaunchChangeRallyPoint(Vector2 at)
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill(UnitSkill.Train)
                && unit.IsBuilding);
            commander.LaunchChangeRallyPoint(targetUnits, at);
        }

        public void LaunchRepair(Unit building)
        {
            if (building.Faction != LocalFaction || !building.IsBuilding) return;
           
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == LocalFaction && unit.HasSkill(UnitSkill.Build));
            commander.LaunchRepair(targetUnits, building);
        }

        public void LaunchHeal(Unit hurtUnit)
        {
            if (hurtUnit.Type.IsBuilding) return;

            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill(UnitSkill.Heal));
            if (targetUnits.Any(unit => unit.Faction != hurtUnit.Faction)) return;
            commander.LaunchHeal(targetUnits, hurtUnit);
        }

        public void LaunchTrain(UnitType unitType)
        {
            IEnumerable<Unit> trainers = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction
                    && !unit.IsUnderConstruction
                    && unit.Type.CanTrain(unitType));

            commander.LaunchTrain(trainers, unitType);
        }

        public void LaunchResearch(Technology technology)
        {
            Unit researcher = selectionManager.SelectedUnits
                .FirstOrDefault(unit => unit.Faction == commander.Faction
                    && !unit.IsUnderConstruction
                    && unit.IsIdle
                    && unit.Type.CanResearch(technology));

            commander.LaunchResearch(researcher, technology);
        }

        public void LaunchSuicide()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction);
            commander.LaunchSuicide(targetUnits);
        }

        public void LaunchStandGuard()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction)
                .Where(unit => unit.HasSkill(UnitSkill.Move));
            commander.LaunchStandGuard(targetUnits);
        }

        public void LaunchCancel()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
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
        #endregion
        #endregion
    }
}
