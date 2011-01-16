using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Data;
using Orion.Engine.Graphics;
using Orion.Engine.Gui2;
using Orion.Engine.Gui2.Adornments;
using Orion.Game.Presentation.Actions;
using Orion.Game.Presentation.Renderers;
using Key = OpenTK.Input.Key;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Provides the in-match user interface.
    /// </summary>
    public sealed partial class MatchUI2 : ContentControl
    {
        #region Fields
        private readonly OrionGuiStyle style;
        private readonly Action<UIManager, TimeSpan> updatedEventHandler;

        private readonly InterpolatedCounter aladdiumAmountCounter = new InterpolatedCounter(0);
        private readonly InterpolatedCounter alageneAmountCounter = new InterpolatedCounter(0);

        private Label aladdiumAmountLabel;
        private Label alageneAmountLabel;
        private Label foodAmountLabel;
        private ImageBox idleWorkerCountImageBox;
        private Label idleWorkerCountLabel;
        private Control bottomBar;
        private ContentControl selectionInfoPanel;
        private TextField chatTextField;
        private MessageConsole messageConsole;
        private GridLayout actionButtonGrid;
        private ActionToolTip actionToolTip;

        private TimeSpan time;
        private TimeSpan lastIdleWorkerCountChangedTime;
        private ActionButton lastActionButtonUnderMouse;
        private Point scrollDirection;
        private bool isLeftPressed, isRightPressed, isUpPressed, isDownPressed;
        private bool isSelectingAllIdleWorkers;
        private bool isFollowingSelection;
        private bool isDisplayingHealthBars;
        #endregion

        #region Constructors
        public MatchUI2(OrionGuiStyle style)
        {
            Argument.EnsureNotNull(style, "style");

            this.style = style;
            updatedEventHandler = OnGuiUpdated;

            DockLayout mainDock = style.Create<DockLayout>();
            Content = mainDock;
            mainDock.LastChildFill = true;
            mainDock.Dock(CreateTopBar(), Direction.NegativeY);
            mainDock.Dock(CreateBottomBar(), Direction.PositiveY);
            mainDock.Dock(CreateOverlays(), Direction.PositiveY);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the view gets moved using the minimap.
        /// The parameter specifies the new world position of the camera,
        /// normalized between (0,0) and (1,1).
        /// </summary>
        public event Action<MatchUI2, Vector2> MinimapCameraMoved;

        /// <summary>
        /// Raised when the minimap receives a right click.
        /// The parameter specifies the world position of the click,
        /// normalized between (0,0) and (1,1).
        /// </summary>
        public event Action<MatchUI2, Vector2> MinimapRightClicked;

        /// <summary>
        /// Raised when the minimap should be rendered.
        /// </summary>
        public event Action<MatchUI2, Region> MinimapRendering;

        /// <summary>
        /// Raised when the game gets zoomed in or out. The parameter specifies the zoom amount.
        /// </summary>
        public event Action<MatchUI2, float> ViewportZoomed;

        /// <summary>
        /// Raised when the user has submitted text using the chat.
        /// </summary>
        public event Action<MatchUI2, string> Chatted;

        /// <summary>
        /// Raised when the user selects idle workers.
        /// The parameter is <c>true</c> if all idle workers should be selected and <c>false</c> if only the next one should.
        /// </summary>
        public event Action<MatchUI2, bool> SelectingIdleWorkers;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the displayed aladdium amount.
        /// </summary>
        public int AladdiumAmount
        {
            get { return aladdiumAmountCounter.TargetValue; }
            set
            {
                Argument.EnsurePositive(value, "AladdiumAmount");
                aladdiumAmountCounter.TargetValue = value;
            }
        }

        /// <summary>
        /// Accesses the displayed alagene amount.
        /// </summary>
        public int AlageneAmount
        {
            get { return alageneAmountCounter.TargetValue; }
            set
            {
                Argument.EnsurePositive(value, "AlageneAmount");
                alageneAmountCounter.TargetValue = value;
            }
        }

        /// <summary>
        /// Accesses the displayed amount of used food.
        /// </summary>
        public int UsedFoodAmount
        {
            get { return int.Parse(foodAmountLabel.Text.Split('/')[0]); }
            set
            {
                // The used food amount can actually be negative because of the twelvehungrymen cheat
                // Argument.EnsurePositive(value, "UsedFoodAmount");
                foodAmountLabel.Text = value + "/" + FoodLimit;
            }
        }

        /// <summary>
        /// Accesses the displayed food limit.
        /// </summary>
        public int FoodLimit
        {
            get { return int.Parse(foodAmountLabel.Text.Split('/')[1]); }
            set
            {
                Argument.EnsurePositive(value, "FoodLimit");
                foodAmountLabel.Text = UsedFoodAmount + "/" + value;
            }
        }

        /// <summary>
        /// Accesses the displayed number of idle workers.
        /// </summary>
        public int IdleWorkerCount
        {
            get { return int.Parse(idleWorkerCountLabel.Text); }
            set
            {
                if (value == IdleWorkerCount) return;
                Argument.EnsurePositive(value, "InactiveWorkerCount");

                idleWorkerCountLabel.Text = value.ToStringInvariant();
                lastIdleWorkerCountChangedTime = time;
            }
        }

        /// <summary>
        /// Gets the screen-space rectangle in which the game can be seen.
        /// </summary>
        public Region ViewportRectangle
        {
            get
            {
                Region rectangle = Rectangle;
                int bottomBarHeight = bottomBar.ActualSize.Height;
                return new Region(
                    rectangle.MinX, rectangle.MinY,
                    rectangle.Width, Math.Max(0, rectangle.Height - bottomBarHeight));
            }
        }

        /// <summary>
        /// Gets a point which indicates the direction the camera should be scrolling.
        /// </summary>
        public Point ScrollDirection
        {
            get { return scrollDirection; }
        }

        /// <summary>
        /// Accesses the control which displays information about the selection.
        /// </summary>
        public Control SelectionInfoPanel
        {
            get { return selectionInfoPanel.Content; }
            set { selectionInfoPanel.Content = value; }
        }

        /// <summary>
        /// Gets a value indicating that the camera should follow the current selection.
        /// </summary>
        public bool IsFollowingSelection
        {
            get { return isFollowingSelection; }
        }

        /// <summary>
        /// Gets a value indicating if all health bars should be displayed.
        /// </summary>
        public bool IsDisplayingHealthBars
        {
            get { return isDisplayingHealthBars; }
        }

        private ActionButton ActionButtonUnderMouse
        {
            get
            {
                if (Manager == null) return null;

                Control controlUnderMouse = Manager.ControlUnderMouse;
                while (controlUnderMouse != null)
                {
                    ActionButton actionButtonUnderMouse = controlUnderMouse as ActionButton;
                    if (actionButtonUnderMouse != null) return actionButtonUnderMouse;
                    controlUnderMouse = controlUnderMouse.Parent;
                }

                return null;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a message to this <see cref="MatchUI2"/>'s message console.
        /// </summary>
        /// <param name="text">The text of the message.</param>
        /// <param name="color">The color of the message.</param>
        public void AddMessage(string text, ColorRgb color)
        {
            messageConsole.AddMessage(text, color);
        }

        /// <summary>
        /// Hides all action buttons.
        /// </summary>
        public void ClearActionButtons()
        {
            foreach (ActionButton button in actionButtonGrid.Children)
                button.VisibilityFlag = Visibility.Hidden;
        }

        /// <summary>
        /// Resets an action button from a descriptor.
        /// </summary>
        /// <param name="rowIndex">The index of the row of the button.</param>
        /// <param name="columnIndex">The index of the column of the button.</param>
        /// <param name="descriptor">
        /// The action descriptor to be applied to the button.
        /// Can be <c>null</c> if the button should not be available.
        /// </param>
        public void SetActionButton(int rowIndex, int columnIndex, ActionDescriptor descriptor)
        {
            ActionButton button = (ActionButton)actionButtonGrid.Children[rowIndex, columnIndex];
            button.Descriptor = descriptor;
        }

        private void UpdateScrollDirection()
        {
            Region rectangle = Rectangle;
            Point mousePosition = Manager.MousePosition;

            int scrollX = 0;
            if (mousePosition.X == 0 || isLeftPressed) scrollX -= 1;
            if (mousePosition.X == rectangle.ExclusiveMaxX - 1 || isRightPressed) scrollX += 1;

            int scrollY = 0;
            if (mousePosition.Y == 0 || isUpPressed) scrollY -= 1;
            if (mousePosition.Y == rectangle.ExclusiveMaxY - 1 || isDownPressed) scrollY += 1;

            scrollDirection = new Point(scrollX, scrollY);
        }

        private void UpdateInactiveWorkerButton()
        {
            if (idleWorkerCountLabel.Text == "0")
            {
                idleWorkerCountImageBox.Color = Colors.White;
                return;
            }

            double timeElapsedInSeconds = time.TotalSeconds - lastIdleWorkerCountChangedTime.TotalSeconds;
            float intensity = (float)((0.2 + Math.Cos(timeElapsedInSeconds * 5) + 1) / 2 * 0.8);

            idleWorkerCountImageBox.Color = new ColorRgb(1, intensity, intensity);
        }

        #region Initialization
        private Control CreateTopBar()
        {
            ContentControl container = new ContentControl();
            container.IsMouseEventSink = true;
            container.HorizontalAlignment = Alignment.Center;
            BorderTextureAdornment adornment = new BorderTextureAdornment(style.GetTexture("Gui/Header"));
            container.Adornment = adornment;
            container.Padding = new Borders(adornment.Texture.Width / 2 - 10, 8);

            DockLayout dock = new DockLayout();
            container.Content = dock;

            StackLayout resourcesStack = new StackLayout();
            dock.Dock(resourcesStack, Direction.NegativeX);
            resourcesStack.Direction = Direction.PositiveX;
            resourcesStack.ChildGap = 10;
            resourcesStack.MaxXMargin = 15;

            ImageBox dummyImageBox;
            resourcesStack.Stack(CreateResourcePanel("Aladdium", out dummyImageBox, out aladdiumAmountLabel));
            Binding.CreateOneWay(() => aladdiumAmountCounter.DisplayedValue, () => aladdiumAmountLabel.Text);

            resourcesStack.Stack(CreateResourcePanel("Alagene", out dummyImageBox, out alageneAmountLabel));
            Binding.CreateOneWay(() => alageneAmountCounter.DisplayedValue, () => alageneAmountLabel.Text);

            resourcesStack.Stack(CreateResourcePanel("Gui/Food", out dummyImageBox, out foodAmountLabel));
            foodAmountLabel.Text = "0/0";

            Button inactiveWorkersButton = new Button()
            {
                AcquireKeyboardFocusWhenPressed = false,
                Content = CreateResourcePanel("Units/Schtroumpf", out idleWorkerCountImageBox, out idleWorkerCountLabel)
            };

            // Hack to detect if button is pressed while shift is down.
            inactiveWorkersButton.MouseMoved += (sender, @event) =>
            {
                isSelectingAllIdleWorkers = @event.IsShiftDown;
                return false;
            };

            inactiveWorkersButton.Clicked += (sender, mouseButton) => SelectingIdleWorkers.Raise(this, isSelectingAllIdleWorkers);
            dock.Dock(inactiveWorkersButton, Direction.NegativeX);

            Button pauseButton = style.CreateTextButton("Pause");
            pauseButton.AcquireKeyboardFocusWhenPressed = false;
            pauseButton.VerticalAlignment = Alignment.Center;
            dock.Dock(pauseButton, Direction.PositiveX);

            Button diplomacyButton = style.CreateTextButton("Diplomatie");
            diplomacyButton.AcquireKeyboardFocusWhenPressed = false;
            diplomacyButton.VerticalAlignment = Alignment.Center;
            diplomacyButton.MaxXMargin = 10;
            dock.Dock(diplomacyButton, Direction.PositiveX);

            return container;
        }

        private StackLayout CreateResourcePanel(string textureName, out ImageBox imageBox, out Label label)
        {
            StackLayout stack = new StackLayout();
            stack.Direction = Direction.PositiveX;

            imageBox = new ImageBox();
            stack.Stack(imageBox);
            imageBox.Texture = style.GetTexture(textureName);
            imageBox.VerticalAlignment = Alignment.Center;
            imageBox.SetSize(30, 30);

            label = style.CreateLabel("0");
            stack.Stack(label);
            label.VerticalAlignment = Alignment.Center;
            label.MinXMargin = 5;
            label.MinWidth = 30;
            label.Color = Colors.White;

            return stack;
        }

        private Control CreateBottomBar()
        {
            ContentControl container = new ContentControl();
            bottomBar = container;
            container.Padding = new Borders(6);
            container.IsMouseEventSink = true;
            container.Adornment = new TilingTextureAdornment(style.GetTexture("Gui/Granite"));

            DockLayout dock = new DockLayout();
            container.Content = dock;
            dock.LastChildFill = true;

            ContentControl minimapBoxContainer = new ContentControl();
            dock.Dock(minimapBoxContainer, Direction.NegativeX);
            minimapBoxContainer.SetSize(200, 200);
            minimapBoxContainer.MaxXMargin = 6;
            minimapBoxContainer.Content = CreateMinimapViewport();

            actionButtonGrid = CreateActionButtons();
            dock.Dock(actionButtonGrid, Direction.PositiveX);

            selectionInfoPanel = new ContentControl();
            dock.Dock(selectionInfoPanel, Direction.PositiveX);
            selectionInfoPanel.Content = new ImageBox
            {
                Texture = style.GetTexture("Gui/Carving"),
                HorizontalAlignment = Alignment.Center,
                VerticalAlignment = Alignment.Center,
            };

            return container;
        }

        private GridLayout CreateActionButtons()
        {
            GridLayout grid = new GridLayout(4, 4);
            grid.AreColumnsUniformSized = true;
            grid.AreRowsUniformSized = true;
            grid.CellGap = 3;
            grid.SetSize(200, 200);

            for (int rowIndex = 0; rowIndex < grid.RowCount; ++rowIndex)
            {
                for (int columnIndex = 0; columnIndex < grid.ColumnCount; ++columnIndex)
                {
                    ActionButton actionButton = style.Create<ActionButton>();
                    actionButton.VisibilityFlag = Visibility.Hidden;
                    grid.Children[rowIndex, columnIndex] = actionButton;
                }
            }

            return grid;
        }

        private Control CreateMinimapViewport()
        {
            ViewportBox minimapBox = new ViewportBox();
            minimapBox.MouseButton += OnMinimapMouseButton;
            minimapBox.MouseMoved += OnMinimapMouseMoved;
            minimapBox.MouseWheel += OnMinimapMouseWheel;
            minimapBox.Rendering += sender => MinimapRendering.Raise(this, sender.Rectangle);

            return minimapBox;
        }

        private Control CreateOverlays()
        {
            DockLayout dock = new DockLayout();

            actionToolTip = new ActionToolTip(style)
            {
                Adornment = new ColoredBackgroundAdornment(Colors.Gray),
                VerticalAlignment = Alignment.Max,
                IsMouseEventSink = true,
                VisibilityFlag = Visibility.Hidden
            };
            dock.Dock(actionToolTip, Direction.PositiveX);

            chatTextField = style.Create<TextField>();
            dock.Dock(chatTextField, Direction.PositiveY);
            chatTextField.MinXMargin = 5;
            chatTextField.MaxYMargin = 5;
            chatTextField.HorizontalAlignment = Alignment.Min;
            chatTextField.Width = 500;
            chatTextField.VisibilityFlag = Visibility.Hidden;
            chatTextField.KeyEvent += OnChatTextFieldKeyEvent;

            messageConsole = new MessageConsole(style)
            {
                Direction = Direction.NegativeY,
                MinXMargin = 5,
                MaxXMargin = 5
            };
            dock.Dock(messageConsole, Direction.PositiveY);

            return dock;
        }
        #endregion

        #region Event Handling
        protected override void OnManagerChanged(UIManager previousManager)
        {
            if (previousManager != null) previousManager.Updated -= updatedEventHandler;
            if (Manager != null)
            {
                Manager.Updated += updatedEventHandler;
                AcquireKeyboardFocus();
            }
        }

        private void OnGuiUpdated(UIManager sender, TimeSpan elapsedTime)
        {
            time += elapsedTime;

            aladdiumAmountCounter.Update(elapsedTime);
            alageneAmountCounter.Update(elapsedTime);

            UpdateScrollDirection();
            UpdateInactiveWorkerButton();

            ActionButton actionButtonUnderMouse = ActionButtonUnderMouse;
            if (actionButtonUnderMouse != lastActionButtonUnderMouse)
            {
                lastActionButtonUnderMouse = actionButtonUnderMouse;
                actionToolTip.Descriptor = actionButtonUnderMouse == null ? null : actionButtonUnderMouse.Descriptor;
            }
        }

        protected override bool OnMouseWheel(MouseEvent @event)
        {
            ViewportZoomed.Raise(this, @event.WheelDelta);
            return true;
        }

        protected override bool OnKeyEvent(KeyEvent @event)
        {
            isDisplayingHealthBars = @event.IsAltDown;

            if (@event.Key == Key.Enter && @event.IsDown)
            {
                chatTextField.VisibilityFlag = Visibility.Visible;
                chatTextField.AcquireKeyboardFocus();
                return true;
            }

            if (@event.Key == Key.Space)
            {
                isFollowingSelection = @event.IsDown;
                return true;
            }

            if (@event.Key == Key.Left) isLeftPressed = @event.IsDown;
            if (@event.Key == Key.Right) isRightPressed = @event.IsDown;
            if (@event.Key == Key.Up) isUpPressed = @event.IsDown;
            if (@event.Key == Key.Down) isDownPressed = @event.IsDown;

            UpdateScrollDirection();

            foreach (ActionButton actionButton in actionButtonGrid.Children)
            {
                ActionDescriptor descriptor = actionButton.Descriptor;
                if (descriptor != null && descriptor.HotKey == @event.Key)
                {
                    if (descriptor.Action != null) descriptor.Action();
                    return true;
                }
            }

            return true;
        }

        private bool OnChatTextFieldKeyEvent(Control sender, KeyEvent @event)
        {
            if ((@event.Key == Key.Enter || @event.Key == Key.Escape) && @event.IsDown)
            {
                string chattedText = chatTextField.Text;

                AcquireKeyboardFocus();
                chatTextField.Text = string.Empty;
                chatTextField.VisibilityFlag = Visibility.Hidden;

                if (@event.Key == Key.Enter && !string.IsNullOrEmpty(chattedText))
                    Chatted.Raise(this, chattedText);

                return true;
            }

            return false;
        }

        private bool OnMinimapMouseButton(Control sender, MouseEvent @event)
        {
            if (@event.Button == MouseButtons.Left)
            {
                if (@event.IsPressed)
                {
                    sender.AcquireMouseCapture();
                    MoveMinimapCamera(sender.Rectangle, @event.Position);
                }
                else
                {
                    sender.ReleaseMouseCapture();
                }

                return true;
            }
            else if (@event.Button == MouseButtons.Right)
            {
                if (@event.IsPressed && sender.IsUnderMouse)
                {
                    Vector2 normalizedPosition = sender.Rectangle.Normalize(sender.Rectangle.Clamp(@event.Position));
                    MinimapRightClicked.Raise(this, normalizedPosition);
                }

                return true;
            }

            return false;
        }

        private bool OnMinimapMouseMoved(Control sender, MouseEvent @event)
        {
            if (sender.HasMouseCapture)
            {
                MoveMinimapCamera(sender.Rectangle, @event.Position);
                return true;
            }

            return false;
        }

        private bool OnMinimapMouseWheel(Control sender, MouseEvent @event)
        {
            ViewportZoomed.Raise(this, @event.WheelDelta);
            return true;
        }

        private void MoveMinimapCamera(Region rectangle, Point position)
        {
            Vector2 normalizedPosition = rectangle.Normalize(rectangle.Clamp(position));
            MinimapCameraMoved.Raise(this, normalizedPosition);
        }
        #endregion
        #endregion
    }
}
