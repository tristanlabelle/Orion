using OpenTK.Math;
using Orion.Geometry;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Widgets
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
        protected override bool OnMouseEnter(MouseEventArgs args)
        {
            mouseTriggered = true;
            timeHoveredInSeconds = 0;
            return base.OnMouseEnter(args);
        }

        protected override bool OnMouseExit(MouseEventArgs args)
        {
            mouseTriggered = false;
            timeHoveredInSeconds = 0;
            return base.OnMouseExit(args);
        }

        protected override bool OnKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == keyboardTrigger)
                keyboardTriggered = true;
            
            return base.OnKeyDown(args);
        }

        protected override bool OnKeyUp(KeyboardEventArgs args)
        {
            if (args.Key == keyboardTrigger)
                keyboardTriggered = false;

            return base.OnKeyUp(args);
        }

        protected override void OnUpdate(UpdateEventArgs args)
        {
            if (mouseTriggered) timeHoveredInSeconds += args.TimeDeltaInSeconds;
            bool shouldScroll = isEnabled && (keyboardTriggered || (mouseTriggered && timeHoveredInSeconds >= ScrollDelayInSeconds));
            if (shouldScroll)
            {
                Vector2 scrollFactor = new Vector2(direction.X * scrolledView.Bounds.Width, direction.Y * scrolledView.Bounds.Height);
                scrolledView.ScrollBy(scrollFactor * args.TimeDeltaInSeconds * ScrollSpeed);
            }
        }
        #endregion
    }
}