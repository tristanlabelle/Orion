﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Game.Presentation.Renderers;
using Orion.Engine;
using Orion.Engine.Gui2.Adornments;
using System.Globalization;

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
        private Control bottomBar;
        private ContentControl selectionInfoPanel;

        private Point scrollDirection;
        #endregion

        #region Constructors
        public MatchUI2(OrionGuiStyle style)
        {
            Argument.EnsureNotNull(style, "style");

            updatedEventHandler = OnGuiUpdated;

            DockPanel mainDockPanel = style.Create<DockPanel>();
            mainDockPanel.Dock(CreateTopBar(style), Direction.MaxY);
            mainDockPanel.Dock(CreateBottomBar(style), Direction.MinY);

            Content = mainDockPanel;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the minimap should be rendered.
        /// </summary>
        public event Action<MatchUI2, Region> MinimapRendering;
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
            if (Manager != null) Manager.Updated += updatedEventHandler;
        }

        private void OnGuiUpdated(UIManager sender, TimeSpan elapsedTime)
        {
            Region rectangle = Rectangle;
            Point mousePosition = sender.MouseState.Position;

            int scrollX = 0;
            if (mousePosition.X == 0) scrollX = -1;
            else if (mousePosition.X == rectangle.ExclusiveMaxX - 1) scrollX = 1;

            int scrollY = 0;
            if (mousePosition.Y == 0) scrollY = -1;
            else if (mousePosition.Y == rectangle.ExclusiveMaxY - 1) scrollY = 1;

            scrollDirection = new Point(scrollX, scrollY);
        }

        private Control CreateTopBar(OrionGuiStyle style)
        {
            ContentControl container = new ContentControl();
            container.IsMouseEventSink = true;
            container.HorizontalAlignment = Alignment.Center;
            container.Width = 500;
            BorderTextureAdornment adornment = new BorderTextureAdornment(style.GetTexture("Gui/Header"));
            container.Adornment = adornment;
            container.Padding = new Borders(adornment.Texture.Width / 2 - 10, 8);

            DockPanel dockPanel = style.Create<DockPanel>();
            container.Content = dockPanel;

            ImageBox aladdiumImageBox = new ImageBox();
            dockPanel.Dock(aladdiumImageBox, Direction.MinX);
            aladdiumImageBox.Texture = style.GetTexture("Aladdium");
            aladdiumImageBox.VerticalAlignment = Alignment.Center;
            aladdiumImageBox.SetSize(30, 30);

            aladdiumAmountLabel = style.CreateLabel("0");
            dockPanel.Dock(aladdiumAmountLabel, Direction.MinX);
            aladdiumAmountLabel.VerticalAlignment = Alignment.Center;
            aladdiumAmountLabel.MinXMargin = 5;
            aladdiumAmountLabel.Color = Colors.White;

            ImageBox alageneImageBox = new ImageBox();
            dockPanel.Dock(alageneImageBox, Direction.MinX);
            alageneImageBox.Texture = style.GetTexture("Alagene");
            alageneImageBox.VerticalAlignment = Alignment.Center;
            alageneImageBox.SetSize(30, 30);
            alageneImageBox.MinXMargin = 20;

            alageneAmountLabel = style.CreateLabel("0");
            dockPanel.Dock(alageneAmountLabel, Direction.MinX);
            alageneAmountLabel.VerticalAlignment = Alignment.Center;
            alageneAmountLabel.MinXMargin = 5;
            alageneAmountLabel.Color = Colors.White;

            Button pauseButton = style.CreateTextButton("Pause");
            dockPanel.Dock(pauseButton, Direction.MaxX);
            pauseButton.VerticalAlignment = Alignment.Center;

            Button diplomacyButton = style.CreateTextButton("Diplomatie");
            dockPanel.Dock(diplomacyButton, Direction.MaxX);
            diplomacyButton.VerticalAlignment = Alignment.Center;
            diplomacyButton.MaxXMargin = 10;

            return container;
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

            Control actionPanel = style.CreateLabel("Placeholder");
            dockPanel.Dock(actionPanel, Direction.MaxX);
            actionPanel.SetSize(200, 200);

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

        private Control CreateMinimapViewport()
        {
            ViewportBox minimapBox = new ViewportBox();
            minimapBox.Rendering += sender => MinimapRendering.Raise(this, sender.Rectangle);
            return minimapBox;
        }
        #endregion
    }
}
