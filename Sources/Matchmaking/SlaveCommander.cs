using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Collections;
using Orion.Matchmaking.Commands;
using Orion.Matchmaking.Commands.Pipeline;
using Orion.GameLogic;
using Orion.GameLogic.Technologies;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Matchmaking
{
    /// <summary>
    /// A behaviorless commander which offers a public interface to create commands.
    /// </summary>
    public class SlaveCommander : Commander
    {
        #region Constructors
        /// <summary>
        /// Constructor For a commander that can listen input to create commands
        /// </summary>
        /// <param name="faction">the faction of the player.</param>
        public SlaveCommander(Faction faction)
            : base(faction)
        { }
        #endregion

        #region Methods
        public void SendMessage(string message)
        {
            Argument.EnsureNotNull(message, "message");
            GenerateCommand(new SendMessageCommand(Faction.Handle, message));
        }

        public void LaunchCancel(IEnumerable<Unit> units)
        {
            if (units.Count() > 0)
                GenerateActionCommand(new CancelCommand(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchAttack(IEnumerable<Unit> units, Unit target)
        {
            units = units.Except(target);
            if (units.Count() > 0)
                GenerateActionCommand(new AttackCommand(Faction.Handle, units.Select(unit => unit.Handle), target.Handle));
        }

        public void LaunchBuild(IEnumerable<Unit> units, UnitType buildingType, Point point)
        {
            if (units.Count() > 0)
                GenerateActionCommand(new BuildCommand(Faction.Handle, units.Select(unit => unit.Handle), buildingType.Handle, point));
        }

        public void LaunchHarvest(IEnumerable<Unit> units, ResourceNode node)
        {
            if (units.Count() > 0)
                GenerateActionCommand(new HarvestCommand(Faction.Handle, units.Select(unit => unit.Handle), node.Handle));
        }

        public void LaunchMove(IEnumerable<Unit> units, Vector2 destination)
        {
            destination = ClampPosition(destination);
            if (units.Count() > 0)
                GenerateActionCommand(new MoveCommand(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }

        private Vector2 ClampPosition(Vector2 destination)
        {
            // Clamp the destination within the world bounds.
            // The world bounds maximums are be exclusive.
            destination = World.Bounds.Clamp(destination);
            if (destination.X == World.Size.Width) destination.X -= 0.0001f;
            if (destination.Y == World.Size.Height) destination.Y -= 0.0001f;
            return destination;
        }

        public void LaunchChangeRallyPoint(IEnumerable<Unit> units, Vector2 destination)
        {
            destination = ClampPosition(destination);
            if (units.Count() > 0)
                GenerateActionCommand(new ChangeRallyPointCommand(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }

        public void LaunchRepair(IEnumerable<Unit> units, Unit repairedUnit)
        {
            units = units.Except(repairedUnit);
            if (units.Count() > 0)
                GenerateActionCommand(new RepairCommand(Faction.Handle, units.Select(unit => unit.Handle), repairedUnit.Handle));
        }

        public void LaunchTrain(IEnumerable<Unit> buildings, UnitType trainedType)
        {
            if (buildings.Count() > 0)
                GenerateActionCommand(new TrainCommand(Faction.Handle, buildings.Select(unit => unit.Handle), trainedType.Handle));
        }

        public void LaunchResearch(Unit reseacher, Technology technology)
        {
            if (reseacher != null)
                GenerateActionCommand(new ResearchCommand(reseacher.Handle, Faction.Handle, technology.Handle));
        }

        public void LaunchEmbark(IEnumerable<Unit> units, Unit target)
        {
            units = units.Except(target);
            if (units.Count() > 0)
                GenerateActionCommand(new EmbarkCommand(Faction.Handle, units.Select(unit => unit.Handle), target.Handle));
        }

        public void LaunchDisembark(IEnumerable<Unit> units)
        {
            if (units.Count() > 0)
                GenerateActionCommand(new DisembarkCommand(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchSuicide(IEnumerable<Unit> units)
        {
            if (units.Count() > 0)
                GenerateActionCommand(new SuicideCommand(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchZoneAttack(IEnumerable<Unit> units, Vector2 destination)
        {
            destination = ClampPosition(destination);
            if (units.Count() > 0)
                GenerateActionCommand(new ZoneAttackCommand(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }

        public void LaunchHeal(IEnumerable<Unit> units, Unit hurtUnit)
        {
            units = units.Except(hurtUnit);
            if (units.Count() > 0)
                GenerateActionCommand(new HealCommand(Faction.Handle, units.Select(unit => unit.Handle), hurtUnit.Handle));
        }

        public void LaunchStandGuard(IEnumerable<Unit> units)
        {
            if (units.Count() > 0)
                GenerateActionCommand(new StandGuardCommand(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void GenerateActionCommand(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            if (Faction.Status == FactionStatus.Defeated) return;
            GenerateCommand(command);
        }

        public void LaunchChangeDiplomacy(Faction otherFaction)
        {
            if (otherFaction == null) return;
            if (Faction.GetDiplomaticStance(otherFaction) == DiplomaticStance.Ally)
                GenerateActionCommand(new ChangeDiplomaticStanceCommand(Faction.Handle, otherFaction.Handle, DiplomaticStance.Enemy));
            else
                GenerateActionCommand(new ChangeDiplomaticStanceCommand(Faction.Handle, otherFaction.Handle, DiplomaticStance.Ally));
        }

        public override void Update(float timeDelta) { }
        #endregion
    }
}