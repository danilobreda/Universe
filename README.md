# Universo — Mini-Universe Simulator

A 2D emergent physics simulation where **simple local rules** generate complex behavior: orbital structures, thermal regions, and proto-atoms — without ever programming those concepts directly.

Built in C# .NET 10 with [Raylib-cs](https://github.com/ChristopherPratt/Raylib-cs) for real-time visualization.

---

## Blog Posts

This project sparked a deep conversation about emergence, consciousness, the simulation hypothesis, and the surprising structural parallels between computational simulation and religious cosmology.

The full essay is available in two languages:

- **Portugues**: [`artigo.md`](artigo.md) — *De 400 Particulas a Deus: O Que Um Simulador de Universo Me Ensinou Sobre Realidade*
- **English**: [`article.md`](article.md) — *From 400 Particles to God: What a Universe Simulator Taught Me About Reality*

---

## Philosophy

```
simple rules  ->  interactions  ->  patterns  ->  complexity
```

The golden rules:

1. **Never program the end result** — only local rules
2. **Everything must emerge** — energy, temperature, atoms are derived, not variables
3. **Deterministic** with a fixed seed
4. **Avoid overengineering** — simplicity first

> The universe is not made of complex objects. It is made of **simple rules that allow complexity**.

Full founding document: [`inicio.txt`](inicio.txt)

---

## Running

```bash
dotnet build
dotnet run          # ESC to quit
```

### Controls

| Key | Action |
|---|---|
| SPACE | Pause / Resume |
| A | Toggle atom detection overlay |
| Scroll | Zoom (toward mouse) |
| Arrows / Right-click | Pan |
| Home | Reset camera |
| ESC | Quit |

---

## What happens when you run it

The simulation starts with a **Big Bang**: all 400 particles packed in a tiny central disk, exploding outward. Two species — heavy-positive and light-negative — interact via gravity and Coulomb forces. Radial damping in close encounters dissipates energy on approach.

As the universe cools, the HUD shows the cosmological epoch:

1. **BIG BANG** — hot plasma, no structure
2. **COOLING** — particles spread, collisions dissipate energy
3. **RECOMBINATION** — temperature drops below binding energy, `H:` counter starts climbing
4. **ATOMIC ERA** — stable hydrogen-like bound pairs dominate

The `AtomDetector` is a pure observer — it never touches the simulation. It applies four criteria to identify emergent atoms: mutual nearest neighbor, negative binding energy, persistence over time, and spatial isolation.

---

## Architecture

- **`Vec2.cs`** — 2D vector struct
- **`Particle.cs`** — position, velocity, mass, charge, Higgs coupling
- **`Field.cs`** — uniform Higgs field (intensity = 1.0)
- **`Physics.cs`** — constants (G, K, SpeedLimit, CollisionRadius, CollisionDamping), forces (Gravity, Electric), minimum-image convention for toroidal topology
- **`Simulation.cs`** — Big Bang initialization, N-body force loop, radial damping, toroidal integration
- **`AtomDetector.cs`** — pure observation: detects bound pairs via 4 criteria (mutual nearest, E < 0, persistence, isolation)
- **`Diagnostics.cs`** — derived metrics (kinetic energy, temperature) — observations, NOT system variables
- **`Renderer.cs`** — Raylib-cs visualization, atom overlays, epoch HUD

---

## Key insights from the essays

**The narrow window.** There's a tiny range of constants where complexity can emerge. Outside it, dead universe. Inside, structure appears on its own. This mirrors the fine-tuning problem in real cosmology.

**Chance paints details, not the picture.** Changing the random seed changes which specific atoms form. Changing the constants changes whether atoms can exist at all.

**The chessboard argument.** A chess game is the same game whether it runs on silicon, gears, or a wooden board. If consciousness is computational, it's substrate-independent — and a simulated being would "know it's alive" for the same reasons you do.

**The convergence.** "God created the universe" and "an operator at a higher level of reality initialized a simulation" are the same statement in two vocabularies. Religious intuition may be pointing at something structurally real, described in pre-computational language.

---

*Developed with the assistance of Claude (Anthropic). The founding philosophy and technical evolution are documented in [`inicio.txt`](inicio.txt) and [`CLAUDE.md`](CLAUDE.md).*
