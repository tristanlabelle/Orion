﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A <see cref="Control"/> which can be clicked and dragged by the user to adjust something.
    /// </summary>
    public sealed class Thumb : Control
    {
        #region Fields
        private Point? dragStartPosition;
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Thumb"/> starts being dragged by the user.
        /// </summary>
        public event Action<Thumb> DragStarted;

        /// <summary>
        /// Raised when this <see cref="Thumb"/> is dragged by the user.
        /// </summary>
        public event Action<Thumb, Point> Dragging;

        /// <summary>
        /// Raised when this <see cref="Thumb"/> stops being dragged by the user.
        /// </summary>
        public event Action<Thumb> DragEnded;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if this <see cref="Thumb"/> is currently being dragged.
        /// </summary>
        [PropertyChangedEvent("DragStarted")]
        [PropertyChangedEvent("DragEnded")]
        public bool IsDragged
        {
            get { return dragStartPosition.HasValue; }
        }
        #endregion

        #region Methods
        protected override Size MeasureSize()
        {
            return MinSize;
        }

        protected internal override bool OnMouseButton(MouseState state, MouseButtons button, int pressCount)
        {
            if (button == MouseButtons.Left)
            {
                if (pressCount > 0)
                {
                    dragStartPosition = state.Position;
                    AcquireMouseCapture();
                    DragStarted.Raise(this);
                }
                else if (IsDragged)
                {
                    ReleaseMouseCapture();
                    dragStartPosition = null;
                    DragEnded.Raise(this);
                }

                return true;
            }

            return false;
        }

        protected internal override bool OnMouseMove(MouseState state)
        {
            if (IsDragged)
            {
                Dragging.Raise(this, dragStartPosition.Value - state.Position);

                return true;
            }

            return false;
        }
        #endregion
    }
}
