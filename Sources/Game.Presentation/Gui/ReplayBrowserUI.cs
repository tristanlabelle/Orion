using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// Provides a user interface which allows to browse the saved replays.
    /// </summary>
    public sealed class ReplayBrowserUI : MaximizedPanel
    {
        #region Fields
        private readonly ListPanel replaysList;
        private readonly Rectangle replayButtonFrame;
        #endregion

        #region Constructors
        public ReplayBrowserUI()
        {
            Rectangle replaysRectangle = Bounds.TranslatedBy(10, 60).ResizedBy(-60, -70);
            replaysList = new ListPanel(replaysRectangle, new Vector2(10, 10));
            Rectangle scrollbarRectangle = new Rectangle(replaysRectangle.MaxX, replaysRectangle.MinY, 30, replaysRectangle.Height);
            Scrollbar replaysScrollbar = new Scrollbar(scrollbarRectangle, replaysList);
            replayButtonFrame = new Rectangle(replaysRectangle.Width - 20, 30);

            Button exitButton = new Button(new Rectangle(10, 10, 100, 40), "Retour");
            exitButton.Triggered += (sender) => ExitPressed.Raise(this);

            Children.Add(exitButton);
            Children.Add(replaysList);
            Children.Add(replaysScrollbar);

            ShowReplayFiles();
        }
        #endregion

        #region Events
        public event Action<ReplayBrowserUI, string> StartPressed;
        public event Action<ReplayBrowserUI> ExitPressed;
        #endregion

        #region Methods
        private void ShowReplayFiles()
        {
            DirectoryInfo replays = new DirectoryInfo("Replays");
            if (replays.Exists)
            {
                foreach (FileInfo file in replays.GetFiles("*.replay").OrderBy(file => file.Name))
                {
                    Button replayButton = new Button(replayButtonFrame, file.Name);
                    string filePathForClosure = file.FullName;
                    replayButton.Triggered += (sender) => StartPressed.Raise(this, filePathForClosure);
                    replaysList.Children.Add(replayButton);
                }
            }
        }
        #endregion
    }
}
