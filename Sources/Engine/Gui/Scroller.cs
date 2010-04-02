using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;
using Orion.Engine.Input;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Engine.Gui
{
    public class Scroller : Responder
    {
        #region Fields
        private static readonly float ScrollDelayInSeconds = 0;
        private static readonly float ScrollSpeed = 40;

        private readonly ClippedView scrolledView;
        private readonly Vector2 direction;
        private readonly Keys keyboardTrigger;

        private bool isEnabled = true;
        private bool mouseTriggered;
        private bool keyboardTriggered;
        private float timeHoveredInSeconds;
        #endregion

        #region Constructors
        public Scroller(ClippedView view, Rectangle frame, Vector2 direction)
            : this(view, frame, direction, Keys.None)
        { }

        public Scroller(ClippedView view, Rectangle frame, Vector2 direction, Keys keyboardTrigger)
        {
            this.Frame = frame;
            this.Bounds = frame;
            this.scrolledView = view;
            this.direction = direction;
            this.keyboardTrigger = keyboardTrigger;
        }
        #endregion

        #region Properties
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }
        #endregion

        #region Methods
        protected override bool OnMouseEntered(MouseEventArgs args)
        {
            mouseTriggered = true;
            timeHoveredInSeconds = 0;
            return base.OnMouseEntered(args);
        }

        protected override bool OnMouseExited(MouseEventArgs args)
        {
            mouseTriggered = false;
            timeHoveredInSeconds = 0;
            return base.OnMouseExited(args);
        }

        protected override bool OnKeyboardButtonPressed(KeyboardEventArgs args)
        {
            if (args.Key == keyboardTrigger)
                keyboardTriggered = true;
            
            return base.OnKeyboardButtonPressed(args);
        }

        protected override bool OnKeyboardButtonReleased(KeyboardEventArgs args)
        {
            if (args.Key == keyboardTrigger)
                keyboardTriggered = false;

            return base.OnKeyboardButtonReleased(args);
        }

        protected override void Update(float timeDeltaInSeconds)
        {
            if (mouseTriggered) timeHoveredInSeconds += timeDeltaInSeconds;
            bool shouldScroll = isEnabled && (keyboardTriggered || (mouseTriggered && timeHoveredInSeconds >= ScrollDelayInSeconds));
            if (shouldScroll)
            {
                Vector2 scrollFactor = new Vector2(direction.X * scrolledView.Bounds.Width, direction.Y * scrolledView.Bounds.Height);
                scrolledView.ScrollBy(scrollFactor * timeDeltaInSeconds * ScrollSpeed);
            }
        }
        #endregion
    }
}