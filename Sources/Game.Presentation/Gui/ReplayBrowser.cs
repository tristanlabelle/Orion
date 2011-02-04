using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Engine.Gui.Adornments;

namespace Orion.Game.Presentation.Gui
{
	/// <summary>
	/// Provides a user interface which enables the user to select a replay to watch.
	/// </summary>
    public sealed class ReplayBrowser : ContentControl
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly ListBox replayListBox;
        #endregion

        #region Constructors
        public ReplayBrowser(GameGraphics graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;

            Adornment = new TextureAdornment(graphics.GetGuiTexture("Granite")) { IsTiling = true };
            Padding = 5;

            DockLayout dock = new DockLayout()
            {
                LastChildFill = true
            };

            dock.Dock(graphics.GuiStyle.CreateLabel("Parties enregistrées:"), Direction.NegativeY);

            DockLayout buttonDock = new DockLayout();
            
            Button backButton = graphics.GuiStyle.CreateTextButton("Retour");
            backButton.MinSize = new Size(150, 40);
            backButton.Clicked += (sender, @event) => Exited.Raise(this);
            buttonDock.Dock(backButton, Direction.NegativeX);

            Button viewButton = graphics.GuiStyle.CreateTextButton("Visionner");
            viewButton.MinSize = new Size(150, 40);
            viewButton.HasEnabledFlag = false;
            viewButton.Clicked += (sender, @event) => TryStartSelectedReplay();
            buttonDock.Dock(viewButton, Direction.PositiveX);

            dock.Dock(buttonDock, Direction.PositiveY);

            replayListBox = graphics.GuiStyle.Create<ListBox>();
            replayListBox.Adornment = new ColorAdornment(Colors.Black.ToRgba(0.2f));
            replayListBox.MaxYMargin = 10;
            replayListBox.Padding = 5;
            replayListBox.SelectionChanged += sender => viewButton.HasEnabledFlag = sender.SelectedItem != null;
            replayListBox.MouseButton += OnReplayListBoxMouseButton;
            dock.Dock(replayListBox, Direction.NegativeY);

            Content = dock;
        }
        #endregion
        
        #region Events
        /// <summary>
        /// Raised when the user exits the screen.
        /// </summary>
        public event Action<ReplayBrowser> Exited;
        
        /// <summary>
        /// Raised when the user starts a replay.
        /// </summary>
        public event Action<ReplayBrowser, string> Started;
        #endregion

        #region Methods
        /// <summary>
        /// Adds a replay to the list of displayed replays.
        /// </summary>
        /// <param name="name">The name of the replay to be added.</param>
        public void AddReplay(string name)
        {
        	Argument.EnsureNotNull(name, "name");
        	
        	Label label = graphics.GuiStyle.CreateLabel(name);
        	replayListBox.AddItem(label);
        }

        private bool OnReplayListBoxMouseButton(Control sender, MouseEvent @event)
        {
            if (@event.IsPressed && @event.ClickCount > 1)
            {
                TryStartSelectedReplay();
                return true;
            }

            return false;
        }

        private void TryStartSelectedReplay()
        {
            Label selectedReplayLabel = (Label)replayListBox.SelectedItem;
            if (selectedReplayLabel != null) Started.Raise(this, selectedReplayLabel.Text);
        }
        #endregion
    }
}
