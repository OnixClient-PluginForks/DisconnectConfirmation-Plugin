using OnixRuntime.Api;
using OnixRuntime.Api.Inputs;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.Utils;
using OnixRuntime.Plugin;

namespace DisconnectConfirmation {
    public class DisconnectConfirmation : OnixPluginBase {
        public static DisconnectConfirmation Instance { get; private set; } = null!;
        public static DisconnectConfirmationConfig Config { get; private set; } = null!;

        private GameUIElement? quitButton;
        private bool isPauseScreenActive;
        private bool quitButtonClickedOnce;
        private long lastInputTime;


        private long lastGlobalInputTime;
        private const long INPUT_DEBOUNCE_TICKS = TimeSpan.TicksPerMillisecond * 5;

        private static readonly TexturePath PurpleBorder = TexturePath.Game("textures/ui/purpleBorder");
        public static readonly NineSlice PurpleBorderNineSlice = new(PurpleBorder);

        public DisconnectConfirmation(OnixPluginInitInfo initInfo) : base(initInfo) {
            Instance = this;
            DisablingShouldUnloadPlugin = false;
#if DEBUG
            // WaitForDebuggerToBeAttached();
#endif
        }

        private HashSet<string> processedElements = new();
        private string str = "";

        private void ProcessElement(GameUIElement element, int depth = 0) {
            string indent = new(' ', depth * 4);
            string elementIdentifier = $"{indent}{element.Name}, {element.Rect.ToString()}, {element.JsonProperties}";

            if (!processedElements.Contains(elementIdentifier)) {
                str += $"{elementIdentifier}\n";
                processedElements.Add(elementIdentifier);
            }

            if (element.Children.Length > 0) {
                foreach (GameUIElement childElement in element.Children) {
                    ProcessElement(childElement, depth + 1);
                }
            }
        }

        protected override void OnLoaded() {
            //Onix.Client.NotifyTray("Copied");
            ////Console.WriteLine($"Plugin {CurrentPluginManifest.Name} loaded!");
            //if (Onix.Gui.RootUiElement?.Children != null) {
            //    foreach (GameUIElement gameUiElement in Onix.Gui.RootUiElement.Children) {
            //        ProcessElement(gameUiElement, 0);
            //    }
            //}
            //Clipboard.SetText(str);
            //Console.WriteLine("Copied ui elements to clipboard at time: " + DateTime.Now);
            Config = new DisconnectConfirmationConfig(PluginDisplayModule, true);

            Onix.Events.Input.Input += InputOnInput;
            Onix.Events.Rendering.RenderScreenGame += RenderingOnRenderScreenGame;
        }

        private void RenderingOnRenderScreenGame(RendererGame gfx, float delta, string screenName, bool isHudHidden,
            bool isClientUi) {
            CheckPauseScreenAndQuitButton();

            if (quitButton != null && isPauseScreenActive) {
                Rect absoluteRect = GetAbsoluteRect(quitButton);
                
                //gfx.FillRectangle(absoluteRect, ColorF.Red.WithOpacity(0.5f));

                Vec2 mousePos = Onix.Gui.MousePosition;
                if (!absoluteRect.Contains(mousePos)) {
                    quitButtonClickedOnce = false;
                }
            }
            

            if (quitButtonClickedOnce) {
                Vec2 tooltipPos = new(Onix.Gui.MousePosition.X + 14, Onix.Gui.MousePosition.Y - 6f);
                const float padding = 4f;
                string displayText = "Click Again To Confirm Disconnect";
                gfx.FontType = FontType.Mojangles;
                Vec2 textSize = gfx.MeasureText(displayText);
                PurpleBorderNineSlice.Render(gfx, new Rect(tooltipPos.X - padding, tooltipPos.Y - padding, tooltipPos.X + textSize.X + padding, tooltipPos.Y + textSize.Y + padding - 0.5f), 1.0f);
                Vec2 textPos = new(tooltipPos.X, tooltipPos.Y);
                textPos.X += 0.5f;
                textPos.Y += 0.5f;
                gfx.RenderText(textPos, ColorF.White, displayText);
                gfx.FontType = FontType.UserPreference;
            }
        }

