using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Geometry;
using Orion.GameLogic;
using Skills = Orion.GameLogic.Skills;

using OpenTK.Math;
using Orion.GameLogic.Skills;

namespace Orion.Commandment
{
    public abstract class UserInputCommand
    {
        public abstract void Execute(Entity entity);
        public abstract void Execute(Vector2 point);
    }

    public abstract class ImmediateUserCommand
    {
        public abstract void Execute();
    }

    public class UserInputManager
    {
        #region Fields
        private SlaveCommander commander;
        private SelectionManager selectionManager;
        private UserInputCommand mouseCommand;
        private Vector2? selectionStart;
        private Vector2? selectionEnd;
        private bool shiftKeyPressed;
        private List<Unit>[] groups;
        #endregion

        #region Constructors
        public UserInputManager(SlaveCommander userCommander)
        {
            Argument.EnsureNotNull(userCommander, "userCommander");
            commander = userCommander;
            commander.Faction.World.Entities.Removed += OnEntityDied;
            selectionManager = new SelectionManager(userCommander.Faction);

            groups = new List<Unit>[10];
            for (int i = 0; i < groups.Length;i++ )
            {
                groups[i] = new List<Unit>();
            }
        }
        #endregion

        #region Properties
        public SlaveCommander Commander
        {
            get { return commander; }
        }

        public SelectionManager SelectionManager
        {
            get { return selectionManager; }
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

        private Faction Faction
        {
            get { return commander.Faction; }
        }

        private World World
        {
            get { return Faction.World; }
        }
        #endregion

        #region Methods
        #region Direct Event Handling
        public void HandleMouseDown(object responder, MouseEventArgs args)
        {
            if (mouseCommand != null)
            {
                if (args.ButtonPressed == MouseButton.Left) LaunchMouseCommand(args.Position);
                mouseCommand = null;
            }
            else
            {
                if (args.ButtonPressed == MouseButton.Left) selectionStart = args.Position;
                else if (args.ButtonPressed == MouseButton.Right) LaunchDefaultCommand(args.Position);
            }
        }

        public void HandleMouseDoubleClick(object responder, MouseEventArgs args)
        {
            selectionStart = args.Position;
            selectionEnd = args.Position;
            Faction faction = commander.Faction;
            Rectangle selection = SelectionRectangle.Value;
            //TO BE OPTIMIZED
            Unit selectedUnit = faction.World.Entities
                .OfType<Unit>()
                .FirstOrDefault(unit => Rectangle.Intersects(selection, unit.BoundingRectangle));

            if (selectedUnit != null && selectedUnit.Faction == faction)
            {
                IEnumerable<Unit> selectedUnits = faction.World.Entities
                    .OfType<Unit>()
                    .Where(unit => (unit.Position - args.Position).Length < 10
                            && unit.Type == selectedUnit.Type
                            && unit.Faction == faction);
                selectionManager.SelectUnits(selectedUnits);
            }
            selectionStart = null;
            selectionEnd = null;
            
        }

        public void HandleMouseUp(object responder, MouseEventArgs args)
        {
            if (!selectionStart.HasValue) return;
            if (args.ButtonPressed != MouseButton.Left) return;

            selectionEnd = args.Position;
            Faction faction = commander.Faction;
            Rectangle selection = SelectionRectangle.Value;
            List<Unit> selectedUnits = faction.World.Entities
                .OfType<Unit>()
                .Where(unit => Rectangle.Intersects(selection, unit.BoundingRectangle))
                .OrderBy(unit => (unit.Center - selectionStart.Value).LengthSquared)
                .ToList();

            // Filter out factions
            bool containsUnitsFromThisFaction = selectedUnits.Any(unit => unit.Faction == faction);
            if (containsUnitsFromThisFaction)
                selectedUnits.RemoveAll(unit => unit.Faction != faction);
            else if (selectedUnits.Count > 1)
                selectedUnits.RemoveRange(1, selectedUnits.Count - 1);

            // Filter out buildings
            bool containsNonBuildingUnits = selectedUnits.Any(unit => !unit.Type.IsBuilding);
            if (containsNonBuildingUnits) selectedUnits.RemoveAll(unit => unit.Type.IsBuilding);

            if (shiftKeyPressed) selectionManager.AppendToSelection(selectedUnits);
            else selectionManager.SelectUnits(selectedUnits);

            selectionStart = null;
            selectionEnd = null;
        }

        public void HandleMouseMove(object responder, MouseEventArgs args)
        {
            if (selectionStart.HasValue) selectionEnd = args.Position;
            else
            {
                Vector2 point = new Vector2(args.X, args.Y);
                SelectionManager.HoveredUnit = Faction.World.Entities
                    .OfType<Unit>()
                    .FirstOrDefault(unit => unit.BoundingRectangle.ContainsPoint(point));
            }
        }

        public void HandleKeyDown(object responder, KeyboardEventArgs args)
        {
            switch (args.Key)
            {
                case Keys.Escape: mouseCommand = null; break;
                case Keys.ShiftKey: shiftKeyPressed = true; break;
                case Keys.Delete: LaunchSuicide(); break;
                case Keys.F9: LaunchChangeDimplomacy(); break;

                case Keys.D0: case Keys.D1: case Keys.D2:
                case Keys.D3: case Keys.D4: case Keys.D5:
                case Keys.D6: case Keys.D7: case Keys.D8:
                case Keys.D9:
                    int groupNumer = args.Key - Keys.D0;
                    if (args.HasControl)
                    {
                        groups[groupNumer] = selectionManager.SelectedUnits.ToList();
                    }
                    else if (groups[groupNumer].Count > 0)
                    {
                        selectionManager.SelectUnits(groups[groupNumer]);
                    }
                    break;
            }
        }

        public void HandleKeyUp(object responder, KeyboardEventArgs args)
        {
            if (args.Key == Keys.ShiftKey) shiftKeyPressed = false;
        }

        private void OnEntityDied(EntityManager sender, Entity args)
        {
            Unit unit = args as Unit;
            if (unit == null) return;
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i].Remove(unit);
            }
        }
        #endregion

