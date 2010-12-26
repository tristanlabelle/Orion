using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui2;
using Orion.Engine.Input;
using Orion.Engine.Networking;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Audio;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Application = System.Windows.Forms.Application;
using MouseButtons = System.Windows.Forms.MouseButtons;
using Orion.Engine.Gui2.Adornments;

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
            System.Windows.Forms.Cursor.Hide();

            GuiRenderer renderer = new OrionGuiRenderer(graphicsContext);
            OrionGuiStyle style = new OrionGuiStyle(renderer);

            UIManager uiManager = style.CreateUIManager();
            uiManager.Adornment = new TextureAdornment(renderer.TryGetTexture("MenuBackground"));
            uiManager.Size = window.ClientAreaSize;
            
            DockPanel dockPanel = style.Create<DockPanel>();
            dockPanel.LastChildFill = true;
            uiManager.Content = dockPanel;

            ImageBox titleImageBox = style.Create<ImageBox>();
            titleImageBox.HorizontalAlignment = Alignment.Center;
            titleImageBox.Texture = renderer.TryGetTexture("Title");
            dockPanel.Dock(titleImageBox, Direction.MaxY);

            StackPanel buttonsStackPanel = style.Create<StackPanel>();
            buttonsStackPanel.HorizontalAlignment = Alignment.Center;
            buttonsStackPanel.VerticalAlignment = Alignment.Center;
            buttonsStackPanel.MinWidth = 300;
            buttonsStackPanel.MinChildSize = 50;
            buttonsStackPanel.ChildGap = 10;
            dockPanel.Dock(buttonsStackPanel, Direction.MinX);

            buttonsStackPanel.Stack(style.CreateTextButton("Monojoueur"));
            buttonsStackPanel.Stack(style.CreateTextButton("Multijoueur"));
            buttonsStackPanel.Stack(style.CreateTextButton("Crédits"));

            Action draw = () =>
            {
                graphicsContext.Clear(Colors.Green);
                graphicsContext.ProjectionBounds = new Rectangle(0, 0, uiManager.Size.Width, uiManager.Size.Height);
                uiManager.Draw();
                graphicsContext.Present();
            };

            Queue<InputEvent> inputEventQueue = new Queue<InputEvent>();
            window.InputReceived += (sender, args) => inputEventQueue.Enqueue(args);
            window.Resized += sender =>
            {
                uiManager.Size = window.ClientAreaSize;
                draw();
            };

            Stopwatch stopwatch = new Stopwatch();
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

                        uiManager.InjectMouseMove((int)args.X, (int)args.Y);
                        if (type == MouseEventType.WheelScrolled)
                        {
                            uiManager.InjectMouseWheel(args.WheelDelta);
                        }
                        else if (type == MouseEventType.ButtonPressed || type == MouseEventType.ButtonReleased)
                        {
                            MouseButtons buttons = MouseButtons.None;
                            if (args.Button == MouseButton.Left) buttons = MouseButtons.Left;
                            else if (args.Button == MouseButton.Middle) buttons = MouseButtons.Middle;
                            else if (args.Button == MouseButton.Right) buttons = MouseButtons.Right;

                            int pressCount = type == MouseEventType.ButtonReleased ? 0 : args.ClickCount;
                            if (buttons != MouseButtons.None) uiManager.InjectMouseButton(buttons, pressCount);
                        }
                    }
                    else if (inputEvent.Type == InputEventType.Keyboard)
                    {
                        KeyboardEventType type;
                        KeyboardEventArgs args;
                        inputEvent.GetKeyboard(out type, out args);

                        uiManager.InjectKey(args.KeyAndModifiers, type == KeyboardEventType.ButtonPressed);
                    }
                    else if (inputEvent.Type == InputEventType.Character)
                    {
                        char character;
                        inputEvent.GetCharacter(out character);

                        uiManager.InjectCharacter(character);
                    }
                }

                uiManager.Update(stopwatch.Elapsed);
                stopwatch.Reset();
                stopwatch.Start();
                draw();
            }
        }
        #endregion
        #endregion
    }
}