        private void CheckPauseScreenAndQuitButton() {
            if (Onix.Gui.ScreenName != "pause_screen") {
                isPauseScreenActive = false;
                quitButton = null;
                quitButtonClickedOnce = false;
                return;
            }

            isPauseScreenActive = false;
            quitButton = null;

            if (Onix.Gui.RootUiElement?.Children == null) return;

            // gotta love texture packs being inconsistent with button names...
            quitButton = FindElementByName(Onix.Gui.RootUiElement, "quit_button");
            if (quitButton == null) {
                quitButton = FindElementByName(Onix.Gui.RootUiElement, "exit_button");
            }

            if (quitButton == null) {
                quitButton = FindElementByName(Onix.Gui.RootUiElement, "exit");
            }

            if (quitButton == null) {
                quitButton = FindElementByName(Onix.Gui.RootUiElement, "disconnect_button");
            }

            if (quitButton == null) {
                quitButton = FindElementByName(Onix.Gui.RootUiElement, "disconnect");
            }

            if (quitButton == null) {
                quitButton = FindElementByName(Onix.Gui.RootUiElement, "leave_button");
            }

            if (quitButton == null) {
                quitButton = FindElementByName(Onix.Gui.RootUiElement, "leave");
            }

            if (quitButton != null) {
                isPauseScreenActive = true;
            }
        }

        private GameUIElement? FindElementByName(GameUIElement parent, string name) {
            return parent.Name == name
                ? parent
                : parent.Children.Select(child => FindElementByName(child, name)).OfType<GameUIElement>()
                    .FirstOrDefault();
        }

        private Rect GetAbsoluteRect(GameUIElement element) {
            float absoluteStartX = 0f;
            float absoluteStartY = 0f;

            GameUIElement? current = element;
            List<GameUIElement> path = [];

            while (current != null) {
                path.Add(current);
                current = GetParentElement(current);
            }

            path.Reverse();
            foreach (GameUIElement pathElement in path) {
                absoluteStartX += pathElement.Rect.X;
                absoluteStartY += pathElement.Rect.Y;
            }

            float absoluteEndX = absoluteStartX + element.Rect.Width;
            float absoluteEndY = absoluteStartY + element.Rect.Height;

            return new Rect(absoluteStartX, absoluteStartY, absoluteEndX, absoluteEndY);
        }

        private GameUIElement? GetParentElement(GameUIElement targetElement) {
            return Onix.Gui.RootUiElement == null
                ? null
                : FindParentElementRecursive(Onix.Gui.RootUiElement, targetElement);
        }

        private GameUIElement? FindParentElementRecursive(GameUIElement current, GameUIElement target) {
            return current.Children.Any(child => child == target)
                ? current
                : current.Children.Select(child => FindParentElementRecursive(child, target)).OfType<GameUIElement>()
                    .FirstOrDefault();
        }

        private bool InputOnInput(InputKey key, bool isDown) {
            long currentTime = DateTime.UtcNow.Ticks;
            if (currentTime - lastGlobalInputTime < INPUT_DEBOUNCE_TICKS) {
                return false;
            }

            lastGlobalInputTime = currentTime;
            if (key == InputKey.Type.LMB && isPauseScreenActive && quitButton != null) {

                Rect absoluteRect = GetAbsoluteRect(quitButton);

                Vec2 mousePos = Onix.Gui.MousePosition;
                if (absoluteRect.Contains(mousePos)) {
                    long currentTime2 = DateTime.UtcNow.Ticks;
                    if (currentTime2 - lastInputTime > TimeSpan.FromMilliseconds(1).Ticks) {
                        lastInputTime = currentTime2;
                        if (!quitButtonClickedOnce) {
                            quitButtonClickedOnce = true;
                            return true;
                        } else {
                            return false;
                        }
                    }
                } else {
                    quitButtonClickedOnce = false;
                }
            }

            return false;
        }
    }
}