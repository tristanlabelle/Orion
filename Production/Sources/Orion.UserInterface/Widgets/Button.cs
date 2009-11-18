using Orion.Geometry;
using Orion.Graphics;
using OpenTK.Math;
using Color = System.Drawing.Color;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Widgets
{
    public class Button : RenderedView
    {
        #region Fields
        private Label caption;
        private Keys hotKey;
        private bool enabled;
        #endregion

        #region Constructors
        public Button(Rectangle frame)
            : this(frame, "", new FilledFrameRenderer())
        { }

        public Button(Rectangle frame, string caption)
            : this(frame, caption, new FilledFrameRenderer())
        { }

        public Button(Rectangle frame, string caption, FrameRenderer renderer)
            : base(frame, renderer)
        {
            this.caption = new Label(caption);
            this.caption.Color = Color.White;
            enabled = true;
            AlignCaption();
            Children.Add(this.caption);
        }
        #endregion

        #region Events
        /// <summary>
        /// Triggered when the button is pressed or when its <see cref="P:HotKey"/> is pressed.
        /// </summary>
        public event GenericEventHandler<Button> Pressed;
        #endregion

        #region Properties

        public Text Caption
        {
            get { return caption.Text; }
            set
            {
                caption.Text = value;
                AlignCaption();
            }
        }

        public Keys HotKey
        {
            get { return hotKey; }
            set { hotKey = value; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        #endregion

        #region Methods
        public override void Dispose()
        {
            Pressed = null;
            base.Dispose();
        }

        protected override bool OnMouseEnter(MouseEventArgs args)
        {
            if(enabled) caption.Color = Color.Cyan;
            return base.OnMouseEnter(args);
        }

        protected override bool OnMouseExit(MouseEventArgs args)
        {
            if (enabled) caption.Color = Color.White;
            return base.OnMouseExit(args);
        }

        protected override bool OnMouseDown(MouseEventArgs args)
        {
            if (enabled) caption.Color = Color.Orange;
            base.OnMouseDown(args);
            return false;
        }

        protected override bool OnMouseUp(MouseEventArgs args)
        {
            if (enabled) caption.Color = Color.Cyan;
            OnPress();
            base.OnMouseUp(args);
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
            if (enabled)
            {
                GenericEventHandler<Button> handler = Pressed;
                if (handler != null) handler(this);
            }
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
