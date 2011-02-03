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

        public void LaunchCancel(IEnumerable<Unit> units)
        {
            if (units.Count() > 0)
                IssueCommand(new CancelCommand(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchAttack(IEnumerable<Unit> units, Unit target)
        {
            units = units.Except(target);
            if (units.Count() > 0)
                IssueCommand(new AttackCommand(Faction.Handle, units.Select(unit => unit.Handle), target.Handle));
        }

        public void LaunchBuild(IEnumerable<Unit> units, Unit buildingType, Point point)
        {
            if (units.Count() > 0)
                IssueCommand(new BuildCommand(Faction.Handle, units.Select(unit => unit.Handle), buildingType.Handle, point));
        }

        public void LaunchHarvest(IEnumerable<Unit> units, Entity node)
        {
            if (units.Count() > 0)
                IssueCommand(new HarvestCommand(Faction.Handle, units.Select(unit => unit.Handle), node.Handle));
        }

        public void LaunchMove(IEnumerable<Unit> units, Vector2 destination)
        {
            destination = World.Clamp(destination);
            if (units.Count() > 0)
                IssueCommand(new MoveCommand(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }

        public void LaunchChangeRallyPoint(IEnumerable<Unit> units, Vector2 destination)
        {
            destination = World.Clamp(destination);
            if (units.Count() > 0)
                IssueCommand(new ChangeRallyPointCommand(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }

        public void LaunchChangeDiplomacy(Faction targetFaction, DiplomaticStance newStance)
        {
            IssueCommand(new ChangeDiplomaticStanceCommand(Faction.Handle, targetFaction.Handle, newStance));
        }

        public void LaunchRepair(IEnumerable<Unit> units, Unit repairedUnit)
        {
            units = units.Except(repairedUnit);
            if (units.Count() > 0)
                IssueCommand(new RepairCommand(Faction.Handle, units.Select(unit => unit.Handle), repairedUnit.Handle));
        }

        public void LaunchTrain(IEnumerable<Unit> buildings, Unit trainedType)
        {
            if (buildings.Count() > 0)
                IssueCommand(new TrainCommand(Faction.Handle, buildings.Select(unit => unit.Handle), trainedType.Handle));
        }

        public void LaunchResearch(Unit reseacher, Technology technology)
        {
            if (reseacher != null)
                IssueCommand(new ResearchCommand(reseacher.Handle, Faction.Handle, technology.Handle));
        }

        public void LaunchSuicide(IEnumerable<Unit> units)
        {
            if (units.Count() > 0)
                IssueCommand(new SuicideCommand(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchZoneAttack(IEnumerable<Unit> units, Vector2 destination)
        {
            destination = World.Clamp(destination);
            if (units.Count() > 0)
                IssueCommand(new ZoneAttackCommand(Faction.Handle, units.Select(unit => unit.Handle), destination));
        }

        public void LaunchHeal(IEnumerable<Unit> units, Unit hurtUnit)
        {
            units = units.Except(hurtUnit);
            if (units.Count() > 0)
                IssueCommand(new HealCommand(Faction.Handle, units.Select(unit => unit.Handle), hurtUnit.Handle));
        }

        public void LaunchStandGuard(IEnumerable<Unit> units)
        {
            if (units.Count() > 0)
                IssueCommand(new StandGuardCommand(Faction.Handle, units.Select(unit => unit.Handle)));
        }

        public void LaunchUpgrade(IEnumerable<Unit> units, Unit targetType)
        {
            if (units.Count() > 0)
                IssueCommand(new UpgradeCommand(Faction.Handle, units.Select(u => u.Handle), targetType.Handle));
        }

        public void LaunchEmbark(IEnumerable<Unit> embarkers, Unit transporter)
        {
            if (embarkers.Count() > 0)
                IssueCommand(new EmbarkCommand(Faction.Handle, embarkers.Select(u => u.Handle), transporter.Handle));
        }

        public void LaunchDisembark(IEnumerable<Unit> transporters)
        {
            if (transporters.Count() > 0)
                IssueCommand(new DisembarkCommand(Faction.Handle, transporters.Select(u => u.Handle)));
        }
        #endregion
    }
}
