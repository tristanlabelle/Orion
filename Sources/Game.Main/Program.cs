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
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Application = System.Windows.Forms.Application;
using MouseButtons = System.Windows.Forms.MouseButtons;

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


        private sealed class GuiRenderer : IGuiRenderer
        {
            #region Fields
            private readonly GraphicsContext graphicsContext;
            private readonly Texture menuBackgroundTexture;
            private readonly Texture buttonUpTexture;
            private readonly Texture buttonDownTexture;
            private readonly Texture checkBoxUncheckedTexture;
            private readonly Texture checkBoxCheckedTexture;
            private readonly Texture scrollBarArrowUpTexture;
            private readonly Texture scrollBarThumbTexture;
            private readonly object[] args = new object[2];
            #endregion

            #region Constructors
            public GuiRenderer(GraphicsContext graphicsContext)
            {
                Argument.EnsureNotNull(graphicsContext, "graphicsContext");

                this.graphicsContext = graphicsContext;
                menuBackgroundTexture = graphicsContext.CreateTextureFromFile("../../../Assets/Textures/MenuBackground.png");
                buttonUpTexture = graphicsContext.CreateTextureFromFile("../../../Assets/Textures/Gui/Button_Up.png");
                buttonDownTexture = graphicsContext.CreateTextureFromFile("../../../Assets/Textures/Gui/Button_Down.png");
                checkBoxUncheckedTexture = graphicsContext.CreateTextureFromFile("../../../Assets/Textures/Gui/CheckBox_Unchecked.png");
                checkBoxCheckedTexture = graphicsContext.CreateTextureFromFile("../../../Assets/Textures/Gui/CheckBox_Checked.png");
                scrollBarArrowUpTexture = graphicsContext.CreateTextureFromFile("../../../Assets/Textures/Gui/ScrollBar_Arrow_Up.png");
                scrollBarThumbTexture = graphicsContext.CreateTextureFromFile("../../../Assets/Textures/Gui/ScrollBar_Thumb.png");
            }
            #endregion

            #region Properties
            #endregion

            #region Methods
            public Size MeasureText(Control control, string text)
            {
                return graphicsContext.Measure(text, null);
            }

            public Size GetImageSize(Control control, object source)
            {
                if (source is Texture) return ((Texture)source).Size;
                return Size.Zero;
            }

            public Size GetCheckBoxSize(CheckBox checkBox)
            {
                return checkBoxUncheckedTexture.Size;
            }

            public void BeginDraw(Control control)
            {
                Region rectangle;
                if (!control.TryGetRectangle(out rectangle))
                {
                    Debug.Fail("BeginDraw was called for a control without a rectangle.");
                    return;
                }

                args[0] = control;
                args[1] = rectangle;
                GetType().InvokeMember("Draw", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, this, args);

                graphicsContext.PushScissorRegion(rectangle);
            }

            public void EndDraw(Control control)
            {
                graphicsContext.PopScissorRegion();
            }

            private void Draw(Control control, Region rectangle) { }

            private void Draw(UIManager control, Region rectangle)
            {
                graphicsContext.Fill(rectangle, menuBackgroundTexture);
            }

            private void Draw(RepeatButton control, Region rectangle)
            {
                ScrollBar parentScrollBar = control.Parent as ScrollBar;
                if (parentScrollBar != null)
                {
                    graphicsContext.Fill(rectangle, scrollBarArrowUpTexture);
                    return;
                }
            }

            private void Draw(Thumb thumb, Region rectangle)
            {
                graphicsContext.Fill(rectangle, scrollBarThumbTexture);
            }

            private void Draw(Button control, Region rectangle)
            {
                CheckBox parentCheckBox = control.Parent as CheckBox;
                if (parentCheckBox != null && parentCheckBox.Content != control)
                {
                    graphicsContext.Fill(rectangle, parentCheckBox.IsChecked ? checkBoxCheckedTexture : checkBoxUncheckedTexture);
                    return;
                }

                bool isMouseOver = rectangle.Contains(control.Manager.MouseState.Position);
                FillNinePart(rectangle, isMouseOver ? buttonDownTexture : buttonUpTexture);
            }

            private void Draw(Label control, Region rectangle)
            {
                graphicsContext.Draw(new Text(control.Text, control.Font), rectangle.Min, Colors.Black);
            }

            private void Draw(TextField control, Region rectangle)
            {
                Draw((Control)control, rectangle);
                graphicsContext.Draw(control.Text, rectangle.Min, Colors.Black);
            }

            private void Draw(ImageBox control, Region rectangle)
            {
                Texture texture = control.Source as Texture;

                if (texture != null) graphicsContext.Fill(rectangle, texture);
            }

            private void FillNinePart(Region rectangle, Texture texture)
            {
                int cornerSize = texture.Width / 2 - 1;
                int middleWidth = rectangle.Width - cornerSize * 2;
                int middleHeight = rectangle.Height - cornerSize * 2;
                float cornerTextureSize = cornerSize / (float)texture.Width;
                float middleTextureSize = 2.0f / (float)texture.Width;

                // Min Row
                DrawTexturePart(texture, rectangle.MinX, rectangle.MinY, cornerSize, cornerSize,
                    0, 0, cornerTextureSize, cornerTextureSize);
                DrawTexturePart(texture, rectangle.MinX + cornerSize, rectangle.MinY, middleWidth, cornerSize,
                    cornerTextureSize, 0, middleTextureSize, cornerTextureSize);
                DrawTexturePart(texture, rectangle.MinX + cornerSize + middleWidth, rectangle.MinY, cornerSize, cornerSize,
                    cornerTextureSize + middleTextureSize, 0, cornerTextureSize, cornerTextureSize);

                // Middle Row
                DrawTexturePart(texture, rectangle.MinX, rectangle.MinY + cornerSize, cornerSize, middleHeight,
                    0, cornerTextureSize, cornerTextureSize, middleTextureSize);
                DrawTexturePart(texture, rectangle.MinX + cornerSize, rectangle.MinY + cornerSize, middleWidth, middleHeight,
                    cornerTextureSize, cornerTextureSize, middleTextureSize, middleTextureSize);
                DrawTexturePart(texture, rectangle.MinX + cornerSize + middleWidth, rectangle.MinY + cornerSize, cornerSize, middleHeight,
                    cornerTextureSize + middleTextureSize, cornerTextureSize, cornerTextureSize, middleTextureSize);

                // Max Row
                DrawTexturePart(texture, rectangle.MinX, rectangle.MinY + cornerSize + middleHeight, cornerSize, cornerSize,
                    0, cornerTextureSize + middleTextureSize, cornerTextureSize, cornerTextureSize);
                DrawTexturePart(texture, rectangle.MinX + cornerSize, rectangle.MinY + cornerSize + middleHeight, middleWidth, cornerSize,
                    cornerTextureSize, cornerTextureSize + middleTextureSize, middleTextureSize, cornerTextureSize);
                DrawTexturePart(texture, rectangle.MinX + cornerSize + middleWidth, rectangle.MinY + cornerSize + middleHeight, cornerSize, cornerSize,
                    cornerTextureSize + middleTextureSize, cornerTextureSize + middleTextureSize, cornerTextureSize, cornerTextureSize);
            }

            private void DrawTexturePart(Texture texture, int x, int y, int width, int height,
                float textureX, float textureY, float textureWidth, float textureHeight)
            {
                Rectangle rectangle = new Rectangle(x, y, width, height);
                Rectangle textureRectangle = new Rectangle(textureX, textureY, textureWidth, textureHeight);
                graphicsContext.Fill(rectangle, texture, textureRectangle);
            }
            #endregion
        }

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

            IGuiRenderer renderer = new GuiRenderer(graphicsContext);
            UIManager uiManager = new UIManager(renderer)
            {
                Size = window.ClientAreaSize,
                Content = new DockPanel()
                {
                    LastChildFill = true,
                    InitChildren = new[]
                    {
                        new DockedControl(new DockPanel()
                        {
                            MinHeight = 60,
                            InitChildren = new[]
                            {
                                new DockedControl(new Button("Retour") { MinWidth = 200 }, Direction.MinX),
                                new DockedControl(new Button("Commencer") { MinWidth = 200 }, Direction.MaxX)
                            }
                        }, Direction.MinY),

                        new DockedControl(new StackPanel()
                        {
                            Orientation = Orientation.Vertical,
                            VerticalAlignment = Alignment.Max,
                            MinXMargin = 5,
                            InitChildren = new Control[]
                            {
                                new DockPanel()
                                {
                                    LastChildFill = true,
                                    InitChildren = new[]
                                    {
                                        new DockedControl(new TextField("200") { MinWidth = 100 }, Direction.MaxX),
                                        new DockedControl(new Label("Aladdium initial: "), Direction.MinX)
                                    }
                                },
                                new CheckBox("Code de triche")
                            }
                        }, Direction.MaxX),

                        new DockedControl(new ScrollBar()
                        {
                            Maximum = 100,
                            Value = 20,
                            Length = 20,
                            SmallStep = 10
                        }, Direction.MaxX),

                        new DockedControl(new StackPanel()
                        {
                            Orientation = Orientation.Vertical,
                            VerticalAlignment = Alignment.Max,
                            InitChildren = new[]
                            {
                                new DockPanel()
                                {
                                    LastChildFill = true,
                                    InitChildren = new[]
                                    {
                                        new DockedControl(new Label("Player 1 Name"), Direction.MinX),
                                        new DockedControl(new Button("Kick"), Direction.MaxX),
                                        new DockedControl(new Button("Future color combo box"), Direction.MaxX),
                                    }
                                },
                                new DockPanel()
                                {
                                    LastChildFill = true,
                                    InitChildren = new[]
                                    {
                                        new DockedControl(new Label("Player 2 Name"), Direction.MinX),
                                        new DockedControl(new Button("Kick"), Direction.MaxX),
                                        new DockedControl(new Button("Future color combo box"), Direction.MaxX),
                                    }
                                }
                            }
                        }, Direction.MinX)
                    }
                }
            };

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
            int frameCount = -1;
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
