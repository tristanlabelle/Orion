using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Skills;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Displays information on a single selected unit.
    /// </summary>
    public sealed partial class SingleEntitySelectionPanel : ContentControl
    {
        #region Fields
        private static readonly UnitStat[] statsToDisplay = new[]
        {
            AttackSkill.PowerStat, AttackSkill.RangeStat,
            BasicSkill.ArmorStat, BasicSkill.ArmorTypeStat,
            MoveSkill.SpeedStat, BasicSkill.SightRangeStat
        };

        private readonly GameGraphics graphics;
        private readonly List<TodoButton> unusedTodoButtons = new List<TodoButton>();
        private readonly Action<Unit> damageChangedEventHandler;
        private readonly Action<TaskQueue> taskQueueChangedEventHandler;
        private readonly Action<ResourceNode> remainingAmountChangedEventHandler;

        private Label nameLabel;
        private Label healthLabel;
        private ImageBox imageBox;
        private FormLayout statsForm;
        private Label todoLabel;
        private StackLayout todoButtonStack;
        private Entity entity;
        #endregion

        #region Constructors
        public SingleEntitySelectionPanel(GameGraphics graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.damageChangedEventHandler = UpdateHealth;
            this.taskQueueChangedEventHandler = UpdateTodoList;
            this.remainingAmountChangedEventHandler = UpdateAmount;

            DockLayout dock = new DockLayout();
            Content = dock;
            dock.LastChildFill = true;

            dock.Dock(CreatePhoto(), Direction.NegativeX);
            dock.Dock(CreateInfoForm(), Direction.PositiveX);
        }
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
        /// Show information on a given unit.
        /// </summary>
        /// <param name="unit">The <see cref="Unit"/> for which information is to be shown.</param>
        /// <param name="showTasks">
        /// A value indicating if the <see cref="Task"/>s of the <see cref="Unit"/> are to be shown.
        /// </param>
        public void ShowUnit(Unit unit, bool showTasks)
        {
            Argument.EnsureNotNull(unit, "unit");
            if (unit == entity) return;
            Clear();

            nameLabel.Text = unit.Type.Name;
            imageBox.Texture = graphics.GetUnitTexture(unit.Type.Name);

            UpdateHealth(unit);
            unit.DamageChanged += damageChangedEventHandler;

            if (showTasks)
            {
                todoLabel.VisibilityFlag = Visibility.Visible;
                UpdateTodoList(unit.TaskQueue);
                unit.TaskQueue.Changed += taskQueueChangedEventHandler;
            }

            statsForm.VisibilityFlag = Visibility.Visible;
            foreach (UnitStat stat in statsToDisplay)
            {
                if (!unit.Type.HasSkill(stat.SkillType)) continue;

                int value = unit.GetStat(stat);
                if (value == 0) continue;

                Label headerLabel = Style.CreateLabel(stat.Description + ":");
                Label valueLabel = Style.CreateLabel(value.ToStringInvariant());

                statsForm.Entries.Add(headerLabel, valueLabel);
            }

            entity = unit;
        }

        /// <summary>
        /// Shows information on a given <see cref="ResourceNode"/>.
        /// </summary>
        /// <param name="resourceNode">The <see cref="ResourceNode"/> for which information is to be shown.</param>
        public void ShowResourceNode(ResourceNode resourceNode)
        {
            Argument.EnsureNotNull(resourceNode, "node");
            if (resourceNode == entity) return;
            Clear();

            nameLabel.Text = resourceNode.Type.ToStringInvariant();
            imageBox.Texture = graphics.GetResourceTexture(resourceNode);
            UpdateAmount(resourceNode);

            resourceNode.RemainingAmountChanged += remainingAmountChangedEventHandler;

            entity = resourceNode;
        }

        /// <summary>
        /// Resets this panel by removing all entity-specific info.
        /// </summary>
        public void Clear()
        {
            nameLabel.Text = string.Empty;
            imageBox.Texture = null;
            healthLabel.Text = string.Empty;

            ClearTodoList();
            todoLabel.VisibilityFlag = Visibility.Hidden;

            statsForm.Entries.Clear();
            statsForm.VisibilityFlag = Visibility.Hidden;

            if (entity is Unit)
            {
                Unit unit = (Unit)entity;

                unit.DamageChanged -= damageChangedEventHandler;
                unit.TaskQueue.Changed -= taskQueueChangedEventHandler;
            }
            else if (entity is ResourceNode)
            {
                ResourceNode resourceNode = (ResourceNode)entity;

                resourceNode.RemainingAmountChanged -= remainingAmountChangedEventHandler;
            }

            entity = null;
        }

        private void ClearTodoList()
        {
            foreach (TodoButton todoButton in todoButtonStack.Children)
                unusedTodoButtons.Add(todoButton);

            todoButtonStack.Children.Clear();
        }

        private void UpdateHealth(Unit unit)
        {
            healthLabel.Text = (int)Math.Ceiling(unit.Health) + "/" + unit.MaxHealth;
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

        private void UpdateAmount(ResourceNode resourceNode)
        {
            healthLabel.Text = resourceNode.RemainingAmount + "/" + resourceNode.TotalAmount;
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

            todoLabel = Style.CreateLabel("Todo:");
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
