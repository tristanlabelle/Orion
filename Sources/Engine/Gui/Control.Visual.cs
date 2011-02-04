using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Orion.Engine.Gui
{
    // This class part defines members relating to the appearance of controls.
    partial class Control
    {
        #region Fields
        private IAdornment adornment;
        private Visibility visibilityFlag = Visibility.Visible;
        #endregion

        #region Events
        /// <summary>
        /// Raised right before this <see cref="Control"/> draws itself.
        /// </summary>
        public event Action<Control, GuiRenderer> PreDrawing;

        internal void RaisePreDrawing()
        {
            PreDrawing.Raise(this, Renderer);
        }

        /// <summary>
        /// Raised right after this <see cref="Control"/> draws itself.
        /// </summary>
        public event Action<Control, GuiRenderer> PostDrawing;

        internal void RaisePostDrawing()
        {
            PostDrawing.Raise(this, Renderer);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the <see cref="IAdornment"/> which visually enhances this control.
        /// </summary>
        public IAdornment Adornment
        {
            get { return adornment; }
            set { adornment = value; }
        }

        /// <summary>
        /// Accesses the current visibility flag of this <see cref="Control"/>.
        /// The actual visibility depends on the flags of this <see cref="Control"/> and its ancestors
        /// </summary>
        public Visibility VisibilityFlag
        {
            get { return visibilityFlag; }
            set
            {
                if (value == visibilityFlag) return;

                Visibility previousVisibilityFlag = visibilityFlag;
                visibilityFlag = value;

                if (visibilityFlag != Visibility.Visible)
                {
                    if (manager != null && HasDescendant(manager.ControlUnderMouse))
                        manager.ControlUnderMouse = Parent;

                    ReleaseKeyboardFocus();
                    ReleaseMouseCapture();
                }

                if (visibilityFlag == Visibility.Collapsed || previousVisibilityFlag == Visibility.Collapsed)
                    InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets the actual visibility of this <see cref="Control"/>,
        /// based on this <see cref="Control"/>'s and its parent's <see cref="VisibilityFlag"/>.
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                Visibility visibility = Visibility.Visible;

                Control ancestor = this;
                do
                {
                    if (ancestor.VisibilityFlag < visibility)
                    {
                        visibility = ancestor.VisibilityFlag;
                        if (visibility == Visibility.Collapsed) break;
                    }

                    ancestor = ancestor.Parent;
                } while (ancestor != null);

                return visibility;
            }
        }

        /// <summary>
        /// Gets the <see cref="GuiRenderer"/> which draws this <see cref="Control"/>.
        /// </summary>
        protected GuiRenderer Renderer
        {
            get
            {
                Debug.Assert(manager != null);
                return manager.Renderer;
            }
        }
        #endregion

        #region Methods
        protected internal virtual void Draw() { }
        #endregion
    }
}
