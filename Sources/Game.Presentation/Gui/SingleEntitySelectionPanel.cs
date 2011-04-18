using System;
using System.Collections.Generic;
using System.Diagnostics;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Engine.Localization;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Displays information on a single selected unit.
    /// </summary>
    public sealed partial class SingleEntitySelectionPanel : ContentControl
    {
        #region Fields
        private static readonly Stat[] statsToDisplay = new[]
        {
            Attacker.PowerStat, Attacker.RangeStat,
            Health.ArmorStat,
            Mobile.SpeedStat, Vision.RangeStat
        };

        private readonly GameGraphics graphics;
        private readonly Localizer localizer;
        private readonly List<TodoButton> unusedTodoButtons = new List<TodoButton>();
        private readonly Action<TaskQueue> taskQueueChangedEventHandler;
        private readonly Action<UIManager, TimeSpan> updatedEventHandler;

        private Label nameLabel;
        private Label healthLabel;
        private ImageBox imageBox;
        private FormLayout statsForm;
        private Label todoLabel;
        private StackLayout todoButtonStack;
        private Entity entity;
        private int currentAmount;
        private int currentMaxAmount;
        #endregion

        #region Constructors
        public SingleEntitySelectionPanel(GameGraphics graphics, Localizer localizer)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(localizer, "localizer");

            this.graphics = graphics;
            this.localizer = localizer;
            this.taskQueueChangedEventHandler = UpdateTodoList;
            this.updatedEventHandler = OnUpdated;

            DockLayout dock = new DockLayout();
            Content = dock;
            dock.LastChildFill = true;

            dock.Dock(CreatePhoto(), Direction.NegativeX);
            dock.Dock(CreateInfoForm(), Direction.PositiveX);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the user cancels a <see cref="Task"/> of the selected <see cref="Entity"/>.
        /// </summary>
        public event Action<SingleEntitySelectionPanel, Task> TaskCancelled;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the entity for which information is being displayed.
        /// </summary>
        public Entity Entity
        {
            get { return entity; }
        }

        private OrionGuiStyle Style
        {
            get { return graphics.GuiStyle; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Show information on a given <see cref="Entity"/>.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> for which information is to be shown.</param>
        /// <param name="showTasks">
        /// A value indicating if the <see cref="Task"/>s of the <see cref="Entity"/> are to be shown.
        /// </param>
        public void Show(Entity entity, bool showTasks)
        {
            Argument.EnsureNotNull(entity, "entity");
            if (entity == this.entity) return;

            Clear();
            this.entity = entity;

            Harvestable harvestable = entity.Components.TryGet<Harvestable>();
            if (harvestable == null)
            {
                nameLabel.Text = localizer.GetNoun(entity.Identity.Name);
                imageBox.Texture = graphics.GetEntityTexture(entity);
            }
            else
            {
                nameLabel.Text = localizer.GetNoun(harvestable.Type.ToStringInvariant());
                imageBox.Texture = graphics.GetResourceTexture(harvestable.Type);
            }

            TryUpdateAmount();

            if (showTasks)
            {
                todoLabel.VisibilityFlag = Visibility.Visible;

                TaskQueue taskQueue = entity.Components.TryGet<TaskQueue>();
                if (taskQueue != null)
                {
                    UpdateTodoList(taskQueue);
                    taskQueue.Changed += taskQueueChangedEventHandler;
                }
            }

            statsForm.VisibilityFlag = Visibility.Visible;
            foreach (Stat stat in statsToDisplay)
            {
                float value = (float)entity.GetStatValue(stat);
                if (value == 0) continue;

                string statName = localizer.GetNoun(stat.FullName + "Stat");
                Label headerLabel = Style.CreateLabel(statName + ":");
                Label valueLabel = Style.CreateLabel(value.ToStringInvariant());

                statsForm.Entries.Add(headerLabel, valueLabel);
            }
        }

        /// <summary>
        /// Resets this panel by removing all entity-specific info.
        /// </summary>
        public void Clear()
        {
            if (entity == null) return;

            nameLabel.Text = string.Empty;
            imageBox.Texture = null;
            healthLabel.Text = string.Empty;

            currentAmount = 0;
            currentMaxAmount = 0;

            ClearTodoList();
            todoLabel.VisibilityFlag = Visibility.Hidden;

            statsForm.Entries.Clear();
            statsForm.VisibilityFlag = Visibility.Hidden;

            TaskQueue taskQueue = entity.Components.TryGet<TaskQueue>();
            if (taskQueue != null) taskQueue.Changed -= taskQueueChangedEventHandler;

            this.entity = null;
        }

        private void ClearTodoList()
        {
            foreach (TodoButton todoButton in todoButtonStack.Children)
            {
                // Stop referencing the task to prevent keeping alive units
                todoButton.Task = null;
                unusedTodoButtons.Add(todoButton);
            }

            todoButtonStack.Children.Clear();
        }

        private void TryUpdateAmount()
        {
            int newAmount = currentAmount;
            int newMaxAmount = currentMaxAmount;

            Health health = entity.Components.TryGet<Health>();
            Harvestable harvestable = entity.Components.TryGet<Harvestable>();
            if (health != null)
            {
                newMaxAmount = (int)entity.GetStatValue(Health.MaxValueStat);
                newAmount = (int)Math.Ceiling(newMaxAmount - health.Damage);
            }
            else if (harvestable != null)
            {
                newMaxAmount = 0;
                newAmount = harvestable.Amount;
            }

            if (newAmount != currentAmount || newMaxAmount != currentMaxAmount)
            {
                healthLabel.VisibilityFlag = newAmount == 0 ? Visibility.Hidden : Visibility.Visible;

                healthLabel.Text = newAmount.ToStringInvariant();
                if (newMaxAmount != 0) healthLabel.Text += "/" + newMaxAmount;

                currentMaxAmount = newMaxAmount;
                currentAmount = newAmount;
            }
        }

        private void UpdateTodoList(TaskQueue taskQueue)
        {
            ClearTodoList();

            if (taskQueue.Count == 0)
            {
                todoLabel.VisibilityFlag = Visibility.Hidden;
                return;
            }

            todoLabel.VisibilityFlag = Visibility.Visible;

            foreach (Task task in taskQueue)
            {
                TodoButton button = GetTodoButton();
                button.Task = task;
                todoButtonStack.Stack(button);
            }
        }

        protected override void OnManagerChanged(UIManager previousManager)
        {
            if (previousManager != null) previousManager.Updated -= updatedEventHandler;
            if (Manager != null) Manager.Updated += updatedEventHandler;
        }

        private void OnUpdated(UIManager sender, TimeSpan elapsedTime)
        {
            if (entity != null) TryUpdateAmount();
        }

        private TodoButton GetTodoButton()
        {
            if (unusedTodoButtons.Count > 0)
            {
                TodoButton button = unusedTodoButtons[unusedTodoButtons.Count - 1];
                unusedTodoButtons.RemoveAt(unusedTodoButtons.Count - 1);
                return button;
            }

            return new TodoButton(this);
        }

        private Control CreatePhoto()
        {
            DockLayout photoDockPanel = new DockLayout()
            {
                MaxXMargin = 10,
                VerticalAlignment = Alignment.Center,
                Height = 120,
                Width = 140,
                LastChildFill = true
            };

            nameLabel = Style.Create<Label>();
            photoDockPanel.Dock(nameLabel, Direction.NegativeY);
            nameLabel.HorizontalAlignment = Alignment.Center;

            healthLabel = Style.Create<Label>();
            photoDockPanel.Dock(healthLabel, Direction.PositiveY);
            healthLabel.HorizontalAlignment = Alignment.Center;

            imageBox = new ImageBox();
            photoDockPanel.Dock(imageBox, Direction.NegativeY);
            imageBox.HorizontalAlignment = Alignment.Center;

            return photoDockPanel;
        }

        private Control CreateInfoForm()
        {
            DockLayout dock = new DockLayout();
            dock.LastChildFill = true;

            StackLayout topStack = new StackLayout();
            topStack.Direction = Direction.PositiveX;
            topStack.MinHeight = 32;
            topStack.MaxYMargin = 6;

            todoLabel = Style.CreateLabel(localizer.GetNoun("Tasks"));
            todoLabel.VerticalAlignment = Alignment.Center;
            todoLabel.MaxXMargin = 6;
            topStack.Stack(todoLabel);

            todoButtonStack = new StackLayout()
            {
                Direction = Direction.PositiveX,
                VerticalAlignment = Alignment.Center,
                ChildGap = 3
            };
            topStack.Stack(todoButtonStack);

            dock.Dock(topStack, Direction.NegativeY);

            statsForm = new FormLayout();
            statsForm.VerticalAlignment = Alignment.Negative;
            statsForm.HeaderContentGap = 5;
            statsForm.EntryGap = 6;

            dock.Dock(statsForm, Direction.PositiveY);

            return dock;
        }
        #endregion
    }
}
