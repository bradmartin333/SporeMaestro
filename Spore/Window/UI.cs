using Serilog;
using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;

namespace Window
{
    public class UI
    {
        private const int TOOL_BOX_HGT = 50;

        public static unsafe void Main(Color* ptr, int colorWid, int colorHgt)
        {
            Log.Debug("Open Window");
            SetTraceLogLevel((int)TraceLogLevel.LOG_WARNING);
            SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            InitWindow(1280, 720 + TOOL_BOX_HGT, "Maestro");
            SetTargetFPS(60);

            // Make base texture
            Image image = GenImageColor(colorWid, colorHgt, MAROON);
            Texture texPattern = LoadTextureFromImage(image);
            SetTextureFilter(texPattern, TextureFilter.TEXTURE_FILTER_BILINEAR);

            // Icon
            Image icon = LoadImage("logo.png");
            ImageResize(&icon, icon.width / 30, icon.height / 30);
            SetWindowIcon(icon);

            while (!WindowShouldClose()) // Detect window close button or ESC key
            {
                BeginDrawing();
                ClearBackground(BLACK);
                UpdateTexture(texPattern, ptr);
                DrawTexture(texPattern, 0, TOOL_BOX_HGT, WHITE);
                EndDrawing();
            }
            CloseWindow();
        }
    }
}