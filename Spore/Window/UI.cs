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
            int wid = GetRenderWidth();
            int hgt = GetRenderHeight();
            float ratio = wid / (float)hgt;
            Image image = GenImageColor(wid, hgt, BLACK);
            Texture texPattern = LoadTextureFromImage(image);
            SetTextureFilter(texPattern, TextureFilter.TEXTURE_FILTER_BILINEAR);

            // Icon
            Image icon = LoadImage("logo.png");
            ImageResize(&icon, icon.width / 30, icon.height / 30);
            SetWindowIcon(icon);

            while (!WindowShouldClose()) // Detect window close button or ESC key
            {
                if (IsWindowResized())
                {
                    int thisWid = GetRenderWidth();
                    int thisHgt = GetRenderHeight();    
                    float ratioDiff = thisWid / (float)thisHgt - ratio;
                    if (ratioDiff < 0) SetWindowSize(thisWid, (int)(thisWid / ratio));
                    else if (ratioDiff > 0) SetWindowSize((int)(thisHgt * ratio), thisHgt);
                }
                BeginDrawing();
                ClearBackground(BLACK);
                UpdateTexture(texPattern, ptr);
                DrawTextureEx(texPattern, new Vector2(0, TOOL_BOX_HGT), 0f, GetRenderWidth() / (float)colorWid + GetScreenHeight() / (float)colorHgt, WHITE);
                EndDrawing();
            }
            CloseWindow();
        }
    }
}