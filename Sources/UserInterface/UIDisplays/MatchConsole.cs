using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.UserInterface.Widgets;
using Orion.Geometry;
using Orion.Engine.Graphics;

namespace Orion.UserInterface
{
    /// <summary>
    /// Provides the user interface for the in-game message console.
    /// </summary>
    public sealed class MatchConsole : Frame
    {
        #region Nested Types
        private sealed class Message : Label
        {
            #region Fields
            private static readonly TimeSpan LifeSpan = new TimeSpan(0, 0, 10);
            private static readonly TimeSpan FadeOutDuration = new TimeSpan(0, 0, 1);

            private float ageInSeconds;
            #endregion

            #region Constructors
            public Message(string text, ColorRgb color)
                : base(text)
            {
                base.Color = color;
            }
            #endregion

            #region Properties
            public bool IsDead
            {
                get { return ageInSeconds >= LifeSpan.TotalSeconds; }
            }
            #endregion

            #region Methods
            protected override void Update(float timeDeltaInSeconds)
            {
                ageInSeconds += timeDeltaInSeconds;

                float timeBeforeDeath = (float)LifeSpan.TotalSeconds - ageInSeconds;
                if (timeBeforeDeath < FadeOutDuration.TotalSeconds)
                {
                    float alpha = timeBeforeDeath / (float)FadeOutDuration.TotalSeconds;
                    if (alpha < 0) alpha = 0;
                    base.Color = new ColorRgba(base.Color.Rgb, alpha);
                }
            }
            #endregion
        }
        #endregion

        #region Instance
        #region Fields
        private static readonly int MaxMessageCount = 15;

        private readonly Queue<Message> messages = new Queue<Message>();
        #endregion

        #region Constructors
        public MatchConsole(Rectangle rectangle)
            : base(rectangle, null)
        {
            base.CaptureMouseEvents = false;
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void AddMessage(string text, ColorRgb color)
        {
            Argument.EnsureNotNull(text, "text");

            Message message = new Message(text, color);
            float height = message.Frame.Height;

            foreach (Message writtenMessage in messages)
                writtenMessage.Frame = writtenMessage.Frame.TranslatedBy(0, height);

            messages.Enqueue(message);
            Children.Add(message);

            if (messages.Count > MaxMessageCount)
            {
                Message oldestMessage = messages.Dequeue();
                oldestMessage.Dispose();
            }
        }

        protected override void Update(float timeDeltaInSeconds)
        {
            while (messages.Count > 0 && messages.Peek().IsDead)
            {
                Message deadMessage = messages.Dequeue();
                deadMessage.Dispose();
            }
        }
        #endregion
        #endregion
    }
}
