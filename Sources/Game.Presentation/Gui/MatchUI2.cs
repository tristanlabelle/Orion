using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Gui2;
using Orion.Engine.Gui2.Adornments;
using Orion.Game.Presentation.Renderers;
using Orion.Engine.Graphics;
using Orion.Engine.Data;
using Key = OpenTK.Input.Key;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Provides the in-match user interface.
    /// </summary>
    public sealed class MatchUI2 : ContentControl
    {
        #region Fields
        private readonly OrionGuiStyle style;
        private readonly Action<UIManager, TimeSpan> updatedEventHandler;

        private readonly InterpolatedCounter aladdiumAmountCounter = new InterpolatedCounter(0);
        private readonly InterpolatedCounter alageneAmountCounter = new InterpolatedCounter(0);

        private Label aladdiumAmountLabel;
        private Label alageneAmountLabel;
        private Label foodAmountLabel;
        private ImageBox inactiveWorkerCountImageBox;
        private Label inactiveWorkerCountLabel;
        private Control bottomBar;
        private ContentControl selectionInfoPanel;
        private TextField chatTextField;
        private StackLayout messageStack;
        private GridLayout actionButtonGrid;
        private TimeSpan time;
        private TimeSpan lastInactiveWorkerCountChangedTime;

        private Point scrollDirection;
        private bool isLeftPressed, isRightPressed, isUpPressed, isDownPressed;
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
            mainDock.Dock(CreateTopBar(), Direction.MinY);
            mainDock.Dock(CreateBottomBar(), Direction.MaxY);
            mainDock.Dock(CreateChatOverlays(), Direction.MaxY);
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
        /// Raised when the user has submitted text using the chat.
        /// </summary>
        public event Action<MatchUI2, string> Chatted;

        /// <summary>
        /// Raised when one of the action buttons gets clicked.
        /// The parameters specify the row and column indices of the button.
        /// </summary>
        public event Action<MatchUI2, int, int> ActionButtonClicked;

        /// <summary>
        /// Raised when the button showing the number of inactive workers gets pressed.
        /// </summary>
        public event Action<MatchUI2> InactiveWorkersButtonPressed;
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
                Argument.EnsurePositive(value, "UsedFoodAmount");
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
        /// Accesses the displayed number of inactive workers.
        /// </summary>
        public int InactiveWorkerCount
        {
            get { return int.Parse(inactiveWorkerCountLabel.Text); }
            set
            {
                if (value == InactiveWorkerCount) return;
                Argument.EnsurePositive(value, "InactiveWorkerCount");

                inactiveWorkerCountLabel.Text = value.ToStringInvariant();
                lastInactiveWorkerCountChangedTime = time;
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
        public Control SelectionInfoControl
        {
            get { return selectionInfoPanel.Content; }
            set { selectionInfoPanel.Content = value; }
        }
        #endregion

        #region Methods
        public void AddMessage(string text, ColorRgb color)
        {
            Label label = style.CreateLabel(text);
            label.Color = color;
            messageStack.Stack(label);
        }

        public void SetActionButton(int rowIndex, int columnIndex, Texture texture)
        {
            Button button = (Button)actionButtonGrid.Children[rowIndex, columnIndex];
            button.Visibility = Visibility.Visible;
            ImageBox imageBox = (ImageBox)button.Content;
            imageBox.Texture = texture;
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
            if (inactiveWorkerCountLabel.Text == "0")
            {
                inactiveWorkerCountImageBox.Color = Colors.White;
                return;
            }

            double timeElapsedInSeconds = time.TotalSeconds - lastInactiveWorkerCountChangedTime.TotalSeconds;
            float intensity = (float)((0.2 + Math.Cos(timeElapsedInSeconds * 5) + 1) / 2 * 0.8);

            inactiveWorkerCountImageBox.Color = new ColorRgb(1, intensity, intensity);
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
            dock.Dock(resourcesStack, Direction.MinX);
            resourcesStack.Direction = Direction.MaxX;
            resourcesStack.ChildGap = 10;
            resourcesStack.MaxXMargin = 15;

            ImageBox dummyImageBox;
            resourcesStack.Stack(CreateResourcePanel("Aladdium", out dummyImageBox, out aladdiumAmountLabel));
            Binding.CreateOneWay(() => aladdiumAmountCounter.DisplayedValue, () => aladdiumAmountLabel.Text);

            resourcesStack.Stack(CreateResourcePanel("Alagene", out dummyImageBox, out alageneAmountLabel));
            Binding.CreateOneWay(() => alageneAmountCounter.DisplayedValue, () => alageneAmountLabel.Text);

            resourcesStack.Stack(CreateResourcePanel("Gui/Food", out dummyImageBox, out foodAmountLabel));
            foodAmountLabel.Text = "0/0";

            Button inactiveWorkersButton = new Button();
            dock.Dock(inactiveWorkersButton, Direction.MinX);
            inactiveWorkersButton.Content = CreateResourcePanel("Units/Schtroumpf", out inactiveWorkerCountImageBox, out inactiveWorkerCountLabel);
            inactiveWorkersButton.Clicked += (sender, mouseButton) => InactiveWorkersButtonPressed.Raise(this);

            Button pauseButton = style.CreateTextButton("Pause");
            dock.Dock(pauseButton, Direction.MaxX);
            pauseButton.VerticalAlignment = Alignment.Center;

            Button diplomacyButton = style.CreateTextButton("Diplomatie");
            dock.Dock(diplomacyButton, Direction.MaxX);
            diplomacyButton.VerticalAlignment = Alignment.Center;
            diplomacyButton.MaxXMargin = 10;

            return container;
        }

        private StackLayout CreateResourcePanel(string textureName, out ImageBox imageBox, out Label label)
        {
            StackLayout stack = new StackLayout();
            stack.Direction = Direction.MaxX;

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
            dock.Dock(minimapBoxContainer, Direction.MinX);
            minimapBoxContainer.SetSize(200, 200);
            minimapBoxContainer.MaxXMargin = 6;
            minimapBoxContainer.Content = CreateMinimapViewport();

            actionButtonGrid = CreateActionButtons();
            dock.Dock(actionButtonGrid, Direction.MaxX);

            selectionInfoPanel = new ContentControl();
            dock.Dock(selectionInfoPanel, Direction.MaxX);
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
                    Button actionButton = style.Create<Button>();
                    grid.Children[rowIndex, columnIndex] = actionButton;
                    //actionButton.Visibility = Visibility.Hidden;
                    actionButton.Content = new ImageBox();
                    actionButton.Clicked += OnActionButtonClicked;
                }
            }

            return grid;
        }

        private Control CreateMinimapViewport()
        {
            ViewportBox minimapBox = new ViewportBox();
            minimapBox.MouseButton += OnMinimapMouseButton;
            minimapBox.MouseMoved += OnMinimapMouseMoved;
            minimapBox.Rendering += sender => MinimapRendering.Raise(this, sender.Rectangle);

            return minimapBox;
        }

        private Control CreateChatOverlays()
        {
            DockLayout dock = new DockLayout();

            chatTextField = style.Create<TextField>();
            dock.Dock(chatTextField, Direction.MinY);
            chatTextField.MinXMargin = 5;
            chatTextField.MinYMargin = 5;
            chatTextField.HorizontalAlignment = Alignment.Min;
            chatTextField.Width = 500;
            chatTextField.Visibility = Visibility.Hidden;
            chatTextField.KeyEvent += OnChatTextFieldKeyEvent;

            messageStack = new StackLayout();
            dock.Dock(messageStack, Direction.MinY);

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
        }

        protected override bool OnKeyEvent(KeyEvent @event)
        {
            if (@event.Key == Key.Enter && @event.IsDown)
            {
                chatTextField.Visibility = Visibility.Visible;
                chatTextField.AcquireKeyboardFocus();
                return true;
            }

            if (@event.Key == Key.Left) isLeftPressed = @event.IsDown;
            if (@event.Key == Key.Right) isRightPressed = @event.IsDown;
            if (@event.Key == Key.Up) isUpPressed = @event.IsDown;
            if (@event.Key == Key.Down) isDownPressed = @event.IsDown;

            UpdateScrollDirection();

            return true;
        }

        private bool OnChatTextFieldKeyEvent(Control sender, KeyEvent @event)
        {
            if ((@event.Key == Key.Enter || @event.Key == Key.Escape) && @event.IsDown)
            {
                string chattedText = chatTextField.Text;

                AcquireKeyboardFocus();
                chatTextField.Text = string.Empty;
                chatTextField.Visibility = Visibility.Hidden;

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

        private void MoveMinimapCamera(Region rectangle, Point position)
        {
            Vector2 normalizedPosition = rectangle.Normalize(rectangle.Clamp(position));
            MinimapCameraMoved.Raise(this, normalizedPosition);
        }
        
        private void OnActionButtonClicked(Button sender, MouseButtons mouseButton)
        {
            int rowIndex, columnIndex;
            if (!actionButtonGrid.Children.Find(sender, out rowIndex, out columnIndex))
            {
                Debug.Fail("An action button that wasn't a child of the action button grid was clicked.");
                return;
            }

            ActionButtonClicked.Raise(this, rowIndex, columnIndex);
        }
        #endregion
        #endregion
    }
}
