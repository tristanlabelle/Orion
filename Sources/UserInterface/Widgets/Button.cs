using System;
using System.Diagnostics;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Graphics;
using Orion.Graphics.Renderers;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Widgets
{
    /// <summary>
    /// Provides a UI button which can be listened for clicks.
    /// </summary>
    [DebuggerDisplay("{Caption} button")]
    public class Button : RenderedView
    {
        #region Fields
        private readonly Label caption;
        private Keys hotKey;
        private bool isEnabled = true;
        private bool isDown;
        private ColorRgba captionUpColor = Colors.White;
        private ColorRgba captionDownColor = Colors.Orange;
        private ColorRgba captionOverColor = Colors.Cyan;
        #endregion

        #region Constructors
        public Button(Rectangle frame)
            : this(frame, string.Empty, new FilledFrameRenderer())
        { }

        public Button(Rectangle frame, string caption)
            : this(frame, caption, new FilledFrameRenderer())
        { }

        public Button(Rectangle frame, string caption, IRenderer renderer)
            : base(frame, renderer)
        {
            this.caption = new Label(caption);
            this.caption.Color = Colors.White;
            AlignCaption();
            Children.Add(this.caption);
        }
        #endregion

        #region Events
        /// <summary>
        /// Triggered when the button is pressed or when its <see cref="P:HotKey"/> is pressed.
        /// </summary>
        public event Action<Button> Triggered;
        #endregion

        #region Properties
        public string Caption
        {
            get { return caption.Text.Value; }
            set
            {
                caption.Text = new Text(value);
                AlignCaption();
            }
        }

        public ColorRgba CaptionUpColor
        {
            get { return captionUpColor; }
            set
            {
                captionUpColor = value;
                caption.Color = value;
            }
        }

        public ColorRgba CaptionDownColor
        {
            get { return captionDownColor; }
            set { captionDownColor = value; }
        }

        public ColorRgba CaptionOverColor
        {
            get { return captionOverColor; }
            set { captionOverColor = value; }
        }

        public Keys HotKey
        {
            get { return hotKey; }
            set { hotKey = value; }
        }

        public bool Enabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }
        #endregion

        #region Methods
        protected override void Dispose(bool disposing)
        {
            if (disposing) Triggered = null;
            base.Dispose(disposing);
        }

        protected override bool OnMouseEntered(MouseEventArgs args)
        {
            if (isEnabled) caption.Color = captionOverColor;
            return base.OnMouseEntered(args);
        }

        protected override bool OnMouseExited(MouseEventArgs args)
        {
            if (isEnabled)
            {
                caption.Color = captionUpColor;
                isDown = false;
            }

            return base.OnMouseExited(args);
        }

        protected override bool OnMouseButtonPressed(MouseEventArgs args)
        {
            if (isEnabled)
            {
                caption.Color = captionDownColor;
                isDown = true;
            }

            base.OnMouseButtonPressed(args);
            return false;
        }

        protected override bool OnMouseButtonReleased(MouseEventArgs args)
        {
            bool isClicked = isEnabled && isDown;

            base.OnMouseButtonReleased(args);

            if (isClicked)
            {
                caption.Color = captionOverColor;
                isDown = false;
                OnPress();
                Debug.Assert(!IsDisposed, "A button was disposed while executing its Triggered handler.");
            }

            return false;
        }

        protected override bool OnDoubleClick(MouseEventArgs args)
        {
            base.OnDoubleClick(args);
            return false;
        }

        protected override bool OnKeyboardButtonPressed(KeyboardEventArgs args)
        {
            if (args.Key == hotKey)
            {
                OnPress();
                base.OnKeyboardButtonPressed(args);
                return false;
            }

            return base.OnKeyboardButtonPressed(args);
        }

        protected virtual void OnPress()
        {
            if (isEnabled) Triggered.Raise(this);
        }

        private void AlignCaption()
        {
            Rectangle textFrame = caption.Text.Frame;
            Vector2 captionOrigin = Bounds.Center - textFrame.Center;
            caption.Frame = caption.Frame.TranslatedTo(captionOrigin);
        }
        #endregion
    }
}
