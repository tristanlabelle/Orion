using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Displays information on a single selected unit.
    /// </summary>
    public sealed partial class SingleEntitySelectionPanel : ContentControl
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly List<TodoButton> unusedTodoButtons = new List<TodoButton>();
        private readonly Action<TaskQueue> taskQueueChangedEventHandler;
        private readonly Action<ResourceNode> remainingAmountChangedEventHandler;

        private Label nameLabel;
        private Label healthLabel;
        private ImageBox imageBox;
        private GridPanel infoGridPanel;
        private StackPanel todoStackPanel;
        private Entity entity;
        #endregion

        #region Constructors
        public SingleEntitySelectionPanel(GameGraphics graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;
            this.taskQueueChangedEventHandler = UpdateTodoList;
            this.remainingAmountChangedEventHandler = UpdateAmount;

            DockPanel mainDockPanel = new DockPanel();
            Content = mainDockPanel;
            mainDockPanel.LastChildFill = true;

            mainDockPanel.Dock(CreatePhoto(), Direction.MinX);
            mainDockPanel.Dock(CreateInfoGrid(), Direction.MaxX);
        }
        #endregion

        #region Properties
        public Entity Entity
        {
            get { return entity; }
            set
            {
                if (value == entity) return;

                if (entity != null) ClearEntity();
                entity = value;
                if (entity != null) SetEntity();
            }
        }

        private OrionGuiStyle Style
        {
            get { return graphics.GuiStyle; }
        }
        #endregion

        #region Methods
        private void ClearEntity()
        {
            nameLabel.Text = string.Empty;
            imageBox.Texture = null;
            healthLabel.Text = string.Empty;
            ClearTodoList();

            if (entity is Unit)
            {
                Unit unit = (Unit)entity;

                unit.TaskQueue.Changed -= taskQueueChangedEventHandler;
            }
            else if (entity is ResourceNode)
            {
                ResourceNode resourceNode = (ResourceNode)entity;

                resourceNode.RemainingAmountChanged -= remainingAmountChangedEventHandler;
            }
        }

        private void SetEntity()
        {
            if (entity is Unit)
            {
                Unit unit = (Unit)entity;

                nameLabel.Text = unit.Type.Name;
                imageBox.Texture = graphics.GetUnitTexture(unit.Type.Name);
                healthLabel.Text = (int)Math.Ceiling(unit.Health) + "/" + unit.MaxHealth;
                UpdateTodoList(unit.TaskQueue);

                unit.TaskQueue.Changed += taskQueueChangedEventHandler;
            }
            else if (entity is ResourceNode)
            {
                ResourceNode resourceNode = (ResourceNode)entity;

                nameLabel.Text = resourceNode.Type.ToStringInvariant();
                imageBox.Texture = graphics.GetResourceTexture(resourceNode);
                UpdateAmount(resourceNode);

                resourceNode.RemainingAmountChanged += remainingAmountChangedEventHandler;
            }
        }

        private void ClearTodoList()
        {
            foreach (TodoButton todoButton in todoStackPanel.Children)
                unusedTodoButtons.Add(todoButton);

            todoStackPanel.Children.Clear();
        }

        private void UpdateTodoList(TaskQueue taskQueue)
        {
            ClearTodoList();

            foreach (Task task in taskQueue)
            {
                TodoButton button = GetTodoButton();
                button.Texture = graphics.GetActionTexture(task);
                todoStackPanel.Stack(button);
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
            DockPanel photoDockPanel = new DockPanel();
            photoDockPanel.LastChildFill = true;
            photoDockPanel.MaxXMargin = 10;
            photoDockPanel.VerticalAlignment = Alignment.Center;
            photoDockPanel.SetSize(120, 120);

            nameLabel = Style.Create<Label>();
            photoDockPanel.Dock(nameLabel, Direction.MinY);
            nameLabel.HorizontalAlignment = Alignment.Center;

            healthLabel = Style.Create<Label>();
            photoDockPanel.Dock(healthLabel, Direction.MaxY);
            healthLabel.HorizontalAlignment = Alignment.Center;

            imageBox = new ImageBox();
            photoDockPanel.Dock(imageBox, Direction.MinY);
            imageBox.HorizontalAlignment = Alignment.Center;

            return photoDockPanel;
        }

        private GridPanel CreateInfoGrid()
        {
            infoGridPanel = new GridPanel(1, 2);
            infoGridPanel.AreRowsUniformSized = true;
            infoGridPanel.VerticalAlignment = Alignment.Min;

            Label todoLabel = Style.CreateLabel("Todo:");
            todoLabel.MaxXMargin = 5;
            todoLabel.VerticalAlignment = Alignment.Center;
            infoGridPanel.Children[0, 0] = todoLabel;
            todoStackPanel = new StackPanel()
            {
                Direction = Direction.MaxX,
                ChildGap = 3
            };
            infoGridPanel.Children[0, 1] = todoStackPanel;

            return infoGridPanel;
        }
        #endregion
    }
}
