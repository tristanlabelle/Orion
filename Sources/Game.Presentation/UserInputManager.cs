using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Engine.Input;
using Orion.Game.Matchmaking;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Technologies;
using Orion.Game.Simulation.Utilities;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Handles the keyboard and mouse input on the world.
    /// </summary>
    public sealed class UserInputManager
    {
        #region Fields
        private const int selectionGroupCount = 10;
        private const float singleClickMaxRectangleArea = 0.1f;

        private readonly Match match;
        private readonly SlaveCommander commander;
        private readonly UnderAttackMonitor underAttackMonitor;
        private readonly SelectionGroupManager selectionGroupManager;
        private readonly SelectionManager selectionManager;
        private Entity hoveredEntity;
        private UserInputCommand mouseCommand;
        private Vector2? selectionStart;
        private Vector2? selectionEnd;
        private bool shiftKeyPressed;
        private bool launchedCommandsWithShift;
        #endregion

        #region Constructors
        public UserInputManager(Match match, SlaveCommander commander)
        {
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(commander, "commander");

            this.match = match;
            this.commander = commander;
            this.underAttackMonitor = new UnderAttackMonitor(commander.Faction);
            this.selectionGroupManager = new SelectionGroupManager(match.World, 10);
            this.selectionManager = new SelectionManager(commander.Faction);
            this.commander.Faction.World.EntityRemoved += OnEntityRemoved;
        }
        #endregion

        #region Properties
        public Match Match
        {
            get { return match; }
        }

        public Commander LocalCommander
        {
            get { return commander; }
        }

        public Faction LocalFaction
        {
            get { return commander.Faction; }
        }

        public World World
        {
            get { return match.World; }
        }

        public UnderAttackMonitor UnderAttackMonitor
        {
            get { return underAttackMonitor; }
        }

        public SelectionManager SelectionManager
        {
            get { return selectionManager; }
        }

        public Selection Selection
        {
            get { return selectionManager.Selection; }
        }

        public Entity HoveredEntity
        {
            get { return hoveredEntity; }
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
        public void HandleMouseDown(MouseEventArgs args)
        {
            if (mouseCommand != null)
            {
                if (args.Button == MouseButton.Left)
                    LaunchMouseCommand(args.Position);

                if (!shiftKeyPressed)
                    mouseCommand = null;
                else
                    launchedCommandsWithShift = true;

                return;
            }

            if (args.Button == MouseButton.Left)
            {
                if (args.ClickCount > 1)
                    HandleMouseMultiClick(args);
                else
                    selectionStart = args.Position;
            }
            else if (args.Button == MouseButton.Right)
            {
                LaunchDefaultCommand(args.Position);
            }
        }

        public void HandleMouseUp(MouseEventArgs args)
        {
            if (!this.selectionStart.HasValue) return;
            if (args.Button != MouseButton.Left) return;

            this.selectionEnd = args.Position;
            Vector2 selectionRectangleStart = this.selectionStart.Value;
            Vector2 selectionRectangleEnd = this.selectionEnd.Value;
            Rectangle selectionRectangle = this.SelectionRectangle.Value;
            this.selectionStart = null;
            this.selectionEnd = null;

            if (selectionRectangle.Area < singleClickMaxRectangleArea)
            {
                HandleMouseClick(selectionRectangleStart);
            }
            else
            {
                if (shiftKeyPressed)
                    Selection.AddFromRectangle(selectionRectangleStart, selectionRectangleEnd);
                else
                    Selection.SetFromRectangle(selectionRectangleStart, selectionRectangleEnd);
            }
        }

        private void HandleMouseClick(Vector2 position)
        {
            Point point = (Point)position;
            if (!World.IsWithinBounds(point)) return;

            Spatial clickedEntitySpatial = World.SpatialManager
                .Intersecting(position)
                .WithMaxOrDefault(spatial => spatial.CollisionLayer);

            Entity clickedEntity = clickedEntitySpatial == null ? null : clickedEntitySpatial.Entity;
            if (clickedEntity == null || !clickedEntity.Identity.IsSelectable)
            {
                if (!shiftKeyPressed) Selection.Clear();
                return;
            }

            if (shiftKeyPressed)
                Selection.Toggle(clickedEntity);
            else
                Selection.Set(clickedEntity);
        }

        private void HandleMouseMultiClick(MouseEventArgs args)
        {
            if (args.Button != MouseButton.Left) return;

            selectionStart = null;
            selectionEnd = null;

            Point point = (Point)args.Position;
            if (!World.IsWithinBounds(point) || LocalFaction.GetTileVisibility(point) != TileVisibility.Visible)
            {
                Selection.Clear();
                return;
            }

            Spatial clickedEntitySpatial = World.SpatialManager.GetTopmostAt(point);
            if (clickedEntitySpatial == null)
            {
                Selection.Clear();
                return;
            }

            Entity clickedEntity = clickedEntitySpatial.Entity;
            if (FactionMembership.GetFaction(clickedEntity) == LocalFaction)
                Selection.SetToNearbySimilar(clickedEntity);
            else
                Selection.Set(clickedEntity);
        }

        public void HandleMouseMove(MouseEventArgs args)
        {
            if (mouseCommand != null)
            {
                mouseCommand.OnMouseMoved(args.Position);
                return;
            }

            if (selectionStart.HasValue) selectionEnd = args.Position;
            else
            {
                Point point = (Point)args.Position;

                Spatial hoveredSpatial = World.SpatialManager.GetTopmostAt(args.Position);
                hoveredEntity = hoveredSpatial == null ? null : hoveredSpatial.Entity;
            }
        }

        public void HandleKeyDown(KeyboardEventArgs args)
        {
            switch (args.Key)
            {
                case Keys.Escape: mouseCommand = null; break;
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    shiftKeyPressed = true;
                    break;
                case Keys.Delete: LaunchSuicide(); break;
            }

            if (args.Key >= Keys.D0 && args.Key <= Keys.D9)
            {
                int groupNumber = args.Key - Keys.D0;
                if (args.IsControlModifierDown)
                    selectionGroupManager.Set(groupNumber, Selection);
                else
                    Selection.Set(selectionGroupManager[groupNumber]);
            }
        }

        public void HandleKeyUp(KeyboardEventArgs args)
        {
            if (args.Key == Keys.ShiftKey || args.Key == Keys.LShiftKey || args.Key == Keys.RShiftKey)
            {
                shiftKeyPressed = false;
                if (launchedCommandsWithShift)
                {
                    launchedCommandsWithShift = false;
                    mouseCommand = null;
                }
            }
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            if (entity == hoveredEntity) hoveredEntity = null;
        }
        #endregion

        #region Launching dynamically chosen commands
        public void LaunchMouseCommand(Vector2 location)
        {
            mouseCommand.OnClick(location);
            if (!shiftKeyPressed)
                mouseCommand = null;
            else
                launchedCommandsWithShift = true;
        }

        public void LaunchDefaultCommand(Vector2 target)
        {
            if (!Selection.Any(entity => IsControllable(entity))) return;

            Point point = (Point)target;
            if (!World.IsWithinBounds(point))
            {
                LaunchMove(target);
                return;
            }

            bool anyCanMove = Selection.Any(entity => entity.Components.Has<Mobile>());
            Spatial targetEntitySpatial = World.SpatialManager.GetTopmostAt(target);
            if (LocalFaction.GetTileVisibility(point) == TileVisibility.Undiscovered
                || targetEntitySpatial == null)
            {
                if (anyCanMove)
                    LaunchMove(target);
                else
                    LaunchChangeRallyPoint(target);
            }
            else
            {
                LaunchDefaultCommand(targetEntitySpatial.Entity);
            }
        }

        private void LaunchDefaultCommand(Entity target)
        {
            Spatial targetSpatial = target.Spatial;
            if (targetSpatial == null) return;

            if (target.Components.Has<Harvestable>())
            {
                if (LocalFaction.CanHarvest(target) && Selection.Any(entity => entity.Components.Has<Harvester>()))
                    LaunchHarvest(target);
                else if (Selection.Any(entity => entity.Components.Has<Mobile>()))
                    LaunchMove(targetSpatial.Position);
                else
                    LaunchChangeRallyPoint(targetSpatial.Center);

                return;
            }

            Faction targetFaction = FactionMembership.GetFaction(target);
            if (targetFaction == null || LocalFaction.GetDiplomaticStance(targetFaction) == DiplomaticStance.Enemy)
            {
                if (target.Components.Has<Health>()) LaunchAttack(target);
                else LaunchMove(targetSpatial.Center);
                return;
            }

            if (Selection.All(entity => entity.Components.Has<Trainer>()))
            {
                LaunchChangeRallyPoint(targetSpatial.Center);
                return;
            }

            if (target.Components.Has<AlageneExtractor>())
            {
                foreach (Spatial spatial in World.SpatialManager.Intersecting(Rectangle.FromCenterSize(targetSpatial.Position, Vector2.One)))
                {
                    if (spatial.Position == targetSpatial.Position && LocalFaction.CanHarvest(spatial.Entity))
                    {
                        LaunchHarvest(spatial.Entity);
                        return;
                    }
                }
            }
            else
            {
                Health entityHealth = target.Components.TryGet<Health>();
                if (entityHealth != null && entityHealth.Damage > 0)
                {
                    switch (entityHealth.Constitution)
                    {
                        case Constitution.Mechanical:
                            LaunchRepair(target);
                            break;

                        case Constitution.Biological:
                            LaunchHeal(target);
                            break;
                    }

                    return;
                }
            }

            LaunchMove(targetSpatial.Position);
        }
        #endregion

        #region Launching individual commands
        private bool IsControllable(Entity entity)
        {
            Faction entityFaction = FactionMembership.GetFaction(entity);
            return entityFaction != null && entityFaction.GetDiplomaticStance(LocalFaction).HasFlag(DiplomaticStance.SharedControl);
        }

        private void ClearTasksIfNecessary(IEnumerable<Entity> entities)
        {
            if (shiftKeyPressed) return;
            
            commander.LaunchCancelAllTasks(entities);
        }

        public void LaunchBuild(Point location, Entity buildingPrototype)
        {
            IEnumerable<Entity> builders = Selection
                .Where(entity => IsControllable(entity) && Builder.Supports(entity, buildingPrototype));

            ClearTasksIfNecessary(builders);
            commander.LaunchBuild(builders, buildingPrototype, location);
        }

        public void LaunchLoad(Entity target)
        {
            Entity transporter = Selection
                .FirstOrDefault(e => IsControllable(e)
                    && Identity.GetPrototype(e) == SelectionManager.FocusedPrototype
                    && e.Components.Has<Transporter>()
                    && e.Components.Get<Transporter>().RemainingSpace >= Cost.GetResourceAmount(target).Food);

            commander.LaunchLoad(transporter, target);
        }

        public void LaunchUnload()
        {
            IEnumerable<Entity> transporters = Selection
                .Where(e => IsControllable(e)
                    && Identity.GetPrototype(e) == SelectionManager.FocusedPrototype
                    && e.Components.Has<Transporter>());

            commander.LaunchUnload(transporters);
        }

        public void LaunchAttack(Entity target)
        {
            IEnumerable<Entity> entities = Selection.Where(entity => IsControllable(entity)
                && entity.Components.Has<Attacker>() || entity.Components.Has<Mobile>());

            ClearTasksIfNecessary(entities);
            // Those who can attack do so, the others simply move to the target's position
            commander.LaunchAttack(entities.Where(entity => entity.Components.Has<Attacker>()), target);
            commander.LaunchMove(entities.Where(entity => !entity.Components.Has<Attacker>() && entity.Components.Has<Mobile>()), target.Spatial.Position);
        }

        public void LaunchZoneAttack(Vector2 destination)
        {
            IEnumerable<Entity> entities = Selection
                .Where(entity => IsControllable(entity) && entity.Components.Has<Mobile>());

            // Those who can attack do so, the others simply move to the destination
            ClearTasksIfNecessary(entities);
            commander.LaunchZoneAttack(entities.Where(entity => entity.Components.Has<Attacker>()), destination);
            commander.LaunchMove(entities.Where(entity => !entity.Components.Has<Attacker>()), destination);
        }

        public void LaunchHarvest(Entity node)
        {
            Debug.Assert(node.Components.Has<Harvestable>(), "Node is not a resource node!");

            IEnumerable<Entity> entities = Selection
                .Where(entity => IsControllable(entity) && entity.Components.Has<Mobile>());

            // Those who can harvest do so, the others simply move to the resource's position
            ClearTasksIfNecessary(entities);
            commander.LaunchHarvest(entities.Where(entity => entity.Components.Has<Harvester>()), node);
            commander.LaunchMove(entities.Where(entity => !entity.Components.Has<Harvester>()), node.Spatial.Position);
        }

        public void LaunchMove(Vector2 destination)
        {
            IEnumerable<Entity> entities = Selection
                .Where(entity => IsControllable(entity) && entity.Components.Has<Mobile>());

            ClearTasksIfNecessary(entities);
            commander.LaunchMove(entities, destination);
        }

        public void LaunchChangeRallyPoint(Vector2 at)
        {
            IEnumerable<Entity> entities = Selection
                .Where(entity => FactionMembership.GetFaction(entity) == LocalFaction
                    && entity.Components.Has<Trainer>());

            commander.LaunchChangeRallyPoint(entities, at);
        }

        public void LaunchRepair(Entity target)
        {
            Argument.EnsureNotNull(target, "target");

            Health targetHealth = target.Components.TryGet<Health>();
            if (targetHealth == null || targetHealth.Constitution != Constitution.Mechanical) return;

            IEnumerable<Entity> entities = Selection
                .Where(entity => FactionMembership.GetFaction(entity) == LocalFaction && entity.Components.Has<Builder>());

            ClearTasksIfNecessary(entities);
            commander.LaunchRepair(entities, target);
        }

        public void LaunchHeal(Entity target)
        {
            Argument.EnsureNotNull(target, "target");

            Health targetHealth = target.Components.TryGet<Health>();
            if (targetHealth.Constitution != Constitution.Biological) return;

            IEnumerable<Entity> entities = Selection
                .Where(entity => FactionMembership.GetFaction(entity) == LocalFaction && entity.Components.Has<Healer>());

            Faction targetFaction = FactionMembership.GetFaction(target);
            if (entities.Any(entity => FactionMembership.GetFaction(entity) != targetFaction)) return;

            ClearTasksIfNecessary(entities);
            commander.LaunchHeal(entities, target);
        }

        public void LaunchTrain(Entity prototype)
        {
            IEnumerable<Entity> entities = Selection
                .Where(entity =>
                {
                    Trainer trainer = entity.Components.TryGet<Trainer>();
                    return IsControllable(entity)
                        && entity.Components.Has<TaskQueue>()
                        && trainer != null
                        && trainer.Supports(prototype);
                });

            commander.LaunchTrain(entities, prototype);
        }

        public void LaunchResearch(Technology technology)
        {
            foreach (Entity entity in Selection)
            {
                Researcher researcher = entity.Components.TryGet<Researcher>();
                if (FactionMembership.GetFaction(entity) != LocalFaction
                    || !entity.Components.Has<TaskQueue>()
                    || researcher == null
                    || !researcher.Supports(technology))
                {
                    return;
                }

                commander.LaunchResearch(entity, technology);
            }
        }

        public void LaunchSuicide()
        {
            IEnumerable<Entity> entities = Selection
                .Where(entity =>
                {
                    Health health = entity.Components.TryGet<Health>();
                    return FactionMembership.GetFaction(entity) == LocalFaction
                        && health != null
                        && health.CanSuicide;
                });

            commander.LaunchSuicide(entities);
        }

        public void LaunchStandGuard()
        {
            IEnumerable<Entity> entities = Selection
                .Where(entity => FactionMembership.GetFaction(entity) == LocalFaction && entity.Components.Has<Mobile>());

            ClearTasksIfNecessary(entities);
            commander.LaunchStandGuard(entities);
        }

        public void LaunchSell()
        {
            IEnumerable<Entity> entities = Selection
                .Where(entity => FactionMembership.GetFaction(entity) == LocalFaction && entity.Components.Has<Sellable>());

            commander.LaunchSuicide(entities);
        }

        public void LaunchCancelAllTasks()
        {
            IEnumerable<Entity> entities = Selection
                .Where(entity => FactionMembership.GetFaction(entity) == LocalFaction);

            commander.LaunchCancelAllTasks(entities);
        }

        public void LaunchCancelTask(Task task)
        {
            Argument.EnsureNotNull(task, "task");
            Debug.Assert(Selection.Count == 1 && task.Entity == Selection.FirstOrDefault());

            commander.LaunchCancelTask(task);
        }

        public void LaunchChangeDiplomacy(Faction targetFaction, DiplomaticStance newStance)
        {
            Argument.EnsureNotNull(targetFaction, "targetFaction");

            if (LocalFaction.GetDiplomaticStance(targetFaction) == newStance) return;
            
            commander.LaunchChangeDiplomacy(targetFaction, newStance);
        }

        public void LaunchChatMessage(string text)
        {
            Argument.EnsureNotNull(text, "text");

            text = text.Trim();
            if (text.Length == 0) return;

            if (text[0] == '#')
                commander.SendAllyMessage(text.Substring(1));
            else
                commander.SendMessage(text);
        }

        public void LaunchUpgrade(Entity targetPrototype)
        {
            Argument.EnsureNotNull(targetPrototype, "targetPrototype");

            var entities = Selection
                .Where(entity => FactionMembership.GetFaction(entity) == LocalFaction
                    && entity.Identity.Upgrades.Any(upgrade => upgrade.Target == targetPrototype.Identity.Name));

            commander.LaunchUpgrade(entities, targetPrototype);
        }
        #endregion
        #endregion
    }
}
