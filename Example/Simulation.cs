namespace Universo;

public class Simulation
{
    public List<Particle> Particles { get; }
    public Field Field { get; } = new();
    public float DeltaTime { get; set; } = 0.01f;
    public long Tick { get; private set; }
    public float WorldSize { get; }

    public Simulation(int particleCount, float worldSize, int? seed = null)
    {
        WorldSize = worldSize;
        Physics.WorldSize = worldSize;
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        Particles = new List<Particle>(particleCount);

        // ─── BIG BANG ────────────────────────────────────────────────────────
        // Tudo concentrado num pequeno disco no centro, expandindo radialmente.
        // O universo toroidal eventualmente se preenche, e a dispersão + damping
        // de colisão (Seção 8.2) faz a temperatura cair → recombinação.
        var center = new Vec2(worldSize / 2f, worldSize / 2f);
        float bangRadius = worldSize * 0.06f;
        float bangSpeed = 22f;

        for (int i = 0; i < particleCount; i++)
        {
            // Posição uniforme em disco (sqrt evita acúmulo no centro)
            double angle = rng.NextDouble() * Math.PI * 2;
            double r = Math.Sqrt(rng.NextDouble()) * bangRadius;
            var offset = new Vec2(
                (float)(Math.Cos(angle) * r),
                (float)(Math.Sin(angle) * r));
            var pos = center + offset;

            // Velocidade radial pra fora + jitter (sem isso vira shell perfeitamente simétrica)
            var radial = offset.LengthSquared() > 1e-4f ? offset.Normalized() : new Vec2(1, 0);
            float scale = (float)(0.7 + rng.NextDouble() * 0.6); // 0.7 a 1.3
            var vel = radial * (bangSpeed * scale);
            vel += new Vec2(
                (float)(rng.NextDouble() * 4 - 2),
                (float)(rng.NextDouble() * 4 - 2));

            // Duas populações alternadas. Não são "prótons" e "elétrons" no código —
            // são apenas pesadas-positivas e leves-negativas. O conceito "átomo"
            // emerge depois, do estado dinâmico, não da etiqueta.
            bool heavy = (i % 2) == 0;
            float mass = heavy ? 8f : 0.4f;     // razão 20:1
            float charge = heavy ? 1f : -1f;

            Particles.Add(new Particle
            {
                Id = i,
                Position = pos,
                Velocity = vel,
                BaseMass = mass,
                Charge = charge,
                InteractionHiggs = 0f
            });
        }
    }

    public void Step()
    {
        float dt = DeltaTime;
        int n = Particles.Count;

        // ─── 1. Forças de longo alcance: gravidade + eletricidade ──────────
        // N² direto, paralelo (cada partícula só LÊ as outras, escreve só na própria velocidade)
        Parallel.For(0, n, i =>
        {
            var p = Particles[i];
            var totalForce = new Vec2();
            for (int j = 0; j < n; j++)
            {
                if (i == j) continue;
                var other = Particles[j];
                totalForce += Physics.Gravity(p, other, Field);
                totalForce += Physics.Electric(p, other);
            }
            float mass = Physics.Mass(p, Field);
            p.Velocity += totalForce / mass * dt;
        });

        // ─── 2. Damping radial em encontros próximos (Seção 8.2) ───────────
        // "perda de energia em impactos": só a componente RADIAL da velocidade
        // relativa é dissipada, e somente quando as partículas se APROXIMAM.
        // Consequência: órbitas circulares são estáveis pra sempre (radialSpeed=0),
        // órbitas elípticas circularizam, encontros frontais perdem energia.
        // Sequencial pra evitar race conditions; com N=400 é barato (~80k iter).
        float radius = Physics.CollisionRadius;
        float radSq = radius * radius;
        float damp = Physics.CollisionDamping;
        for (int i = 0; i < n; i++)
        {
            var a = Particles[i];
            for (int j = i + 1; j < n; j++)
            {
                var b = Particles[j];
                var dir = Physics.MinImage(b.Position - a.Position);
                float dSq = dir.LengthSquared();
                if (dSq > radSq || dSq < 1e-6f) continue;

                float dist = MathF.Sqrt(dSq);
                var norm = dir / dist;
                var relVel = b.Velocity - a.Velocity;
                float radialSpeed = Vec2.Dot(relVel, norm);

                if (radialSpeed >= 0) continue; // se afastando: sem dissipação

                var impulse = norm * (radialSpeed * damp);
                a.Velocity += impulse * 0.5f;
                b.Velocity -= impulse * 0.5f;
            }
        }

        // ─── 3. Integração + wrap toroidal ─────────────────────────────────
        float ws = WorldSize;
        for (int i = 0; i < n; i++)
        {
            var p = Particles[i];
            Physics.ClampSpeed(p);
            p.Position += p.Velocity * dt;
            p.Position = new Vec2(
                ((p.Position.X % ws) + ws) % ws,
                ((p.Position.Y % ws) + ws) % ws);
        }

        Tick++;
    }
}
