﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Game.Presentation.Renderers;
using Orion.Engine;
using Orion.Engine.Gui2.Adornments;
using System.Globalization;
using OpenTK;
using MouseButtons = System.Windows.Forms.MouseButtons;
using Keys = System.Windows.Forms.Keys;
using System.Diagnostics;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Provides the in-match user interface.
    /// </summary>
    public sealed class MatchUI2 : ContentControl
    {
        #region Fields
        private readonly Action<UIManager, TimeSpan> updatedEventHandler;

        private Label aladdiumAmountLabel;
        private Label alageneAmountLabel;
        private Label foodAmountLabel;
        private Label inactiveWorkerCountLabel;
        private Control bottomBar;
        private ContentControl selectionInfoPanel;
        private TextField chatTextField;
        private UniformGridPanel actionButtonGrid;

        private Point scrollDirection;
        private bool isLeftPressed, isRightPressed, isUpPressed, isDownPressed;
        #endregion

        #region Constructors
        public MatchUI2(OrionGuiStyle style)
        {
            Argument.EnsureNotNull(style, "style");

            updatedEventHandler = OnGuiUpdated;

            DockPanel mainDockPanel = style.Create<DockPanel>();
            mainDockPanel.LastChildFill = true;
            mainDockPanel.Dock(CreateTopBar(style), Direction.MaxY);
            mainDockPanel.Dock(CreateBottomBar(style), Direction.MinY);
            mainDockPanel.Dock(CreateChatOverlays(style), Direction.MinY);

            Content = mainDockPanel;
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
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the displayed aladdium amount.
        /// </summary>
        public int AladdiumAmount
        {
            get { return int.Parse(aladdiumAmountLabel.Text, NumberFormatInfo.InvariantInfo); }
            set { aladdiumAmountLabel.Text = value.ToStringInvariant(); }
        }

        /// <summary>
        /// Accesses the displayed alagene amount.
        /// </summary>
        public int AlageneAmount
        {
            get { return int.Parse(alageneAmountLabel.Text, NumberFormatInfo.InvariantInfo); }
            set { alageneAmountLabel.Text = value.ToStringInvariant(); }
        }

        /// <summary>
        /// Accesses the displayed amount of used food.
        /// </summary>
        public int UsedFoodAmount
        {
            get { return int.Parse(foodAmountLabel.Text.Split('/')[0]); }
            set { foodAmountLabel.Text = value + "/" + FoodLimit; }
        }

        /// <summary>
        /// Accesses the displayed food limit.
        /// </summary>
        public int FoodLimit
        {
            get { return int.Parse(foodAmountLabel.Text.Split('/')[1]); }
            set { foodAmountLabel.Text = UsedFoodAmount + "/" + value; }
        }

        /// <summary>
        /// Accesses the displayed number of inactive workers.
        /// </summary>
        public int InactiveWorkerCount
        {
            get { return int.Parse(inactiveWorkerCountLabel.Text); }
            set { inactiveWorkerCountLabel.Text = value.ToStringInvariant(); }
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
                    rectangle.MinX, rectangle.MinY + bottomBarHeight,
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
        #endregion

        #region Methods
        protected override void OnManagerChanged(UIManager previousManager)
        {
            if (previousManager != null) previousManager.Updated -= updatedEventHandler;
            if (Manager != null)
            {
                Manager.Updated += updatedEventHandler;
                AcquireKeyboardFocus();
            }
        }

        protected override bool OnKey(Keys key, Keys modifiers, bool pressed)
        {
            if (key == Keys.Enter && pressed)
            {
                chatTextField.Visibility = Visibility.Visible;
                chatTextField.AcquireKeyboardFocus();
                return true;
            }

            if (key == Keys.Left) isLeftPressed = pressed;
            if (key == Keys.Right) isRightPressed = pressed;
            if (key == Keys.Up) isUpPressed = pressed;
            if (key == Keys.Down) isDownPressed = pressed;

            UpdateScrollDirection();

            return true;
        }

        private void OnGuiUpdated(UIManager sender, TimeSpan elapsedTime)
        {
            UpdateScrollDirection();
        }

        private void UpdateScrollDirection()
        {
            Region rectangle = Rectangle;
            Point mousePosition = Manager.MouseState.Position;

            int scrollX = 0;
            if (mousePosition.X == 0 || isLeftPressed) scrollX += -1;
            if (mousePosition.X == rectangle.ExclusiveMaxX - 1 || isRightPressed) scrollX += 1;

            int scrollY = 0;
            if (mousePosition.Y == 0 || isDownPressed) scrollY += -1;
            if (mousePosition.Y == rectangle.ExclusiveMaxY - 1 || isUpPressed) scrollY += 1;

            scrollDirection = new Point(scrollX, scrollY);
        }

        private Control CreateTopBar(OrionGuiStyle style)
        {
            ContentControl container = new ContentControl();
            container.IsMouseEventSink = true;
            container.HorizontalAlignment = Alignment.Center;
            BorderTextureAdornment adornment = new BorderTextureAdornment(style.GetTexture("Gui/Header"));
            container.Adornment = adornment;
            container.Padding = new Borders(adornment.Texture.Width / 2 - 10, 8);

            DockPanel dockPanel = style.Create<DockPanel>();
            container.Content = dockPanel;

            dockPanel.Dock(CreateResourceImageBox(style, "Aladdium"), Direction.MinX);
            aladdiumAmountLabel = CreateResourceLabel(style);
            dockPanel.Dock(aladdiumAmountLabel, Direction.MinX);

            dockPanel.Dock(CreateResourceImageBox(style, "Alagene"), Direction.MinX);
            alageneAmountLabel = CreateResourceLabel(style);
            dockPanel.Dock(alageneAmountLabel, Direction.MinX);

            dockPanel.Dock(CreateResourceImageBox(style, "Gui/Food"), Direction.MinX);
            foodAmountLabel = CreateResourceLabel(style);
            dockPanel.Dock(foodAmountLabel, Direction.MinX);
            foodAmountLabel.Text = "0/0";

            dockPanel.Dock(CreateResourceImageBox(style, "Units/Schtroumpf"), Direction.MinX);
            inactiveWorkerCountLabel = CreateResourceLabel(style);
            dockPanel.Dock(inactiveWorkerCountLabel, Direction.MinX);

            Button pauseButton = style.CreateTextButton("Pause");
            dockPanel.Dock(pauseButton, Direction.MaxX);
            pauseButton.VerticalAlignment = Alignment.Center;

            Button diplomacyButton = style.CreateTextButton("Diplomatie");
            dockPanel.Dock(diplomacyButton, Direction.MaxX);
            diplomacyButton.VerticalAlignment = Alignment.Center;
            diplomacyButton.MaxXMargin = 10;

            return container;
        }

        private static ImageBox CreateResourceImageBox(OrionGuiStyle style, string textureName)
        {
            ImageBox imageBox = new ImageBox();
            imageBox.Texture = style.GetTexture(textureName);
            imageBox.VerticalAlignment = Alignment.Center;
            imageBox.SetSize(30, 30);
            return imageBox;
        }

        private static Label CreateResourceLabel(OrionGuiStyle style)
        {
            Label label = style.CreateLabel("0");
            label.VerticalAlignment = Alignment.Center;
            label.MinXMargin = 5;
            label.MinWidth = 30;
            label.Color = Colors.White;
            label.MaxXMargin = 10;
            return label;
        }

        private Control CreateBottomBar(OrionGuiStyle style)
        {
            ContentControl container = new ContentControl();
            bottomBar = container;
            container.IsMouseEventSink = true;
            container.Adornment = new TilingTextureAdornment(style.GetTexture("Gui/Granite"));

            DockPanel dockPanel = new DockPanel();
            container.Content = dockPanel;
            dockPanel.LastChildFill = true;

            ContentControl minimapBoxContainer = new ContentControl();
            dockPanel.Dock(minimapBoxContainer, Direction.MinX);
            minimapBoxContainer.SetSize(200, 200);
            minimapBoxContainer.Padding = new Borders(6);
            minimapBoxContainer.Content = CreateMinimapViewport();

            actionButtonGrid = CreateActionButtons(style);
            dockPanel.Dock(actionButtonGrid, Direction.MaxX);
            actionButtonGrid.SetSize(200, 200);

            selectionInfoPanel = new ContentControl();
            dockPanel.Dock(selectionInfoPanel, Direction.MaxX);
            selectionInfoPanel.Content = new ImageBox
            {
                Texture = style.GetTexture("Gui/Carving"),
                HorizontalAlignment = Alignment.Center,
                VerticalAlignment = Alignment.Center,
            };

            return container;
        }

        private UniformGridPanel CreateActionButtons(OrionGuiStyle style)
        {
            UniformGridPanel grid = new UniformGridPanel(3, 3);
            grid.CellGap = 3;

            for (int rowIndex = 0; rowIndex < grid.RowCount; ++rowIndex)
            {
                for (int columnIndex = 0; columnIndex < grid.ColumnCount; ++columnIndex)
                {
                    Button actionButton = style.Create<Button>();
                    grid.Children[rowIndex, columnIndex] = actionButton;
                    actionButton.Visibility = Visibility.Hidden;
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

        private Control CreateChatOverlays(OrionGuiStyle style)
        {
            DockPanel dockPanel = new DockPanel();

            chatTextField = style.Create<TextField>();
            dockPanel.Dock(chatTextField, Direction.MinY);
            chatTextField.MinXMargin = 5;
            chatTextField.MinYMargin = 5;
            chatTextField.HorizontalAlignment = Alignment.Min;
            chatTextField.Width = 500;
            chatTextField.Visibility = Visibility.Hidden;
            chatTextField.Key += OnChatTextFieldKey;

            return dockPanel;
        }

        private bool OnChatTextFieldKey(Control sender, Keys key, bool pressed)
        {
            if ((key == Keys.Enter || key == Keys.Escape) && pressed)
            {
                string chattedText = chatTextField.Text;

                AcquireKeyboardFocus();
                chatTextField.Text = string.Empty;
                chatTextField.Visibility = Visibility.Hidden;

                if (key == Keys.Enter && !string.IsNullOrEmpty(chattedText))
                    Chatted.Raise(this, chattedText);

                return true;
            }

            return false;
        }

        private bool OnMinimapMouseButton(Control sender, MouseState mouseState, MouseButtons button, int pressCount)
        {
            if (button == MouseButtons.Left)
            {
                if (pressCount > 0)
                {
                    sender.AcquireMouseCapture();
                    MoveMinimapCamera(sender.Rectangle, mouseState.Position);
                }
                else
                {
                    sender.ReleaseMouseCapture();
                }

                return true;
            }
            else if (button == MouseButtons.Right)
            {
                if (pressCount > 0 && sender.IsUnderMouse)
                {
                    Vector2 normalizedPosition = sender.Rectangle.Normalize(sender.Rectangle.Clamp(mouseState.Position));
                    MinimapRightClicked.Raise(this, normalizedPosition);
                }

                return true;
            }

            return false;
        }

        private bool OnMinimapMouseMoved(Control sender, MouseState mouseState)
        {
            if (sender.HasMouseCapture)
            {
                MoveMinimapCamera(sender.Rectangle, mouseState.Position);
                return true;
            }

            return false;
        }

        private void MoveMinimapCamera(Region rectangle, Point position)
        {
            Vector2 normalizedPosition = rectangle.Normalize(rectangle.Clamp(position));
            MinimapCameraMoved.Raise(this, normalizedPosition);
        }

        private void OnActionButtonClicked(Button sender)
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
    }
}
