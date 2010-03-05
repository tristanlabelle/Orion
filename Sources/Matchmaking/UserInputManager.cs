using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Geometry;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.GameLogic.Technologies;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Matchmaking
{
    public class UserInputManager
    {
        #region Fields
        private static readonly float SingleClickMaxRectangleArea = 0.1f;

        private readonly SlaveCommander commander;
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
            this.selectionManager = new SelectionManager(commander.Faction);
            commander.Faction.World.Entities.Removed += OnEntityRemoved;
        }
        #endregion

        #region Properties
        public SlaveCommander LocalCommander
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
        public void HandleMouseDown(object responder, MouseEventArgs args)
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

        public void HandleMouseUp(object responder, MouseEventArgs args)
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

        public void HandleMouseDoubleClick(object responder, MouseEventArgs args)
        {
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

        public void HandleMouseMove(object responder, MouseEventArgs args)
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

        public void HandleKeyDown(object responder, KeyboardEventArgs args)
        {
            switch (args.Key)
            {
                case Keys.Escape: mouseCommand = null; break;
                case Keys.ShiftKey: shiftKeyPressed = true; break;
                case Keys.Delete: LaunchSuicide(); break;
                case Keys.F9: ChangeDiplomaticStance(); break;
            }

            if (args.Key >= Keys.D0 && args.Key <= Keys.D9)
            {
                int groupNumber = args.Key - Keys.D0;
                if (args.HasControl) selectionManager.SaveSelectionGroup(groupNumber);
                else selectionManager.TryLoadSelectionGroup(groupNumber);
            }
        }

        public void HandleKeyUp(object responder, KeyboardEventArgs args)
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
                if (selectionManager.SelectedUnits.All(unit => unit.Type.IsBuilding && unit.Type.HasSkill<TrainSkill>()))
                    LaunchChangeRallyPoint(targetResourceNode.Center);
                else
                    LaunchDefaultCommand(targetResourceNode);
            }
            else
            {
                if (selectionManager.SelectedUnits.All(unit => unit.Type.IsBuilding && unit.Type.HasSkill<TrainSkill>()))
                    LaunchChangeRallyPoint(target);
                else
                    LaunchMove(target);
            }
        }

        private void LaunchDefaultCommand(Unit target)
        {
            if (target.Faction == commander.Faction)
            {
                if (target.HasSkill<ExtractAlageneSkill>())
                {
                    ResourceNode alageneNode = World.Entities
                        .OfType<ResourceNode>()
                        .First(node => node.Position == target.Position);
                    if (alageneNode.IsHarvestableByFaction(LocalFaction))
                        LaunchHarvest(alageneNode);
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

        public void LaunchBuild(Point location, UnitType unitTypeToBuild)
        {
            IEnumerable<Unit> builders = selectionManager.SelectedUnits.Where(unit =>
            {
                BuildSkill buildSkill = unit.GetSkill<BuildSkill>();
                MoveSkill moveSkill = unit.GetSkill<MoveSkill>();
                if (buildSkill == null) return false;
                if (unit.Faction != commander.Faction) return false;
                if (moveSkill == null) return false;
                return buildSkill.Supports(unitTypeToBuild);
            });


            commander.LaunchBuild(builders, unitTypeToBuild, location);
        }

        public void LaunchAttack(Unit target)
        {
            IEnumerable<Unit> selection = selectionManager.SelectedUnits.Where(unit => unit.Faction == commander.Faction);
            // Those who can attack do so, the others simply move to the target's position
            commander.LaunchAttack(selection.Where(unit => unit.HasSkill<AttackSkill>()), target);
            commander.LaunchMove(selection.Where(unit => !unit.HasSkill<AttackSkill>() && unit.HasSkill<MoveSkill>()), target.Position);
        }

        public void LaunchZoneAttack(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<MoveSkill>());
            // Those who can attack do so, the others simply move to the destination
            commander.LaunchZoneAttack(movableUnits.Where(unit => unit.HasSkill<AttackSkill>()), destination);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<AttackSkill>()), destination);
        }

        public void LaunchHarvest(ResourceNode node)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<MoveSkill>());
            // Those who can harvest do so, the others simply move to the resource's position
            commander.LaunchHarvest(movableUnits.Where(unit => unit.HasSkill<HarvestSkill>()), node);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<HarvestSkill>()), node.Position);
        }

        public void LaunchMove(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<MoveSkill>());
            commander.LaunchMove(movableUnits, destination);
        }

        public void LaunchChangeRallyPoint(Vector2 at)
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<TrainSkill>()
                && unit.IsBuilding);
            commander.LaunchChangeRallyPoint(targetUnits, at);
        }

        public void LaunchRepair(Unit building)
        {
            if (building.Faction != LocalFaction) return;
            if (!building.IsBuilding) return;
            if (building.Damage < 1) return;
           
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == LocalFaction && unit.HasSkill<BuildSkill>());
            commander.LaunchRepair(targetUnits, building);
        }

        public void LaunchEmbark(Unit target)
        {
            if (target.Faction != LocalFaction || target.IsBuilding) return;

            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == LocalFaction && unit.HasSkill<MoveSkill>());
            commander.LaunchEmbark(targetUnits, target);
        }

        public void LaunchDisembark()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == LocalFaction && unit.HasSkill<TransportSkill>());
            commander.LaunchDisembark(targetUnits);
        }

        public void LaunchHeal(Unit hurtUnit)
        {
            if (hurtUnit.Type.IsBuilding) return;
            if (hurtUnit.Damage < 1) return;

            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<HealSkill>());
            if (targetUnits.Any(unit => unit.Faction != hurtUnit.Faction)) return;
            commander.LaunchHeal(targetUnits, hurtUnit);
        }

        public void LaunchTrain(UnitType unitType)
        {
            IEnumerable<Unit> trainers = selectionManager.SelectedUnits
                .Where(unit =>
                {
                    if (unit.Faction != commander.Faction) return false;
                    TrainSkill train = unit.Type.GetSkill<TrainSkill>();
                    if (train == null) return false;
                    if (unit.IsUnderConstruction) return false;
                    return train.Supports(unitType);
                });
            commander.LaunchTrain(trainers, unitType);
        }

        public void LaunchResearch(Technology technology)
        {
            Unit tristan = selectionManager.SelectedUnits
                .FirstOrDefault(unit =>
                {
                    if (unit.Faction != commander.Faction) return false;
                    ResearchSkill reseach = unit.Type.GetSkill<ResearchSkill>();
                    if (reseach == null) return false;
                    if (unit.IsUnderConstruction) return false;
                    if (!unit.IsIdle) return false;
                    return reseach.Supports(technology);
                });

                commander.LaunchResearch(tristan, technology);
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
                .Where(unit => unit.HasSkill<MoveSkill>());
            commander.LaunchStandGuard(targetUnits);
        }

        public void LaunchCancel()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction);
            commander.LaunchCancel(targetUnits);
        }

        public void ChangeDiplomaticStance()
        {
            // For Now I just Test Ally
            Faction otherFaction = World.Factions.FirstOrDefault(faction => faction.Name == "Cyan");
            commander.LaunchChangeDiplomacy(otherFaction);
        }
        #endregion
        #endregion
    }
}
