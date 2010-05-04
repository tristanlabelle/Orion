using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Geometry;
using Orion.Engine.Input;

namespace Orion.Engine.Gui
{
    public class Checkbox : RenderedView
    {
        #region Fields
        private bool isChecked;
        private bool isDown;
        private bool isEnabled = true;
        #endregion

        #region Constructors
        public Checkbox(Rectangle frame)
            : this(frame, false)
        { }

        public Checkbox(Rectangle frame, bool state)
            : this(frame, state, new FilledRenderer())
        { }

        public Checkbox(Rectangle frame, bool state, IViewRenderer renderer)
            : base(frame, renderer)
        {
            this.isChecked = state;
            Renderer = new CheckboxRenderer(this, Renderer);
        }
        #endregion

        #region Events
        public event Action<Checkbox, bool> StateChanged;
        #endregion

        #region Properties
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    OnStateChanged();
                }
            }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }
        #endregion

        #region Methods
        public void Toggle()
        {
            IsChecked ^= isEnabled;
        }

        private void OnStateChanged()
        {
            var handler = StateChanged;
            if (handler != null) handler(this, isChecked);
        }

        protected override bool OnMouseButtonPressed(MouseEventArgs args)
        {
            isDown = true;
            base.OnMouseButtonPressed(args);
            return false;
        }

        protected override bool OnMouseButtonReleased(MouseEventArgs args)
        {
            if (isDown)
            {
                Toggle();
                isDown = false;
            }
            base.OnMouseButtonReleased(args);
            return false;
        }
        #endregion
    }
}
