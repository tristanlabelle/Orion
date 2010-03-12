using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Collections;
using Orion.Geometry;
using Orion.GameLogic;
using Orion.GameLogic.Skills;
using Orion.GameLogic.Technologies;
using Orion.Matchmaking.Commands;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Matchmaking
{
    /// <summary>
    /// A commander which generates commands based on user input.
    /// </summary>
    public sealed class UICommander : Commander
    {
        #region Fields
        private static readonly float SingleClickMaxRectangleArea = 0.1f;

        private readonly SelectionManager selectionManager;
        private Unit hoveredUnit;
        private UserInputCommand mouseCommand;
        private Vector2? selectionStart;
        private Vector2? selectionEnd;
        private bool shiftKeyPressed;
        #endregion

        #region Constructors
        public UICommander(Faction faction)
            : base(faction)
        {
            this.selectionManager = new SelectionManager(faction);
            faction.World.Entities.Removed += OnEntityRemoved;
        }
        #endregion

        #region Properties
        public World World
        {
            get { return Faction.World; }
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
            if (!World.IsWithinBounds(point) || Faction.GetTileVisibility(point) != TileVisibility.Visible)
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

            if (clickedUnit.Faction == Faction)
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
            bool otherFactionOnlySelection = selectionManager.SelectedUnits.All(unit => unit.Faction != Faction);
            if (otherFactionOnlySelection) return;

            Point point = (Point)target;
            if (!World.IsWithinBounds(point) || Faction.GetTileVisibility(point) == TileVisibility.Undiscovered)
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
            if (target.Faction == Faction)
            {
                if (target.HasSkill<ExtractAlageneSkill>())
                {
                    if (selectionManager.SelectedUnits.All(unit => unit.Type.IsBuilding && unit.Type.HasSkill<TrainSkill>()))
                        LaunchChangeRallyPoint(target.Center);
                    else
                    {
                        ResourceNode alageneNode = World.Entities
                            .OfType<ResourceNode>()
                            .First(node => node.Position == target.Position);
                        if (alageneNode.IsHarvestableByFaction(Faction))
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
                if (Faction.GetDiplomaticStance(target.Faction) == DiplomaticStance.Ally)
                    LaunchMove(target.Center);
                else
                    LaunchAttack(target);
            }
        }

        private void LaunchDefaultCommand(ResourceNode target)
        {
            if (target.IsHarvestableByFaction(Faction))
                LaunchHarvest(target);
            else
                LaunchMove(target.Position);
        }
        #endregion

        #region Launching individual commands
        public void Cancel()
        {
            if (selectionManager.IsSelectionEmpty) return;

            var unitHandles = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == Faction)
                .Select(unit => unit.Handle);

            if (unitHandles.None()) return;
            
            GenerateCommand(new CancelCommand(Faction.Handle, unitHandles));
        }

        public void LaunchBuild(Point location, UnitType buildingType)
        {
            var builderHandles = selectionManager.SelectedUnits
                .Where(unit =>
                {
                    if (unit.Faction != Faction) return false;
                    BuildSkill buildSkill = unit.GetSkill<BuildSkill>();
                    return buildSkill != null && buildSkill.Supports(buildingType);
                })
                .Select(builder => builder.Handle);

            if (builderHandles.None()) return;
            
            GenerateCommand(new BuildCommand(Faction.Handle, builderHandles, buildingType.Handle, location));
        }

        public void LaunchAttack(Unit target)
        {
            Argument.EnsureNotNull(target, "target");

            var units = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == Faction);

            // Those who can attack do so, the others simply move to the target's position
            var attackingUnitHandles = units
                .Where(unit => unit.HasSkill<AttackSkill>())
                .Select(unit => unit.Handle);
            if (attackingUnitHandles.Any())
                GenerateCommand(new AttackCommand(Faction.Handle, attackingUnitHandles, target.Handle));

            var movingUnitHandles = units
                .Where(unit => !unit.HasSkill<AttackSkill>() && unit.HasSkill<MoveSkill>())
                .Select(unit => unit.Handle);

            if (movingUnitHandles.Any())
                GenerateCommand(new MoveCommand(Faction.Handle, movingUnitHandles, target.Center));
        }

        public void LaunchZoneAttack(Vector2 destination)
        {
            destination = World.ClampWithinSafeBounds(destination);

            var movingUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == Faction && unit.HasSkill<MoveSkill>());

            // Those who can zone attack do so, the others simply move
            var zoneAttackingUnitHandles = movingUnits
                .Where(unit => unit.HasSkill<AttackSkill>())
                .Select(unit => unit.Handle);
            if (zoneAttackingUnitHandles.Any())
                GenerateCommand(new ZoneAttackCommand(Faction.Handle, zoneAttackingUnitHandles, destination));

            var movingUnitHandles = movingUnits
                .Where(unit => !unit.HasSkill<AttackSkill>())
                .Select(unit => unit.Handle);
            if (movingUnitHandles.Any())
                GenerateCommand(new MoveCommand(Faction.Handle, movingUnitHandles, destination));
        }

        public void LaunchHarvest(ResourceNode node)
        {
            var movingUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == Faction && unit.HasSkill<MoveSkill>());

            var harvestingUnitHandles = movingUnits
                .Where(unit => unit.HasSkill<HarvestSkill>())
                .Select(unit => unit.Handle);
            if (harvestingUnitHandles.Any()) GenerateCommand(new HarvestCommand(Faction.Handle, harvestingUnitHandles, node.Handle));

            var movingUnitHandles = movingUnits
                .Where(unit => !unit.HasSkill<HarvestSkill>())
                .Select(unit => unit.Handle);
            if (movingUnitHandles.Any()) GenerateCommand(new MoveCommand(Faction.Handle, harvestingUnitHandles, node.Center));
        }

        public void LaunchMove(Vector2 destination)
        {
            destination = World.ClampWithinSafeBounds(destination);

            var movingUnitHandles = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == Faction && unit.HasSkill<MoveSkill>())
                .Select(unit => unit.Handle);

            if (movingUnitHandles.None()) return;

            GenerateCommand(new MoveCommand(Faction.Handle, movingUnitHandles, destination));
        }

        public void LaunchChangeRallyPoint(Vector2 at)
        {
            var trainerHandles = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == Faction && unit.IsBuilding && unit.HasSkill<TrainSkill>())
                .Select(trainer => trainer.Handle);

            if (trainerHandles.None()) return;

            GenerateCommand(new ChangeRallyPointCommand(Faction.Handle, trainerHandles, at));
        }

        public void LaunchRepair(Unit target)
        {
            if (target.Faction != Faction || !target.IsBuilding) return;

            var repairerHandles = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == Faction && unit.HasSkill<BuildSkill>())
                .Select(unit => unit.Handle);

            if (repairerHandles.None()) return;

            GenerateCommand(new RepairCommand(Faction.Handle, repairerHandles, target.Handle));
        }

        public void LaunchHeal(Unit target)
        {
            if (target.IsBuilding) return;

            var healerHandles = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == Faction && unit.HasSkill<HealSkill>())
                .Select(unit => unit.Handle);

            if (healerHandles.None()) return;

            GenerateCommand(new HealCommand(Faction.Handle, healerHandles, target.Handle));
        }

        public void LaunchTrain(UnitType unitType)
        {
            var trainerHandles = selectionManager.SelectedUnits
                .Where(unit =>
                {
                    if (unit.Faction != Faction || unit.IsUnderConstruction) return false;
                    TrainSkill train = unit.Type.GetSkill<TrainSkill>();
                    return train != null && train.Supports(unitType);
                })
                .Select(unit => unit.Handle);

            if (trainerHandles.None()) return;

            GenerateCommand(new TrainCommand(Faction.Handle, trainerHandles, unitType.Handle));
        }

        public void LaunchResearch(Technology technology)
        {
            var researcher = selectionManager.SelectedUnits
                .FirstOrDefault(unit =>
                {
                    if (unit.IsUnderConstruction || !unit.IsIdle || unit.Faction != Faction)
                        return false;

                    ResearchSkill reseach = unit.Type.GetSkill<ResearchSkill>();
                    return reseach != null && reseach.Supports(technology);
                });
            
            if (researcher == null) return;

            GenerateCommand(new ResearchCommand(Faction.Handle, researcher.Handle, technology.Handle));
        }

        public void LaunchSuicide()
        {
            var suiciderHandles = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == Faction)
                .Select(unit => unit.Handle);

            if (suiciderHandles.None()) return;

            GenerateCommand(new SuicideCommand(Faction.Handle, suiciderHandles));
        }

        public void LaunchStandGuard()
        {
            var guardianHandles = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == Faction && unit.HasSkill<MoveSkill>() && unit.HasSkill<AttackSkill>())
                .Select(unit => unit.Handle);

            if (guardianHandles.None()) return;

            GenerateCommand(new StandGuardCommand(Faction.Handle, guardianHandles));
        }

        public void LaunchChangeDiplomacy(Faction otherFaction, DiplomaticStance newStance)
        {
            if (newStance == Faction.GetDiplomaticStance(otherFaction)) return;

            GenerateCommand(new ChangeDiplomaticStanceCommand(Faction.Handle, otherFaction.Handle, newStance));
        }

        public void LaunchChangeDiplomacy(Faction otherFaction)
        {
            DiplomaticStance newStance = Faction.GetDiplomaticStance(otherFaction) == DiplomaticStance.Ally
                ? DiplomaticStance.Enemy : DiplomaticStance.Ally;
            LaunchChangeDiplomacy(otherFaction, newStance);
        }

        public void SendMessage(string message)
        {
            Argument.EnsureNotNull(message, "message");
            GenerateCommand(new SendMessageCommand(Faction.Handle, message));
        }
        #endregion
        #endregion
    }
}
