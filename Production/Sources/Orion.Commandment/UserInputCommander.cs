using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment.Commands;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Commandment
{
    public sealed class UserInputCommander : Commander
    {
        #region Constructors
        /// <summary>
        /// Constructor For a commander that can listen input to create commands
        /// </summary>
        /// <param name="faction">the faction of the player.</param>
        public UserInputCommander(Faction faction)
            : base(faction)
        { }
        #endregion

        #region Methods
        public void SendMessage(string message)
        {
            GenerateCommand(new SendMessage(Faction.Handle, message));
        }

        public void CancelCommands(IEnumerable<Unit> units)
        {
            if (units.Count() > 0)
                GenerateCommand(new Cancel(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchAttack(IEnumerable<Unit> units, Unit target)
        {
            if (units.Count() > 0)
                GenerateCommand(new Attack(Faction.Handle, units.Select(unit => unit.Handle), target.Handle));
        }

        public void LaunchBuild(Unit builder, UnitType buildingType, Vector2 buildingPosition)
        {
            GenerateCommand(new Build(Faction.Handle, builder.Handle, buildingType.Handle, buildingPosition));
        }

        public void LaunchHarvest(IEnumerable<Unit> units, ResourceNode node)
        {
            if (units.Count() > 0)
                GenerateCommand(new Harvest(Faction.Handle, units.Select(unit => unit.Handle), node.Handle));
        }

        public void LaunchMove(IEnumerable<Unit> units, Vector2 destination)
        {
            // Clamp the destination within the world bounds.
            // The world bounds maximums are be exclusive.
            destination = World.Bounds.ClosestPointInside(destination);
            if (destination.X == World.Size.Width) destination.X -= 0.0001f;
            if (destination.Y == World.Size.Height) destination.Y -= 0.0001f;

            if (units.Count() > 0)
                GenerateCommand(new Move(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }

        public void LaunchRepair(IEnumerable<Unit> units, Unit repairedUnit)
        {
            if (units.Count() > 0)
                GenerateCommand(new Repair(Faction.Handle, units.Select(unit => unit.Handle), repairedUnit.Handle));
        }

        public void LaunchTrain(IEnumerable<Unit> buildings, UnitType trainedType)
        {
            if (buildings.Count() > 0)
                GenerateCommand(new Train(Faction.Handle, buildings.Select(unit => unit.Handle), trainedType.Handle));
        }

        public void LaunchSuicide(IEnumerable<Unit> units)
        {
            if (units.Count() > 0)
                GenerateCommand(new Suicide(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchChangeDimplomacy(Faction otherFaction)
        {
            if (otherFaction == null) return;
            if (Faction.GetDiplomaticStance(otherFaction) == DiplomaticStance.Ally)
                GenerateCommand(new ChangeDiplomaticStance(Faction.Handle, otherFaction.Handle, DiplomaticStance.Enemy));
            else
                GenerateCommand(new ChangeDiplomaticStance(Faction.Handle, otherFaction.Handle, DiplomaticStance.Ally));
        }

        public void LaunchZoneAttack(IEnumerable<Unit> units, Vector2 destination)
        {
            if (units.Count() > 0)
                GenerateCommand(new ZoneAttack(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }
        #endregion
    }
}
