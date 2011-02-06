using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Matchmaking.Commands;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Technologies;
using System;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// A behaviorless commander which offers a public interface to create commands.
    /// </summary>
    public class SlaveCommander : Commander
    {
        #region Constructors
        public SlaveCommander(Match match, Faction faction)
            : base(match, faction)
        { }
        #endregion

        #region Methods
        public void SendMessage(string message)
        {
            Argument.EnsureNotNull(message, "message");

            IssueCommand(new SendMessageCommand(Faction.Handle, message));
        }

        public void SendAllyMessage(string message)
        {
            Argument.EnsureNotNull(message, "message");

            var allyFactionHandles = Faction.World.Factions
                .Where(f => Faction.GetDiplomaticStance(f) != DiplomaticStance.Enemy)
                .Select(f => f.Handle);
            IssueCommand(new SendMessageCommand(Faction.Handle, allyFactionHandles, message));
        }

        public void LaunchCancelAllTasks(IEnumerable<Unit> units)
        {
            if (units.Any())
                IssueCommand(new CancelAllTasksCommand(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchCancelTask(Task task)
        {
            Unit unit = task.Unit;
            Handle taskHandle = unit.TaskQueue.TryGetTaskHandle(task);
            IssueCommand(new CancelTaskCommand(unit.Faction.Handle, unit.Handle, taskHandle));
        }

        public void LaunchAttack(IEnumerable<Unit> units, Unit target)
        {
            units = units.Except(target);
            if (units.Any())
                IssueCommand(new AttackCommand(Faction.Handle, units.Select(unit => unit.Handle), target.Handle));
        }

        public void LaunchBuild(IEnumerable<Unit> units, UnitType buildingType, Point point)
        {
            if (units.Any())
                IssueCommand(new BuildCommand(Faction.Handle, units.Select(unit => unit.Handle), buildingType.Handle, point));
        }

        public void LaunchHarvest(IEnumerable<Unit> units, ResourceNode node)
        {
            if (units.Any())
                IssueCommand(new HarvestCommand(Faction.Handle, units.Select(unit => unit.Handle), node.Handle));
        }

        public void LaunchMove(IEnumerable<Unit> units, Vector2 destination)
        {
            destination = World.Clamp(destination);
            if (units.Any())
                IssueCommand(new MoveCommand(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }

        public void LaunchChangeRallyPoint(IEnumerable<Unit> units, Vector2 destination)
        {
            destination = World.Clamp(destination);
            if (units.Any())
                IssueCommand(new ChangeRallyPointCommand(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }

        public void LaunchChangeDiplomacy(Faction targetFaction, DiplomaticStance newStance)
        {
            IssueCommand(new ChangeDiplomaticStanceCommand(Faction.Handle, targetFaction.Handle, newStance));
        }

        public void LaunchRepair(IEnumerable<Unit> units, Unit repairedUnit)
        {
            units = units.Except(repairedUnit);
            if (units.Any())
                IssueCommand(new RepairCommand(Faction.Handle, units.Select(unit => unit.Handle), repairedUnit.Handle));
        }

        public void LaunchTrain(IEnumerable<Unit> buildings, UnitType trainedType)
        {
            if (buildings.Any())
                IssueCommand(new TrainCommand(Faction.Handle, buildings.Select(unit => unit.Handle), trainedType.Handle));
        }

        public void LaunchResearch(Unit reseacher, Technology technology)
        {
            if (reseacher != null)
                IssueCommand(new ResearchCommand(reseacher.Handle, Faction.Handle, technology.Handle));
        }

        public void LaunchSuicide(IEnumerable<Unit> units)
        {
            if (units.Any())
                IssueCommand(new SuicideCommand(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchZoneAttack(IEnumerable<Unit> units, Vector2 destination)
        {
            destination = World.Clamp(destination);
            if (units.Any())
                IssueCommand(new ZoneAttackCommand(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }

        public void LaunchHeal(IEnumerable<Unit> units, Unit hurtUnit)
        {
            units = units.Except(hurtUnit);
            if (units.Any())
                IssueCommand(new HealCommand(Faction.Handle, units.Select(unit => unit.Handle), hurtUnit.Handle));
        }

        public void LaunchStandGuard(IEnumerable<Unit> units)
        {
            if (units.Any())
                IssueCommand(new StandGuardCommand(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchUpgrade(IEnumerable<Unit> units, UnitType targetType)
        {
            if (units.Any())
                IssueCommand(new UpgradeCommand(Faction.Handle, units.Select(u => u.Handle), targetType.Handle));
        }

        public void LaunchEmbark(IEnumerable<Unit> embarkers, Unit transporter)
        {
            if (embarkers.Any())
                IssueCommand(new EmbarkCommand(Faction.Handle, embarkers.Select(u => u.Handle), transporter.Handle));
        }

        public void LaunchDisembark(IEnumerable<Unit> transporters)
        {
            if (transporters.Any())
                IssueCommand(new DisembarkCommand(Faction.Handle, transporters.Select(u => u.Handle)));
        }
        #endregion
    }
}
