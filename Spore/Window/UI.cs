using Serilog;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

namespace Window
{
    public class UI
    {
        public static unsafe void Main()
        {
            Log.Debug("Open Window");
            SetTraceLogLevel((int)TraceLogLevel.LOG_WARNING);
            SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            InitWindow(1280, 720, "Hello, Raylib-CsLo");
            SetTargetFPS(60);
            while (!WindowShouldClose()) // Detect window close button or ESC key
            {
                BeginDrawing();
                ClearBackground(SKYBLUE);
                DrawFPS(10, 10);
                DrawText("Raylib is easy!!!", 640, 360, 50, RED);
                EndDrawing();
            }
            CloseWindow();
        }
    }
}