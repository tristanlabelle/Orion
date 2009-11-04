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
    public enum MouseDrivenCommand
    {
        Attack, Build, Harvest, Move, Repair, ZoneAttack, Train, Suicide, Cancel
    }

    public class UserInputManager
    {
        #region Fields
        // TODO
        // use unit skills for key mapping instead of a static keys map
        private static Dictionary<Keys, MouseDrivenCommand?> keysMap;

        private UserInputCommander commander;
        private SelectionManager selectionManager;
        private MouseDrivenCommand? mouseCommand;
        private Vector2? selectionStart;
        private Vector2? selectionEnd;
        private bool shiftKeyPressed;
        #endregion

        #region Constructors
        public UserInputManager(UserInputCommander userCommander)
        {
            commander = userCommander;
            selectionManager = new SelectionManager(userCommander.Faction);
        }

        static UserInputManager()
        {
            keysMap = new Dictionary<Keys, MouseDrivenCommand?>();
            keysMap[Keys.A] = MouseDrivenCommand.Attack;
            keysMap[Keys.B] = MouseDrivenCommand.Build;
            keysMap[Keys.G] = MouseDrivenCommand.Harvest; // "G"ather
            keysMap[Keys.M] = MouseDrivenCommand.Move;
            keysMap[Keys.Escape] = null;
        }
        #endregion

        #region Properties
        public UserInputCommander Commander
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

        public MouseDrivenCommand? SelectedCommand
        {
            get { return mouseCommand; }
        }
        #endregion

        #region Methods
        #region Direct Event Handling
        public void HandleMouseDown(object responder, MouseEventArgs args)
        {
            if (mouseCommand.HasValue)
            {
                if (args.ButtonPressed == MouseButton.Left) LaunchMouseCommand(args.Position);
            }
            else
            {
                if (args.ButtonPressed == MouseButton.Left) selectionStart = args.Position;
                else if (args.ButtonPressed == MouseButton.Right) LaunchDefaultCommand(args.Position);
            }

            mouseCommand = null;
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
                .ToList();

            // Filter out buildings
            bool containsNonBuildingUnits = selectedUnits.Any(unit => !unit.Type.IsBuilding);
            if (containsNonBuildingUnits) selectedUnits.RemoveAll(unit => unit.Type.IsBuilding);

            // Filter out factions
            bool containsUnitsFromThisFaction = selectedUnits.Any(unit => unit.Faction == faction);
            if (containsUnitsFromThisFaction)
                selectedUnits.RemoveAll(unit => unit.Faction != faction);
            else if (selectedUnits.Count > 1)
                selectedUnits.RemoveRange(1, selectedUnits.Count - 1);

            if (shiftKeyPressed) selectionManager.AppendToSelection(selectedUnits);
            else selectionManager.SelectUnits(selectedUnits);

            selectionStart = null;
            selectionEnd = null;
        }

        public void HandleMouseMove(object responder, MouseEventArgs args)
        {
            if (selectionStart.HasValue) selectionEnd = args.Position;
        }

        public void HandleKeyDown(object responder, KeyboardEventArgs args)
        {
            if (args.Key == Keys.ShiftKey) shiftKeyPressed = true;
            if (args.Key == Keys.Delete) LaunchSuicide();
            if (args.Key == Keys.C) LaunchCancel();
            if (args.Key == Keys.T)
                LaunchTrain(commander.Faction.World.UnitTypes.First
                    (unit => unit.HasSkill<Attack>()));
            if (keysMap.ContainsKey(args.Key)) mouseCommand = keysMap[args.Key];
        }

        public void HandleKeyUp(object responder, KeyboardEventArgs args)
        {
            if (args.Key == Keys.ShiftKey) shiftKeyPressed = false;
        }
        #endregion

        #region Launching dynamically chosen commands
        public void LaunchMouseCommand(Vector2 at)
        {
            Faction faction = commander.Faction;
            Rectangle hitRect = new Rectangle(at.X, at.Y, 1, 1);
            Unit target = faction.World.Entities
                .OfType<Unit>()
                .Where(u => Rectangle.Intersects(hitRect, u.BoundingRectangle))
                .FirstOrDefault();

            switch (mouseCommand)
            {
                case MouseDrivenCommand.Attack:
                    // if a unit can move *and* attack, then it's probably capable of ZoneAttack'ing.
                    if (target == null) LaunchZoneAttack(at);
                    else LaunchAttack(target);
                    break;

                case MouseDrivenCommand.Build:
                    if (target != null) return;
                    LaunchBuild(at, commander.Faction.World.UnitTypes.First(unit => unit.IsBuilding));
                    break;

                case MouseDrivenCommand.Harvest:
                    ResourceNode resource = faction.World.Entities
                        .OfType<ResourceNode>()
                        .FirstOrDefault(node => Rectangle.Intersects(hitRect, node.BoundingRectangle));
                    LaunchHarvest(resource);
                    break;

                case MouseDrivenCommand.Move: LaunchMove(at); break;

                case MouseDrivenCommand.Repair:
                    if (target == null || !target.Type.IsBuilding) break;
                    LaunchRepair(target);
                    break;

            }

            mouseCommand = null;
        }

        public void LaunchDefaultCommand(Vector2 at)
        {
            Faction faction = commander.Faction;
            Rectangle hitRect = new Rectangle(at.X, at.Y, 1, 1);
            Unit target = faction.World.Entities
                .OfType<Unit>()
                .Where(u => Rectangle.Intersects(hitRect, u.BoundingRectangle))
                .FirstOrDefault();

            if (target == null)
            {
                ResourceNode node = faction.World.Entities
                    .OfType<ResourceNode>()
                    .FirstOrDefault(n => Rectangle.Intersects(hitRect, n.BoundingRectangle));
                if (node != null) LaunchHarvest(node);
                else LaunchMove(at);
            }
            else
            {
                // TODO
                // implement friendlyness checks more elaborate than this
                if (target.Faction == commander.Faction) LaunchMove(target.Position);
                else LaunchAttack(target);
            }
        }
        #endregion

        #region Launching individual commands

        private void LaunchBuild(Vector2 destination, UnitType unitTypeToBuild)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                  .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.Move>());
            // Those who can attack do so, the others simply move to the destination
            Unit theBuilder = movableUnits.FirstOrDefault(unit =>
            {
                 Skills.Build build = unit.GetSkill<Skills.Build>();
                if (build == null) return false;
                return build.Supports(unitTypeToBuild);
            });

            if (theBuilder != null)
            {
                commander.LaunchBuild(theBuilder, unitTypeToBuild, destination);
                commander.LaunchMove(movableUnits.Where(unit => unit != theBuilder), destination);
            }
            

        }

        private void LaunchAttack(Unit target)
        {
            IEnumerable<Unit> selection = selectionManager.SelectedUnits.Where(unit => unit.Faction == commander.Faction);
            // Those who can attack do so, the others simply move to the target's position
            commander.LaunchAttack(selection.Where(unit => unit.HasSkill<Skills.Attack>()), target);
            commander.LaunchMove(selection.Where(unit => !unit.HasSkill<Skills.Attack>() && unit.HasSkill<Skills.Move>()), target.Position);
        }

        private void LaunchZoneAttack(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.Move>());
            // Those who can attack do so, the others simply move to the destination
            commander.LaunchZoneAttack(movableUnits.Where(unit => unit.HasSkill<Skills.Attack>()), destination);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<Skills.Attack>()), destination);
        }

        private void LaunchHarvest(ResourceNode node)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.Move>());
            // Those who can harvest do so, the others simply move to the resource's position
            commander.LaunchHarvest(movableUnits.Where(unit => unit.HasSkill<Skills.Harvest>()), node);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<Skills.Harvest>()), node.Position);
        }

        private void LaunchMove(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.Move>());
            commander.LaunchMove(movableUnits, destination);
        }

        private void LaunchRepair(Unit building)
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.Build>());
            commander.LaunchRepair(targetUnits, building);
        }

        private void LaunchTrain(UnitType unitType)
        {
            IEnumerable<Unit> trainers = selectionManager.SelectedUnits
                .Where(unit =>
                {
                    if (unit.Faction != commander.Faction) return false;
                    Skills.Train train = unit.Type.GetSkill<Skills.Train>();
                    if (train == null) return false;
                    return train.Supports(unitType);
                });
            commander.LaunchTrain(trainers, unitType);
        }

        private void LaunchSuicide()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction);
            commander.LaunchSuicide(targetUnits);
        }
        private void LaunchCancel()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction);
            commander.CancelCommands(targetUnits);
        }
        #endregion
        #endregion
    }
}
