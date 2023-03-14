using Serilog;
using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;

namespace Window
{
    public class UI
    {
        internal const int FONT_SIZE = 30;

        private static Texture MakeBaseTexture(int wid, int hgt)
        {
            Random Random = new();
            string[] loadingStrings = new string[] // Froms SIMS 2
            {
                "blurring reality lines",
                "reticulating 3-dimensional splines",
                "preparing captive simulators",
                "capacitating genetic modifiers",
                "destabilizing orbital payloads",
                "manipulating modal memory",
            };
            string loadingString = loadingStrings[Random.Next(loadingStrings.Length)];
            Image image = GenImageColor(wid, hgt, BLACK);
            unsafe { ImageDrawText(&image, $"{loadingString}...", 10, hgt - FONT_SIZE - 10, FONT_SIZE, MAROON); }
            return LoadTextureFromImage(image);
        }

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
            Texture texPattern = MakeBaseTexture(wid, hgt);
            SetTextureFilter(texPattern, TextureFilter.TEXTURE_FILTER_BILINEAR);
            NPatchInfo ninePatchInfo = new(new Rectangle(0, 0, colorWid, colorHgt), 16, 16, 16, 16, NPatchLayout.NPATCH_NINE_PATCH);

            while (!WindowShouldClose()) // Detect window close button or ESC key
            {
                BeginDrawing();
                ClearBackground(SKYBLUE);
                UpdateTexture(texPattern, ptr);
                DrawTexture(texPattern, 0, 0, WHITE);
                //DrawTextureNPatch(texPattern, ninePatchInfo, 
                //                  new Rectangle(0, 0, GetRenderWidth(), GetRenderHeight()), 
                //                  Vector2.Zero, 0f, WHITE);
                EndDrawing();
            }
            CloseWindow();
        }
    }
}