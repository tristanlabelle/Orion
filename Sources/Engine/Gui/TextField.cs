﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Engine.Gui
{
    public class TextField : View
    {
        #region Field
        private const int cursorBlinkFrequency = 30;

        private Text contents;
        private int updateCounter;
        #endregion

        #region Constructors
        public TextField(Rectangle frame)
            : base(frame)
        {
            contents = new Text(string.Empty);
        }
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
            get { return contents.Value; }
            set { contents = new Text(value); }
        }
        #endregion

        #region Methods
        public void Clear()
        {
            contents = new Text(string.Empty);
        }

        protected override bool OnCharacterPressed(char arg)
        {
            if (arg == '\b')
            {
                string value = Contents;
                if (value.Length > 0)
                    Contents = value.Remove(value.Length - 1);
            }
            else if (arg == '\r')
            {
                Triggered.Raise(this);
                Debug.Assert(!IsDisposed, "A text field was disposed while executing its Triggered handler.");
            }
            else Contents += arg;

            base.OnCharacterPressed(arg);
            return false;
        }

        protected override void Update(float timeDeltaInSeconds)
        {
            updateCounter++;
            base.Update(timeDeltaInSeconds);
        }

        protected internal override void Draw(GraphicsContext context)
        {
            context.Fill(Bounds, Colors.LightGreen);
            context.Stroke(Bounds, Colors.Gray);

            Text text;
            if (contents.Value.Length == 1) text = new Text(contents.Value + " ");
            else text = contents;
            Rectangle textBounds = new Rectangle(Bounds.Width, Math.Min(Bounds.Height, text.Frame.Height));
            context.Draw(text, textBounds, Colors.Black);
            if ((updateCounter / cursorBlinkFrequency) % 2 == 0)
            {
                Rectangle textFrame = contents.Frame;
                LineSegment lineSegment = new LineSegment(textFrame.Max, new Vector2(textFrame.MaxX, textFrame.MinY));
                context.Stroke(lineSegment, Colors.Black);
            }
        }

        protected internal override void OnAncestryChanged(ViewContainer ancestor)
        {
            RootView root = Root as RootView;
            if (root != null && root.FocusedView == this)
                root.FocusedView = null;

            base.OnAncestryChanged(ancestor);
        }

        protected internal override void OnAddToParent(ViewContainer parent)
        {
            RootView root = Root as RootView;
            if (root != null) root.FocusedView = this;
            base.OnAddToParent(parent);
        }

        protected internal override void OnRemovedFromParent(ViewContainer parent)
        {
            RootView root = Root as RootView;
            if (root != null) root.FocusedView = null;
            base.OnRemovedFromParent(parent);
        }
        #endregion
    }
}