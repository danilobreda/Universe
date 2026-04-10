using Raylib_cs;
using Universo;

const int particleCount = 400;
const float worldSize = 500f;

var sim = new Simulation(particleCount, worldSize, seed: 42);
var detector = new AtomDetector();
using var renderer = new Renderer(900, 900, worldSize);
renderer.Detector = detector;

bool paused = false;

while (!renderer.ShouldClose)
{
    if (Raylib.IsKeyPressed(KeyboardKey.Space))
        paused = !paused;
    if (Raylib.IsKeyPressed(KeyboardKey.A))
        renderer.ShowAtoms = !renderer.ShowAtoms;

    if (!paused)
    {
        sim.Step();
        detector.Update(sim.Particles, sim.Field);
    }

    renderer.Render(sim, paused);
}
