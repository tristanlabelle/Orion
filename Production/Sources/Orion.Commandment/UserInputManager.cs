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
        private UserInputCommander commander;
        private SelectionManager selectionManager;
        private UserInputCommand mouseCommand;
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
        #endregion

        #region Events
        public event GenericEventHandler<UserInputManager> AssignedCommand;
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
            switch (args.Key)
            {
                case Keys.Escape: mouseCommand = null; break;
                case Keys.ShiftKey: shiftKeyPressed = true; break;
                case Keys.Delete: LaunchSuicide(); break;

                // cheats
                case Keys.F10:
                    commander.Faction.AladdiumAmount = 10000;
                    commander.Faction.AlageneAmount = 10000;
                    break;
                case Keys.F11: commander.Faction.FogOfWar.Reveal(); break;
                case Keys.F12: commander.Faction.FogOfWar.Disable(); break;

                // should be refactored out
                case Keys.T:
                    LaunchTrain(commander.Faction.World.UnitTypes
                        .First(unit => unit.HasSkill<Attack>()));
                    break;
            }
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
            Entity target = faction.World.Entities
                .Where(u => Rectangle.Intersects(hitRect, u.BoundingRectangle)).FirstOrDefault();

            if (target == null) mouseCommand.Execute(at);
            else mouseCommand.Execute(target);

            mouseCommand = null;
            GenericEventHandler<UserInputManager> handler = AssignedCommand;
            if (handler != null) handler(this);
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

            GenericEventHandler<UserInputManager> handler = AssignedCommand;
            if (handler != null) handler(this);
        }
        #endregion

        #region Launching individual commands

        public void Cancel()
        {
            commander.CancelCommands(selectionManager.SelectedUnits);
        }

        public void LaunchBuild(Vector2 destination, UnitType unitTypeToBuild)
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

        public void LaunchAttack(Unit target)
        {
            IEnumerable<Unit> selection = selectionManager.SelectedUnits.Where(unit => unit.Faction == commander.Faction);
            // Those who can attack do so, the others simply move to the target's position
            commander.LaunchAttack(selection.Where(unit => unit.HasSkill<Skills.Attack>()), target);
            commander.LaunchMove(selection.Where(unit => !unit.HasSkill<Skills.Attack>() && unit.HasSkill<Skills.Move>()), target.Position);
        }

        public void LaunchZoneAttack(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.Move>());
            // Those who can attack do so, the others simply move to the destination
            commander.LaunchZoneAttack(movableUnits.Where(unit => unit.HasSkill<Skills.Attack>()), destination);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<Skills.Attack>()), destination);
        }

        public void LaunchHarvest(ResourceNode node)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.Move>());
            // Those who can harvest do so, the others simply move to the resource's position
            commander.LaunchHarvest(movableUnits.Where(unit => unit.HasSkill<Skills.Harvest>()), node);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<Skills.Harvest>()), node.Position);
        }

        public void LaunchMove(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.Move>());
            commander.LaunchMove(movableUnits, destination);
        }

        public void LaunchRepair(Unit building)
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction && unit.HasSkill<Skills.Build>());
            commander.LaunchRepair(targetUnits, building);
        }

        public void LaunchTrain(UnitType unitType)
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

        public void LaunchSuicide()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction);
            commander.LaunchSuicide(targetUnits);
        }

        public void LaunchCancel()
        {
            IEnumerable<Unit> targetUnits = selectionManager.SelectedUnits
                .Where(unit => unit.Faction == commander.Faction);
            commander.CancelCommands(targetUnits);
        }
        #endregion
        #endregion
    }
}
