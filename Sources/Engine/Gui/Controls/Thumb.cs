using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// A <see cref="Control"/> which can be clicked and dragged by the user to adjust something.
    /// </summary>
    public sealed class Thumb : Control
    {
        #region Fields
        private Point? lastDragPosition;
        #endregion

        #region Events
        /// <summary>
        /// Raised when this <see cref="Thumb"/> starts or stops being dragged by the user.
        /// </summary>
        public event Action<Thumb> DragStateChanged;

        /// <summary>
        /// Raised when this <see cref="Thumb"/> is dragged by the user.
        /// </summary>
        public event Action<Thumb, Point> Dragging;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating if this <see cref="Thumb"/> is currently being dragged.
        /// </summary>
        [PropertyChangedEvent("DragStateChanged")]
        public bool IsDragged
        {
            get { return lastDragPosition.HasValue; }
        }
        #endregion

        #region Methods
        protected override Size MeasureSize(Size availableSize)
        {
            return MinSize;
        }

        protected override void ArrangeChildren() { }

        protected override bool OnMouseButton(MouseEvent @event)
        {
            if (@event.Button == MouseButtons.Left)
            {
                if (@event.IsPressed)
                {
                    lastDragPosition = @event.Position;
                    AcquireMouseCapture();
                    DragStateChanged.Raise(this);
                }
                else if (IsDragged)
                {
                    ReleaseMouseCapture();
                    lastDragPosition = null;
                    DragStateChanged.Raise(this);
                }

                return true;
            }

            return false;
        }

        protected override bool OnMouseMoved(MouseEvent @event)
        {
            if (IsDragged)
            {
                Point delta = @event.Position - lastDragPosition.Value;
                lastDragPosition = @event.Position;
                Dragging.Raise(this, delta);

                return true;
            }

            return false;
        }
        #endregion
    }
}
