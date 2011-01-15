using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine;

namespace Orion.Game.Presentation.Gui
{
	partial class MatchUI2
	{
        private sealed class MessageLabel : Label
        {
            #region Fields
            private readonly TimeSpan expirationTime;
            #endregion

            #region Constructors
            public MessageLabel(string text, ColorRgb color, TimeSpan expirationTime)
            {
                Text = text;
                Color = color;
                this.expirationTime = expirationTime;
            }
            #endregion

            #region Properties
            /// <summary>
            /// Gets the time when this label is supposed to have completely disappeared.
            /// </summary>
            public TimeSpan ExpirationTime
            {
                get { return expirationTime; }
            }
            #endregion
        }

        private sealed class MessageConsole : StackLayout
        {
            #region Fields
            private static readonly TimeSpan messageLifeSpan = new TimeSpan(0, 0, 10);
            private static readonly TimeSpan messageFadeOutDuration = new TimeSpan(0, 0, 2);
            private const int maxMessageCount = 15;

            private readonly OrionGuiStyle style;
            private readonly Action<UIManager, TimeSpan> updatedEventHandler;
            private TimeSpan time;
            #endregion

            #region Constructors
            public MessageConsole(OrionGuiStyle style)
            {
                Argument.EnsureNotNull(style, "style");

                this.style = style;
                updatedEventHandler = OnUpdated;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Adds a message to this console.
            /// </summary>
            /// <param name="text">The text of the message.</param>
            /// <param name="color">The color of the message.</param>
            public void AddMessage(string text, ColorRgb color)
            {
                MessageLabel label = new MessageLabel(text, color, time + messageLifeSpan);
                style.ApplyStyle(label);

                if (Children.Count == maxMessageCount)
                    Children.RemoveAt(Children.Count - 1);

                Children.Insert(0, label);
            }

            protected override void OnManagerChanged(UIManager previousManager)
            {
                if (previousManager != null) previousManager.Updated -= updatedEventHandler;
                if (Manager != null) Manager.Updated += updatedEventHandler;
            }

            private void OnUpdated(UIManager manager, TimeSpan timeDelta)
            {
                time += timeDelta;

                for (int i = Children.Count - 1; i >= 0; --i)
                {
                    MessageLabel label = (MessageLabel)Children[i];
                    TimeSpan remainingLifeSpan = label.ExpirationTime - time;
                    if (remainingLifeSpan < TimeSpan.Zero)
                    {
                        Children.RemoveAt(i);
                        continue;
                    }

                    if (remainingLifeSpan < messageFadeOutDuration)
                    {
                        float alpha = (float)(remainingLifeSpan.TotalSeconds / messageFadeOutDuration.TotalSeconds);
                        label.Color = new ColorRgba(label.Color.Rgb, alpha);
                    }
                }
            }
            #endregion
        }
	}
}
