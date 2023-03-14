using Serilog;
using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;

namespace Window
{
    public class UI
    {
        public static unsafe void Main(Color* ptr, int colorWid, int colorHgt)
        {
            Log.Debug("Open Window");
            SetTraceLogLevel((int)TraceLogLevel.LOG_WARNING);
            SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            InitWindow(1280, 720, "Maestro");
            SetTargetFPS(60);

            // Make base texture
            int wid = GetRenderWidth();
            int hgt = GetRenderHeight();
            float ratio = wid / (float)hgt;
            Image image = GenImageColor(wid, hgt, BLACK);
            Texture texPattern = LoadTextureFromImage(image);
            SetTextureFilter(texPattern, TextureFilter.TEXTURE_FILTER_BILINEAR);

            while (!WindowShouldClose()) // Detect window close button or ESC key
            {
                if (IsWindowResized())
                {
                    int thisWid = GetRenderWidth();
                    int thisHgt = GetRenderHeight();    
                    float ratioDiff = thisWid / (float)thisHgt - ratio;
                    if (ratio != 0) Log.Verbose($"Adjusting window for difference of {ratioDiff}");
                    if (ratioDiff < 0) SetWindowSize(thisWid, (int)(thisWid / ratio));
                    else if (ratioDiff > 0) SetWindowSize((int)(thisHgt * ratio), thisHgt);
                }
                BeginDrawing();
                ClearBackground(SKYBLUE);
                UpdateTexture(texPattern, ptr);
                DrawTextureEx(texPattern, Vector2.Zero, 0f, GetRenderWidth() / (float)colorWid + GetScreenHeight() / (float)colorHgt, WHITE);
                EndDrawing();
            }
            CloseWindow();
        }
    }
}