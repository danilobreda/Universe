using System.Numerics;
using Raylib_cs;

namespace Universo;

public class Renderer : IDisposable
{
    private readonly int _width;
    private readonly int _height;
    private Camera2D _camera;

    public bool ShowAtoms { get; set; } = true;
    public AtomDetector? Detector { get; set; }

    public Renderer(int width, int height, float worldSize)
    {
        _width = width;
        _height = height;

        _camera = new Camera2D
        {
            Offset = new Vector2(width / 2f, height / 2f),
            Target = new Vector2(worldSize / 2f, worldSize / 2f),
            Rotation = 0f,
            Zoom = width / worldSize
        };

        Raylib.InitWindow(width, height, "Universo");
        Raylib.SetTargetFPS(60);
    }

    public bool ShouldClose => Raylib.WindowShouldClose();

    public void Render(Simulation sim, bool paused)
    {
        HandleCamera(sim.WorldSize);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        Raylib.BeginMode2D(_camera);
        DrawParticles(sim);
        if (ShowAtoms) DrawAtoms(sim);
        Raylib.EndMode2D();

        DrawHUD(sim, paused);
        Raylib.EndDrawing();
    }

    private void HandleCamera(float worldSize)
    {
        float wheel = Raylib.GetMouseWheelMove();
        if (wheel != 0)
        {
            var mouseWorld = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
            _camera.Zoom *= 1f + wheel * 0.1f;
            _camera.Zoom = Math.Clamp(_camera.Zoom, 0.5f, 80f);
            var mouseWorldAfter = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
            _camera.Target += mouseWorld - mouseWorldAfter;
        }

        if (Raylib.IsMouseButtonDown(MouseButton.Right) || Raylib.IsMouseButtonDown(MouseButton.Middle))
        {
            var delta = Raylib.GetMouseDelta();
            _camera.Target -= delta / _camera.Zoom;
        }

        float speed = 3f / _camera.Zoom;
        if (Raylib.IsKeyDown(KeyboardKey.Right)) _camera.Target.X += speed;
        if (Raylib.IsKeyDown(KeyboardKey.Left)) _camera.Target.X -= speed;
        if (Raylib.IsKeyDown(KeyboardKey.Down)) _camera.Target.Y += speed;
        if (Raylib.IsKeyDown(KeyboardKey.Up)) _camera.Target.Y -= speed;

        if (Raylib.IsKeyPressed(KeyboardKey.Home))
        {
            _camera.Target = new Vector2(worldSize / 2f, worldSize / 2f);
            _camera.Zoom = _width / worldSize;
        }
    }

    private void DrawParticles(Simulation sim)
    {
        foreach (var p in sim.Particles)
        {
            float mass = Physics.Mass(p, sim.Field);
            float radius = 0.3f + MathF.Sqrt(mass) * 0.45f;

            Color color;
            if (p.Charge > 0)
                color = new Color(255, 90, 90, 255);   // pesada+ (vermelho)
            else if (p.Charge < 0)
                color = new Color(90, 150, 255, 255);  // leve- (azul)
            else
                color = new Color(200, 200, 200, 230); // neutra (cinza)

            Raylib.DrawCircleV(p.Position.ToVector2(), radius, color);
        }
    }

    private void DrawAtoms(Simulation sim)
    {
        if (Detector == null) return;

        var ring = new Color(255, 255, 255, 140);
        var line = new Color(255, 255, 255, 70);

        foreach (var (a, b) in Detector.Atoms)
        {
            var pa = sim.Particles[a].Position;
            var pb = sim.Particles[b].Position;

            // Desenrolar pelo wrap toroidal pra desenhar a linha sem cortar pela borda
            var dir = Physics.MinImage(pb - pa);
            var pbUnwrapped = pa + dir;
            var mid = pa + dir * 0.5f;
            float r = dir.Length() * 0.5f + 0.7f;

            Raylib.DrawCircleLinesV(mid.ToVector2(), r, ring);
            Raylib.DrawLineV(pa.ToVector2(), pbUnwrapped.ToVector2(), line);
        }
    }

    private void DrawHUD(Simulation sim, bool paused)
    {
        float temp = Diagnostics.Temperature(sim.Particles);
        float energy = Diagnostics.TotalKineticEnergy(sim.Particles, sim.Field);

        int hudH = Detector != null ? 52 : 28;
        Raylib.DrawRectangle(0, 0, _width, hudH, new Color(0, 0, 0, 200));

        string line1 = $"Tick: {sim.Tick}  |  Part: {sim.Particles.Count}  |  Temp: {temp:F2}  |  E: {energy:F1}  |  Zoom: {_camera.Zoom:F1}x";
        if (paused) line1 += "  |  PAUSADO";
        Raylib.DrawText(line1, 8, 5, 16, Color.White);

        if (Detector != null)
        {
            string epoch = ClassifyEpoch(temp, Detector);
            string line2 = $"H: {Detector.HydrogenCount}    livres: e- {Detector.FreeLights}  p+ {Detector.FreeHeavies}    [{epoch}]";
            Raylib.DrawText(line2, 8, 27, 16, new Color(180, 220, 255, 255));
        }

        string help = "[SPACE] Pausar  [A] Atomos  [Scroll] Zoom  [Setas/Mouse] Pan  [Home] Reset  [ESC] Sair";
        Raylib.DrawText(help, 8, _height - 22, 14, new Color(120, 120, 120, 200));
    }

    /// <summary>
    /// Rótulo derivado da temperatura + estado do detector. Pura observação cosmética.
    /// </summary>
    private static string ClassifyEpoch(float temp, AtomDetector det)
    {
        if (temp > 80f) return "BIG BANG";
        if (temp > 8f) return "RESFRIANDO";
        if (det.HydrogenCount == 0) return "ERA ESCURA";
        if (det.FreeLights > det.HydrogenCount) return "RECOMBINACAO";
        return "ERA ATOMICA";
    }

    public void Dispose()
    {
        Raylib.CloseWindow();
    }
}