        #region Launching dynamically chosen commands
        public void LaunchMouseCommand(Vector2 at)
        {
            Entity intersectedEntity = World.Entities
                .FirstOrDefault(entity => entity.BoundingRectangle.ContainsPoint(at));

            if (intersectedEntity == null) mouseCommand.Execute(at);
            else mouseCommand.Execute(intersectedEntity);

            mouseCommand = null;
        }

        public void LaunchDefaultCommand(Vector2 at)
        {
            bool otherFactionOnlySelection = selectionManager.SelectedUnits.All(unit => unit.Faction != Faction);
            if (otherFactionOnlySelection) return;

            Entity intersectedEntity = World.Entities
                .FirstOrDefault(entity => entity.BoundingRectangle.ContainsPoint(at));

            Unit intersectedUnit = intersectedEntity as Unit;
            if (intersectedUnit != null)
            {
                // TODO: implement better friendliness checks
                if (intersectedUnit.Faction == commander.Faction)
                {
                    if (intersectedUnit.HasSkill<Skills.ExtractAlageneSkill>())
                    {
                        ResourceNode alageneNode = World.Entities
                            .OfType<ResourceNode>()
                            .First(node => node.Position == intersectedUnit.Position);
                        if (alageneNode.IsHarvestableByFaction(this.Faction))
                            LaunchHarvest(alageneNode);
                    }
                    else if (intersectedUnit.IsBuilding)
                        LaunchRepair(intersectedUnit);
                    else
                        LaunchMove(intersectedUnit.Position);
                }
                else LaunchAttack(intersectedUnit);
            }
            else if (intersectedEntity is ResourceNode)
            {
                if (((ResourceNode)intersectedEntity).IsHarvestableByFaction(this.Faction))
                    LaunchHarvest((ResourceNode)intersectedEntity);
                else
                    LaunchMove(((ResourceNode)intersectedEntity).Position);
            }
            else
            {
                if (World.Terrain.IsWalkableAndWithinBounds((Point)at))
                {
                    if (selectionManager.SelectedUnits.All(unit => unit.Type.IsBuilding && unit.Type.HasSkill<Skills.TrainSkill>()))
                    {
                        LaunchChangeRallyPoint(at);
                    }
                }
                LaunchMove(at);
            }
        }

