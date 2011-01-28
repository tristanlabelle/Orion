using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui2;
using Orion.Engine.Gui2.Adornments;

namespace Orion.Game.Presentation.Gui
{
	/// <summary>
	/// Provides a user interface which enables the user to select a replay to watch.
	/// </summary>
    public sealed class ReplayBrowser2 : ContentControl
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly ListBox replayListBox;
        #endregion

        #region Constructors
        public ReplayBrowser2(GameGraphics graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            this.graphics = graphics;

            Adornment = new TextureAdornment(graphics.GetGuiTexture("Granite")) { IsTiling = true };
            Padding = 5;

            DockLayout dock = new DockLayout()
            {
                LastChildFill = true
            };

            DockLayout buttonDock = new DockLayout();
            
            Button backButton = graphics.GuiStyle.CreateTextButton("Retour");
            backButton.Clicked += (sender, @event) => Exited.Raise(this);
            buttonDock.Dock(backButton, Direction.NegativeX);

            Button viewButton = graphics.GuiStyle.CreateTextButton("Visionner");
            viewButton.HasEnabledFlag = false;
            viewButton.Clicked += (sender, @event) => Started.Raise(this, ((Label)replayListBox.SelectedItem).Text);
            buttonDock.Dock(viewButton, Direction.PositiveX);

            dock.Dock(buttonDock, Direction.PositiveY);

            replayListBox = graphics.GuiStyle.Create<ListBox>();
            replayListBox.Adornment = new ColorAdornment(Colors.Black.ToRgba(0.2f));
            replayListBox.MaxYMargin = 10;
            replayListBox.Padding = 5;
            replayListBox.SelectionChanged += sender => viewButton.HasEnabledFlag = sender.SelectedItem != null;
            dock.Dock(replayListBox, Direction.NegativeY);

            Content = dock;
        }
        #endregion
        
        #region Events
        /// <summary>
        /// Raised when the user exits the screen.
        /// </summary>
        public event Action<ReplayBrowser2> Exited;
        
        /// <summary>
        /// Raised when the user starts a replay.
        /// </summary>
        public event Action<ReplayBrowser2, string> Started;
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
        #endregion
    }
}
