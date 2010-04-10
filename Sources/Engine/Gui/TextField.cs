using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Engine.Input;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Engine.Gui
{
    public class TextField : View
    {
        #region Field
        private const float caretBlinkFrequency = 0.5f;

        private string contents = string.Empty;
        private int caretIndex;
        private float time;
        #endregion

        #region Constructors
        public TextField(Rectangle frame)
            : base(frame)
        {}
        #endregion

        #region Events
        /// <summary>
        /// Triggered when the Return key is pressed.
        /// </summary>
        public event Action<TextField> Triggered;
        #endregion

        #region Properties
        public string Contents
        {
            get { return contents; }
            set
            {
                Argument.EnsureNotNull(value, "Contents");
                contents = value;
                caretIndex = contents.Length;
            }
        }
        #endregion

        #region Methods
        public void Clear()
        {
            Contents = string.Empty;
        }

        protected override bool OnKeyboardButtonPressed(KeyboardEventArgs args)
        {
            switch (args.Key)
            {
                case Keys.Left:
                    if (caretIndex > 0) --caretIndex;
                    break;
                
                case Keys.Right:
                    if (caretIndex < contents.Length) ++caretIndex;
                    break;

                case Keys.Home:
                case Keys.PageUp:
                    caretIndex = 0;
                    break;

                case Keys.End:
                case Keys.PageDown:
                    caretIndex = contents.Length;
                    break;
                    
                case Keys.Back:
                    if (caretIndex > 0)
                    {
                        contents = contents.RemoveAt(caretIndex - 1);
                        --caretIndex;
                    }
                    break;
                    
                case Keys.Delete:
                    if (caretIndex < contents.Length)
                        contents = contents.RemoveAt(caretIndex);
                    break;

                case Keys.Enter:
                    Triggered.Raise(this);
                    //Debug.Assert(!IsDisposed, "A text field was disposed while executing its Triggered handler.");
                    break;

                default:
                    break;
            }

            base.OnKeyboardButtonPressed(args);
            // Always return false so sibling controls don't get the event.
            return false;
        }

        protected override bool OnCharacterPressed(char character)
        {
            // Ignore \0, \n, \r, \b, etc.
            if (!char.IsControl(character))
            {
                contents = contents.Insert(caretIndex, character);
                ++caretIndex;
            }

            base.OnCharacterPressed(character);
            return false;
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            time += timeDeltaInSeconds;
            base.Update(timeDeltaInSeconds);
        }

        protected internal override void Draw(GraphicsContext context)
        {
            context.Fill(Bounds, Colors.LightGreen);
            context.Stroke(Bounds, Colors.Gray);

            if (contents.Length > 0)
            {
                Text text = new Text(contents + " ");
                Rectangle textBounds = new Rectangle(Bounds.Width, Math.Min(Bounds.Height, text.Frame.Height));
                context.Draw(text, textBounds, Colors.Black);
            }

            DrawCaret(context);
        }

        private void DrawCaret(GraphicsContext context)
        {
            if ((int)(time / caretBlinkFrequency) % 2 == 0)
            {
                Text textBeforeCaret = new Text(contents.Substring(0, caretIndex));
                Rectangle textFrame = textBeforeCaret.Frame;
                float lineX = textFrame.MaxX - (caretIndex == 0 ? 3 : 6); // Account for the offsetting OpenTK induces
                LineSegment lineSegment = new LineSegment(lineX, textFrame.MinY, lineX, textFrame.MaxY);
                context.Stroke(lineSegment, Colors.Black);
            }
        }

        protected internal override void OnAncestryChanged(ViewContainer ancestor)
        {
            Unfocus();
            base.OnAncestryChanged(ancestor);
        }

        protected internal override void OnAddToParent(ViewContainer parent)
        {
            Unfocus();
            base.OnAddToParent(parent);
        }

        protected internal override void OnRemovedFromParent(ViewContainer parent)
        {
            Unfocus();
            base.OnRemovedFromParent(parent);
        }

        private void Unfocus()
        {
            RootView root = Root as RootView;
            if (root != null && root.FocusedView == this)
                root.FocusedView = null;
        }
        #endregion
    }
}
