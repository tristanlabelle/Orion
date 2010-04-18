using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Engine.Collections;
using Orion.Engine;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Matchmaking.Commands;
using Orion.Game.Simulation.Skills;
using OpenTK.Math;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// An AI commander which is pretty much helpless.
    /// </summary>
    public sealed class HarvestingAICommander : Commander
    {
        #region Fields
        private const float updatePeriod = 1;

        private readonly UnitType workerUnitType;
        private readonly UnitType foodSupplyUnitType;
        private float lastUpdateTime;
        #endregion

        #region Constructors
        public HarvestingAICommander(Match match, Faction faction)
            : base(match, faction)
        {
            workerUnitType = match.UnitTypes.FromName("Schtroumpf");
            foodSupplyUnitType = match.UnitTypes.FromName("Réserve");
        }
        #endregion

        #region Properties
        private Random Random
        {
            get { return Match.Random; }
        }

        private IEnumerable<ResourceNode> VisibleResourceNodes
        {
            get
            {
                foreach (Entity entity in World.Entities)
                {
                    ResourceNode node = entity as ResourceNode;
                    if (node == null || !Faction.CanSee(node)) continue;
                    yield return node;
                }
            }
        }
        #endregion

        #region Methods
        public override void Update(SimulationStep step)
        {
            if (step.TimeInSeconds - lastUpdateTime < updatePeriod)
                return;

            lastUpdateTime = step.TimeInSeconds;

            bool hasTrainedInFrame = false;
            foreach (Unit unit in Faction.Units.Where(unit => unit.IsIdle))
            {
                if (unit.Type.CanBuild(foodSupplyUnitType)
                    && Faction.UsedFoodAmount / (float)Faction.MaxFoodAmount > 0.8f
                    && Faction.MaxFoodAmount != World.MaximumFoodAmount)
                {
                    if (Faction.AladdiumAmount >= Faction.GetStat(foodSupplyUnitType, BasicSkill.AladdiumCostStat)
                        && Faction.AlageneAmount >= Faction.GetStat(foodSupplyUnitType, BasicSkill.AlageneCostStat))
                    {
                        Point buildingLocation = new Point(
                            (int)unit.Center.X + Random.Next(-10, 11),
                            (int)unit.Center.X + Random.Next(-10, 11));
                        buildingLocation = new Region(
                            World.Width - foodSupplyUnitType.Width,
                            World.Height - foodSupplyUnitType.Height)
                            .Clamp(buildingLocation);

                        Region buildingRegion = new Region(buildingLocation, foodSupplyUnitType.Size);
                        if (Faction.CanSee(buildingRegion) && World.IsFree(buildingRegion, CollisionLayer.Ground))
                        {
                            var command = new BuildCommand(Faction.Handle, unit.Handle, foodSupplyUnitType.Handle, buildingLocation);
                            IssueCommand(command);
                        }
                    }

                    continue;
                }

                if (unit.Type.CanTrain(workerUnitType) && !hasTrainedInFrame)
                {
                    int workerToCreateCount = GetMaximumTraineeCount(workerUnitType);
                    if (workerToCreateCount > 0)
                    {
                        var command = new TrainCommand(Faction.Handle, unit.Handle, workerUnitType.Handle, workerToCreateCount);
                        IssueCommand(command);
                        continue;
                    }
                }

                if (unit.HasSkill<HarvestSkill>())
                {
                    var resourceNode = VisibleResourceNodes
                        .Where(node => node.Type == ResourceType.Aladdium)
                        .Select(node => new
                        {
                            Node = node,
                            HarvesterCount = Faction.Units
                                .Select(u => u.TaskQueue.FirstOrDefault() as HarvestTask)
                                .Count(t => t != null && t.ResourceNode == node)
                        })
                        .Where(entry => entry.HarvesterCount < 5)
                        .WithMinOrDefault(entry => Region.Distance(unit.GridRegion, entry.Node.GridRegion) + entry.HarvesterCount * 2);
                    if (resourceNode != null)
                    {
                        var command = new HarvestCommand(Faction.Handle,
                            unit.Handle, resourceNode.Node.Handle);
                        IssueCommand(command);
                        continue;
                    }
                }

                if (unit.HasSkill<MoveSkill>())
                {
                    Vector2 destination = new Vector2(
                        unit.Center.X + (float)(Match.Random.NextDouble() * 10 - 5),
                        unit.Center.Y + (float)(Match.Random.NextDouble() * 10 - 5));
                    destination = World.Clamp(destination);
                    var command = new MoveCommand(Faction.Handle, unit.Handle, destination);
                    IssueCommand(command);
                    continue;
                }
            }
        }

        private int GetMaximumTraineeCount(UnitType traineeType)
        {
            int aladdiumCost = Faction.GetStat(traineeType, BasicSkill.AladdiumCostStat);
            int alageneCost = Faction.GetStat(traineeType, BasicSkill.AlageneCostStat);
            int foodCost = Faction.GetStat(traineeType, BasicSkill.FoodCostStat);

            int maximum = int.MaxValue;
            if (aladdiumCost > 0) maximum = Math.Min(maximum, Faction.AladdiumAmount / aladdiumCost);
            if (alageneCost > 0) maximum = Math.Min(maximum, Faction.AlageneAmount / alageneCost);
            if (foodCost > 0) maximum = Math.Min(maximum, Faction.RemainingFoodAmount / foodCost);

            return maximum;
        }
        #endregion
    }
}
