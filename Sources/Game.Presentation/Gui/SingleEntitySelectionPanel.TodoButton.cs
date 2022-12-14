using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Presentation.Gui
{
    partial class SingleEntitySelectionPanel
    {
        private sealed class TodoButton : Button
        {
            #region Fields
            private const int progressBarHeight = 4;
            private const int progressBarMargin = 4;

            private readonly SingleEntitySelectionPanel panel;
            private readonly ImageBox imageBox;
            private Task task;
            #endregion

            #region Constructors
            public TodoButton(SingleEntitySelectionPanel panel)
            {
                this.panel = panel;
                panel.Style.ApplyStyle(this);
                SetSize(32, 32);

                AcquireKeyboardFocusWhenPressed = false;
                Content = imageBox = new ImageBox();
                PostDrawing += OnPostDrawing;
            }
            #endregion

            #region Properties
            /// <summary>
            /// Accesses the <see cref="Task"/> displayed on this button.
            /// </summary>
            public Task Task
            {
                get { return task; }
                set
                {
                    task = value;

                    if (task == null)
                        imageBox.Texture = null;
                    else if (task is TrainTask)
                        imageBox.Texture = panel.graphics.GetEntityTexture(((TrainTask)task).Prototype);
                    else if (task is RepairTask)
                        imageBox.Texture = panel.graphics.GetEntityTexture(((RepairTask)task).Target);
                    else if (task is ResearchTask)
                        imageBox.Texture = panel.graphics.GetTechnologyTexture(((ResearchTask)task).Technology);
                    else
                        imageBox.Texture = panel.graphics.GetActionTexture(task);
                }
            }
            #endregion

            #region Methods
            protected override void OnClicked(ButtonClickEvent @event)
            {
                Debug.Assert(task != null);
                panel.TaskCancelled.Raise(panel, task);
            }

            private static void OnPostDrawing(Control sender, GuiRenderer renderer)
            {
                TodoButton button = (TodoButton)sender;

                Task task = button.Task;
                if (task == null) return;

                float progress = task.Progress;
                if (float.IsNaN(progress)) progress = 0;
                progress = progress.Clamp(0, 1);

                Region buttonRectangle = button.Rectangle;
                Region progressBarRectangle = new Region(
                    buttonRectangle.MinX + progressBarMargin, buttonRectangle.ExclusiveMaxY - progressBarHeight - progressBarMargin,
                    (int)((buttonRectangle.Width - progressBarMargin * 2) * progress), progressBarHeight);
                if (progressBarRectangle.Area == 0) return;

                renderer.DrawRectangle(progressBarRectangle, Colors.Green);
            }
            #endregion
        }
    }
}
