using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Orion.UserInterface.Widgets;
using Orion.Engine.Geometry;
using OpenTK.Math;

namespace Orion.UserInterface
{
    public class ReplayLoadingUI : UIDisplay
    {
        #region Fields
        private readonly ListFrame replaysList;
        private readonly Rectangle replayButtonFrame;
        #endregion

        #region Constructors
        public ReplayLoadingUI()
        {
            Rectangle replaysRectangle = Bounds.TranslatedBy(10, 60).ResizedBy(-60, -70);
            replaysList = new ListFrame(replaysRectangle, new Vector2(10, 10));
            Rectangle scrollbarRectangle = new Rectangle(replaysRectangle.MaxX, replaysRectangle.MinY, 30, replaysRectangle.Height);
            Scrollbar replaysScrollbar = new Scrollbar(scrollbarRectangle, replaysList);
            replayButtonFrame = new Rectangle(replaysRectangle.Width - 20, 30);

            Button exitButton = new Button(new Rectangle(10, 10, 100, 40), "Retour");

            exitButton.Triggered += PressedExitButton;

            Children.Add(exitButton);
            Children.Add(replaysList);
            Children.Add(replaysScrollbar);

            ShowReplayFiles();
        }
        #endregion

        #region Events
        public event Action<ReplayLoadingUI, string> PressedStartReplay;
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
                    replayButton.Triggered += StartReplay;
                    replaysList.Children.Add(replayButton);
                }
            }
        }

        private void PressedExitButton(Button sender)
        {
            Parent.PopDisplay(this);
        }

        private void StartReplay(Button sender)
        {
            if (PressedStartReplay != null)
                PressedStartReplay(this, sender.Caption);
        }

        #endregion
    }
}
