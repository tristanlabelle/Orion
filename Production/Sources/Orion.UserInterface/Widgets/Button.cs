using Orion.Geometry;
using Orion.Graphics;
using Color = System.Drawing.Color;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface.Widgets
{
    public class Button : RenderedView
    {
        #region Fields
        private Label caption;
        private Keys hotKey;
        #endregion

        #region Constructors
        public Button(Rectangle frame, string caption)
            : this(frame, caption, new FilledFrameRenderer())
        { }

        public Button(Rectangle frame, string caption, FrameRenderer renderer)
            : base(frame, renderer)
        {
            this.caption = new Label(Bounds.Translate(2, 2).Resize(-2, -2), caption);
            this.caption.Color = Color.White;
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

        public string Caption
        {
            get { return caption.Text; }
            set { caption.Text = value; }
        }

        public Keys HotKey
        {
            get { return hotKey; }
            set { hotKey = value; }
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
            caption.Color = Color.Cyan;
            return base.OnMouseEnter(args);
        }

        protected override bool OnMouseExit(MouseEventArgs args)
        {
            caption.Color = Color.White;
            return base.OnMouseExit(args);
        }

        protected override bool OnMouseDown(MouseEventArgs args)
        {
            caption.Color = Color.Orange;
            base.OnMouseDown(args);
            return false;
        }

        protected override bool OnMouseUp(MouseEventArgs args)
        {
            caption.Color = Color.Cyan;
            Click();
            base.OnMouseUp(args);
            return false;
        }

        protected override bool OnKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == hotKey)
            {
                Click();
                base.OnKeyDown(args);
                return false;
            }
            return base.OnKeyDown(args);
        }

        private void Click()
        {
            GenericEventHandler<Button> handler = Pressed;
            if (handler != null) handler(this);
        }
        #endregion
    }
}
