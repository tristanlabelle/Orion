using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine;
using Orion.Engine.Graphics;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Displays information on a single selected unit.
    /// </summary>
    public sealed class SingleUnitSelectionPanel : ContentControl
    {
        #region Fields
        private readonly OrionGuiStyle style;

        private Label nameLabel;
        private Label healthLabel;
        private ImageBox imageBox;
        #endregion

        #region Constructors
        public SingleUnitSelectionPanel(OrionGuiStyle style)
        {
            Argument.EnsureNotNull(style, "style");

            this.style = style;

            DockPanel mainDockPanel = new DockPanel();
            Content = mainDockPanel;
            mainDockPanel.LastChildFill = true;

            mainDockPanel.Dock(CreatePhoto(), Direction.MinX);
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return nameLabel.Text; }
            set { nameLabel.Text = value; }
        }

        public int Health
        {
            get { return int.Parse(healthLabel.Text.Split('/')[0]); }
            set { healthLabel.Text = value + "/" + TotalHealth; }
        }

        public int TotalHealth
        {
            get { return int.Parse(healthLabel.Text.Split('/')[1]); }
            set { healthLabel.Text = Health + "/" + value; }
        }

        public Texture Texture
        {
            get { return imageBox.Texture; }
            set { imageBox.Texture = value; }
        }
        #endregion

        #region Methods
        private Control CreatePhoto()
        {
            DockPanel photoDockPanel = new DockPanel();
            photoDockPanel.LastChildFill = true;
            photoDockPanel.MaxXMargin = 10;

            nameLabel = style.Create<Label>();
            photoDockPanel.Dock(nameLabel, Direction.MaxY);
            nameLabel.HorizontalAlignment = Alignment.Center;

            healthLabel = style.Create<Label>();
            photoDockPanel.Dock(healthLabel, Direction.MinY);
            healthLabel.HorizontalAlignment = Alignment.Center;

            imageBox = new ImageBox();
            photoDockPanel.Dock(imageBox, Direction.MinY);
            imageBox.HorizontalAlignment = Alignment.Center;

            return photoDockPanel;
        }
        #endregion
    }
}
