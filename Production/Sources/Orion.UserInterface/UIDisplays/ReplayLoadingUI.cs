using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.UserInterface.Widgets;
using Orion.Geometry;
using OpenTK.Math;

namespace Orion.UserInterface
{
    public class ReplayLoadingUI : UIDisplay
    {
        #region Fields
        private readonly ListFrame replaysList;
        #endregion

        #region Constructors
        public ReplayLoadingUI()
        {
            Rectangle replaysRectangle = Bounds.TranslatedBy(10, 60).ResizedBy(-60, -70);
            replaysList = new ListFrame(replaysRectangle, new Rectangle(replaysRectangle.Width - 20, 30), new Vector2(10, 10));
            Rectangle scrollbarRectangle = new Rectangle(replaysRectangle.MaxX, replaysRectangle.MinY, 30, replaysRectangle.Height);
            Scrollbar replaysScrollbar = new Scrollbar(scrollbarRectangle, replaysList);

            Button exitButton = new Button(new Rectangle(10, 10, 100, 40), "Go Back");

            exitButton.Triggered += PressedExitButton;

            Children.Add(exitButton);
            Children.Add(replaysList);
            Children.Add(replaysScrollbar);
        }
        #endregion

        #region Events
        public event GenericEventHandler<ReplayLoadingUI> StartReplay;
        #endregion

        #region Methods

        private void PressedExitButton(Button sender)
        {
            Parent.PopDisplay(this);
        }

        #endregion
    }
}
