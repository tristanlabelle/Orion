using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Audio;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Engine.Gui2;
using Application = System.Windows.Forms.Application;
using Orion.Engine.Input;

namespace Orion.Game.Main
{
    internal class Program
    {
        #region Fields
        private const float TargetFramesPerSecond = 40;
        private const float TargetSecondsPerFrame = 1.0f / TargetFramesPerSecond;
        private const float TimeSpeedMultiplier = 1;
        private const int DefaultHostPort = 41223;
        private const int DefaultClientPort = 41224;
        #endregion

        #region Methods
        #region Logging Utilities
        private static void EnableLogging()
        {
            foreach (string logFileName in GetPossibleLogFileNames())
            {
                try
                {
                    var stream = new FileStream(logFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                    var writer = new StreamWriter(stream);
#if DEBUG
                    writer.AutoFlush = true;
#endif
                    Trace.Listeners.Add(new TextWriterTraceListener(writer));
                    break;
                }
                catch (IOException) { }
            }
        }

        private static IEnumerable<string> GetPossibleLogFileNames()
        {
            const string baseFileNameWithoutExtension = "Log";
            const string extension = ".txt";
            yield return baseFileNameWithoutExtension + extension;
            for (int i = 2; i < 10; ++i)
                yield return "{0} ({1}){2}".FormatInvariant(baseFileNameWithoutExtension, i, extension);
        }
        #endregion

        #region Main
        /// <summary>
        /// Main entry point for the program.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            IGameWindow window = new OpenTKGameWindow("Orion", WindowMode.Windowed, new Size(1024, 768));
            GraphicsContext graphicsContext = window.GraphicsContext;

            UIManager uiManager = new UIManager(graphicsContext);

            Queue<InputEvent> inputEventQueue = new Queue<InputEvent>();
            window.InputReceived += (sender, args) => inputEventQueue.Enqueue(args);
            window.Resized += sender => uiManager.Size = window.ClientAreaSize;

            StackPanel stackPanel = new StackPanel
            {
                HorizontalAlignment = Alignment.Center,
                VerticalAlignment = Alignment.Center,
                ItemGap = 10
            };

            stackPanel.Children.Add(new Label { Text = "Foo" });
            stackPanel.Children.Add(new Label { Text = "Bar" });
            stackPanel.Children.Add(new Button("Frob"));
            uiManager.Root = stackPanel;

            while (!window.WasClosed)
            {
                window.Update();

                while (inputEventQueue.Count > 0)
                {
                    InputEvent inputEvent = inputEventQueue.Dequeue();
                    if (inputEvent.Type == InputEventType.Mouse)
                    {
                        MouseEventType type;
                        MouseEventArgs args;
                        inputEvent.GetMouse(out type, out args);
                        uiManager.SendMouseEvent(type, args);
                    }
                }

                graphicsContext.Clear(Colors.Green);
                graphicsContext.ProjectionBounds = new Engine.Geometry.Rectangle(0, 0, uiManager.Size.Width, uiManager.Size.Height);
                uiManager.Draw();
                graphicsContext.Present();
            }
        }
        #endregion
        #endregion
    }
}
