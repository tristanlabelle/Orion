using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Geometry;
using Orion.GameLogic;
using Skills = Orion.GameLogic.Skills;

using OpenTK.Math;

namespace Orion.Commandment
{
    public enum MouseDrivenCommand
    {
        None, Attack, Build, Harvest, Move, ZoneAttack
    }

    public class UserInputManager
    {
        #region Fields
        // TODO
        // use unit skills for key mapping instead of a static keys map
        private static Dictionary<Keys, MouseDrivenCommand> keysMap;

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
            keysMap = new Dictionary<Keys,MouseDrivenCommand>();
            keysMap[Keys.A] = MouseDrivenCommand.Attack;
            keysMap[Keys.B] = MouseDrivenCommand.Build;
            keysMap[Keys.G] = MouseDrivenCommand.Harvest; // "G"ather
            keysMap[Keys.M] = MouseDrivenCommand.Move;
            keysMap[Keys.Escape] = MouseDrivenCommand.None;
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
        #region Event Handling
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
            if(!selectionStart.HasValue) return;
            if (args.ButtonPressed != MouseButton.Left) return;

            selectionEnd = args.Position;
            Faction faction = commander.Faction;
            Rectangle selection = SelectionRectangle.Value;
            List<Unit> selectedUnits = faction.World.Units.Where(unit => Intersection.Test(selection, unit.Circle)).ToList();

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
            if (keysMap.ContainsKey(args.Key)) mouseCommand = keysMap[args.Key];
        }

        public void HandleKeyUp(object responder, KeyboardEventArgs args)
        {
            if (args.Key == Keys.ShiftKey) shiftKeyPressed = false;
        }
        #endregion

        #region Launching commands from the UI
        public void LaunchMouseCommand(Vector2 at)
        {
            Faction faction = commander.Faction;
            Rectangle hitRect = new Rectangle(at.X, at.Y, 1, 1);
            Unit target = faction.World.Units.Where(u => Intersection.Test(hitRect, u.Circle)).FirstOrDefault();
            switch (mouseCommand)
            {
                case MouseDrivenCommand.Attack:
                    // if a unit can move *and* attack, then it's probably capable of ZoneAttack'ing.
                    if (target == null) LaunchZoneAttack(at);
                    else LaunchAttack(target);
                    break;

                case MouseDrivenCommand.Build:
                    if (target != null) return;
                    throw new NotImplementedException("Cannot create buildings at this stage of refactoring");

                case MouseDrivenCommand.Harvest:
                    ResourceNode resource = faction.World.ResourceNodes.Where(node => Intersection.Test(hitRect, node.Circle)).FirstOrDefault();
                    LaunchHarvest(resource);
                    break;

                case MouseDrivenCommand.Move: LaunchMove(at);  break;
            }

            mouseCommand = MouseDrivenCommand.None;
        }

        public void LaunchDefaultCommand(Vector2 at)
        {
            Faction faction = commander.Faction;
            Rectangle hitRect = new Rectangle(at.X, at.Y, 1, 1);
            Unit target = faction.World.Units.Where(u => Intersection.Test(hitRect, u.Circle)).FirstOrDefault();

            if (target == null)
            {
                ResourceNode node = faction.World.ResourceNodes.Where(n => Intersection.Test(hitRect, n.Circle)).FirstOrDefault();
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
        private void LaunchAttack(Unit target)
        {
            IEnumerable<Unit> selection = selectionManager.SelectedUnits;
            commander.LaunchAttack(selection.Where(unit => unit.HasSkill<Skills.Attack>()), target);
            commander.LaunchMove(selection.Where(unit => !unit.HasSkill<Skills.Attack>() && unit.HasSkill<Skills.Move>()), target.Position);
        }

        private void LaunchZoneAttack(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits.Where(unit => unit.HasSkill<Skills.Move>());
            commander.LaunchZoneAttack(movableUnits.Where(unit => unit.HasSkill<Skills.Attack>()), destination);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<Skills.Attack>()), destination);
        }

        private void LaunchHarvest(ResourceNode node)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits.Where(unit => unit.HasSkill<Skills.Move>());
            commander.LaunchHarvest(movableUnits.Where(unit => unit.HasSkill<Skills.Harvest>()), node);
            commander.LaunchMove(movableUnits.Where(unit => !unit.HasSkill<Skills.Harvest>()), node.Position);
        }

        private void LaunchMove(Vector2 destination)
        {
            IEnumerable<Unit> movableUnits = selectionManager.SelectedUnits.Where(unit => unit.HasSkill<Skills.Move>());
            commander.LaunchMove(movableUnits, destination);
        }
        #endregion
        #endregion
    }
}
