using System;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;
using Orion.Graphics.Renderers;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Widgets
{
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

        private void RaiseTriggered()
        {
            var handler = Triggered;
            if (handler != null) handler(this);
        }
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

        protected override bool OnMouseEnter(MouseEventArgs args)
        {
            if (isEnabled) caption.Color = captionOverColor;
            return base.OnMouseEnter(args);
        }

        protected override bool OnMouseExit(MouseEventArgs args)
        {
            if (isEnabled)
            {
                caption.Color = captionUpColor;
                isDown = false;
            }

            return base.OnMouseExit(args);
        }

        protected override bool OnMouseDown(MouseEventArgs args)
        {
            if (isEnabled)
            {
                caption.Color = captionDownColor;
                isDown = true;
            }

            base.OnMouseDown(args);
            return false;
        }

        protected override bool OnMouseUp(MouseEventArgs args)
        {
            bool isClicked = isEnabled && isDown;

            base.OnMouseUp(args);

            if (isClicked)
            {
                caption.Color = captionOverColor;
                isDown = false;
                OnPress();
            }

            return false;
        }

        protected override bool OnDoubleClick(MouseEventArgs args)
        {
            base.OnDoubleClick(args);
            return false;
        }

        protected override bool OnKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == hotKey)
            {
                OnPress();
                base.OnKeyDown(args);
                return false;
            }
            return base.OnKeyDown(args);
        }

        protected virtual void OnPress()
        {
            if (isEnabled) RaiseTriggered();
        }

        private void AlignCaption()
        {
            Rectangle textFrame = caption.Text.Frame;
            Vector2 captionOrigin = Bounds.Center - textFrame.Center;
            caption.Frame = caption.Frame.TranslatedTo(captionOrigin);
        }

        #region Object Model
        public override string ToString()
        {
            return "Button \"{0}\"".FormatInvariant(caption.Text.Value);
        }
        #endregion
        #endregion
    }
}
