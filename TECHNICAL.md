# Technical Details

## Running

```bash
cd Example
dotnet build
dotnet run          # ESC to quit
```

**Requirements:** .NET 10 SDK, [Raylib-cs](https://github.com/ChristopherPratt/Raylib-cs) (auto-restored via NuGet)

## Controls

| Key | Action |
|---|---|
| SPACE | Pause / Resume |
| A | Toggle atom detection overlay |
| Scroll | Zoom (toward mouse) |
| Arrows / Right-click | Pan |
| Home | Reset camera |
| ESC | Quit |

## What happens when you run it

The simulation starts with a **Big Bang**: all 400 particles packed in a tiny central disk, exploding outward. Two species — heavy-positive and light-negative — interact via gravity and Coulomb forces. Radial damping in close encounters dissipates energy on approach.

As the universe cools, the HUD shows the cosmological epoch:

1. **BIG BANG** — hot plasma, no structure
2. **COOLING** — particles spread, collisions dissipate energy
3. **RECOMBINATION** — temperature drops below binding energy, `H:` counter starts climbing
4. **ATOMIC ERA** — stable hydrogen-like bound pairs dominate

The `AtomDetector` is a pure observer — it never touches the simulation. It applies four criteria to identify emergent atoms: mutual nearest neighbor, negative binding energy, persistence over time, and spatial isolation.

## Architecture

| File | Role |
|---|---|
| `Vec2.cs` | 2D vector struct |
| `Particle.cs` | Position, velocity, mass, charge, Higgs coupling |
| `Field.cs` | Uniform Higgs field (intensity = 1.0) |
| `Physics.cs` | Constants (G, K, SpeedLimit), forces (Gravity, Electric), toroidal topology |
| `Simulation.cs` | Big Bang init, N-body force loop, radial damping, toroidal integration |
| `AtomDetector.cs` | Pure observation: detects bound pairs via 4 criteria |
| `Diagnostics.cs` | Derived metrics (kinetic energy, temperature) — observations, NOT variables |
| `Renderer.cs` | Raylib-cs visualization, atom overlays, epoch HUD |