        #endregion

        #region Launching individual commands
        public void Cancel()
        {
            commander.LaunchCancel(selectionManager.SelectedUnits);
        }

        public void LaunchBuild(Vector2 destination, UnitType unitTypeToBuild)
        {
            IEnumerable<Unit> builders = selectionManager.SelectedUnits.Where(unit =>
            {
                Skills.BuildSkill build = unit.GetSkill<Skills.BuildSkill>();
                Skills.MoveSkill move = unit.GetSkill<Skills.MoveSkill>();
                if (build == null) return false;
                if (unit.Faction != commander.Faction) return false;
                if (move == null) return false;
                return build.Supports(unitTypeToBuild);
            });


            commander.LaunchBuild(builders, unitTypeToBuild, (Point)destination);
        }

        public void LaunchAttack(Unit target)
        {
            IEnumerable<Unit> selection = selectionManager.SelectedUnits.Where(unit => unit.Faction == commander.Faction);
            // Those who can attack do so, the others simply move to the target's position
            commander.LaunchAttack(selection.Where(unit => unit.HasSkill<Skills.AttackSkill>()), target);
            commander.LaunchMove(selection.Where(unit => !unit.HasSkill<Skills.AttackSkill>() && unit.HasSkill<Skills.MoveSkill>()), target.Position);
        }

        public void LaunchZoneAttack(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.MoveSkill>());
            // Those who can attack do so, the others simply move to the destination
            commander.LaunchZoneAttack(movableUnits.Where(unit => unit.HasSkill<Skills.AttackSkill>()), destination);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<Skills.AttackSkill>()), destination);
        }

        public void LaunchHarvest(ResourceNode node)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.MoveSkill>());
            // Those who can harvest do so, the others simply move to the resource's position
            commander.LaunchHarvest(movableUnits.Where(unit => unit.HasSkill<Skills.HarvestSkill>()), node);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<Skills.HarvestSkill>()), node.Position);
        }

        public void LaunchMove(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.MoveSkill>());
            commander.LaunchMove(movableUnits, destination);
        }

        public void LaunchChangeRallyPoint(Vector2 at)
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.TrainSkill>()
                && unit.Type.IsBuilding);
            commander.LaunchChangeRallyPoint(targetUnits, at);
        }

        public void LaunchRepair(Unit building)
        {
            if (!building.Type.IsBuilding) return;
            if (building.Damage < 1) return;
           
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.BuildSkill>());
            if (targetUnits.Any(unit => unit.Faction != building.Faction)) return;
            commander.LaunchRepair(targetUnits, building);
        }

        public void LaunchTrain(UnitType unitType)
        {
            IEnumerable<Unit> trainers = selectionManager.SelectedUnits
                .Where(unit =>
                {
                    if (unit.Faction != commander.Faction) return false;
                    Skills.TrainSkill train = unit.Type.GetSkill<Skills.TrainSkill>();
                    if (train == null) return false;
                    if (unit.IsUnderConstruction) return false;
                    return train.Supports(unitType);
                });
            commander.LaunchTrain(trainers, unitType);
        }

        public void LaunchSuicide()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction);
            commander.LaunchSuicide(targetUnits);
        }

        public void LaunchChangeDimplomacy()
        {
            // For Now I just Test Ally
            Faction otherFaction = World.Factions.FirstOrDefault(faction => faction.Name == "Cyan");
            commander.LaunchChangeDiplomacy(otherFaction);
        }

        public void LaunchCancel()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction);
            commander.LaunchCancel(targetUnits);
        }
        #endregion
        #endregion
    }
}
