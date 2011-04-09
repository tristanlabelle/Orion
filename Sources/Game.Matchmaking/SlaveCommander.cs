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
using Orion.Game.Simulation.Components;

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

        public void LaunchCancelAllTasks(IEnumerable<Entity> entities)
        {
            if (entities.Any())
                IssueCommand(new CancelAllTasksCommand(Faction.Handle, entities.Select(entity => entity.Handle)));
        }

        public void LaunchCancelTask(Task task)
        {
            Entity entity = task.Entity;
            Handle taskHandle = entity.Components.Get<TaskQueue>().TryGetTaskHandle(task);
            IssueCommand(new CancelTaskCommand(Faction.Handle, entity.Handle, taskHandle));
        }

        public void LaunchAttack(IEnumerable<Entity> entities, Entity target)
        {
            entities = entities.Except(target);
            if (entities.Any())
                IssueCommand(new AttackCommand(Faction.Handle, entities.Select(entity => entity.Handle), target.Handle));
        }

        public void LaunchBuild(IEnumerable<Entity> entities, Entity buildingPrototype, Point point)
        {
            if (entities.Any())
                IssueCommand(new BuildCommand(Faction.Handle, entities.Select(entity => entity.Handle), buildingPrototype.Handle, point));
        }

        public void LaunchHarvest(IEnumerable<Entity> entities, Entity node)
        {
            if (entities.Any())
                IssueCommand(new HarvestCommand(Faction.Handle, entities.Select(entity => entity.Handle), node.Handle));
        }

        public void LaunchMove(IEnumerable<Entity> entities, Vector2 destination)
        {
            destination = World.Clamp(destination);
            if (entities.Any())
                IssueCommand(new MoveCommand(Faction.Handle, entities.Select(entity => entity.Handle), destination));
        }

        public void LaunchChangeRallyPoint(IEnumerable<Entity> entities, Vector2 destination)
        {
            destination = World.Clamp(destination);
            if (entities.Any())
                IssueCommand(new ChangeRallyPointCommand(Faction.Handle, entities.Select(entity => entity.Handle), destination));
        }

        public void LaunchChangeDiplomacy(Faction targetFaction, DiplomaticStance newStance)
        {
            IssueCommand(new ChangeDiplomaticStanceCommand(Faction.Handle, targetFaction.Handle, newStance));
        }

        public void LaunchRepair(IEnumerable<Entity> entities, Entity target)
        {
            entities = entities.Except(target);
            if (entities.Any())
                IssueCommand(new RepairCommand(Faction.Handle, entities.Select(entity => entity.Handle), target.Handle));
        }

        public void LaunchTrain(IEnumerable<Entity> buildings, Entity trainedPrototype)
        {
            if (buildings.Any())
                IssueCommand(new TrainCommand(Faction.Handle, buildings.Select(entity => entity.Handle), trainedPrototype.Handle));
        }

        public void LaunchResearch(Entity reseacher, Technology technology)
        {
            if (reseacher != null)
                IssueCommand(new ResearchCommand(reseacher.Handle, Faction.Handle, technology.Handle));
        }

        public void LaunchSuicide(IEnumerable<Entity> entities)
        {
            if (entities.Any())
                IssueCommand(new SuicideCommand(Faction.Handle, entities.Select(entity => entity.Handle)));
        }

        public void LaunchZoneAttack(IEnumerable<Entity> entities, Vector2 destination)
        {
            destination = World.Clamp(destination);
            if (entities.Any())
                IssueCommand(new ZoneAttackCommand(Faction.Handle, entities.Select(entity => entity.Handle), destination));
        }

        public void LaunchHeal(IEnumerable<Entity> entities, Entity target)
        {
            entities = entities.Except(target);
            if (entities.Any())
                IssueCommand(new HealCommand(Faction.Handle, entities.Select(entity => entity.Handle), target.Handle));
        }

        public void LaunchStandGuard(IEnumerable<Entity> entities)
        {
            if (entities.Any())
                IssueCommand(new StandGuardCommand(Faction.Handle, entities.Select(entity => entity.Handle)));
        }

        public void LaunchLoad(Entity transporter, Entity target)
        {
            if (!target.Components.Has<Transporter>())
                IssueCommand(new LoadCommand(Faction.Handle, transporter.Handle, target.Handle));
        }

        public void LaunchUpgrade(IEnumerable<Entity> entities, Entity targetPrototype)
        {
            if (entities.Any())
                IssueCommand(new UpgradeCommand(Faction.Handle, entities.Select(u => u.Handle), targetPrototype.Handle));
        }
        #endregion
    }
}
