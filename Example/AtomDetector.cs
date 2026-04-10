namespace Universo;

/// <summary>
/// Detector de pares ligados (proto-átomos de hidrogênio).
///
/// PURA OBSERVAÇÃO — não toca em posição, velocidade ou massa de nada.
/// Se você apertar [A] pra esconder o desenho, a simulação roda idêntica.
/// Os átomos estão lá ou não estão; o detector só revela.
///
/// Critérios (todos derivados das variáveis fundamentais do inicio.txt):
///
///   1. VIZINHANÇA MÚTUA — entre cargas opostas, cada um é o mais próximo do outro.
///      Elimina o caso de uma leve passando perto de várias pesadas.
///
///   2. ENERGIA LIGADA — ½·μ·v_rel² + K·q₁·q₂/r < 0
///      Estado clássico ligado: KE no referencial do centro de massa não basta
///      pra escapar do poço de Coulomb.
///
///   3. PERSISTÊNCIA — par satisfaz (1)+(2) por N ticks consecutivos.
///      Filtra encontros transitórios de 3 corpos.
///
///   4. ISOLAMENTO — nenhum terceiro corpo dentro de raio R do par.
///      Distingue "átomo" de "plasma denso" ou "molécula maior".
/// </summary>
public class AtomDetector
{
    public int PersistenceThreshold { get; set; } = 120;
    public float IsolationRadius { get; set; } = 6f;

    private Dictionary<long, int> _pairAge = new();
    private Dictionary<long, int> _nextAge = new();

    public List<(int A, int B)> Atoms { get; } = new();
    public int HydrogenCount => Atoms.Count;
    public int FreeHeavies { get; private set; }
    public int FreeLights { get; private set; }

    public void Update(List<Particle> particles, Field field)
    {
        Atoms.Clear();
        _nextAge.Clear();

        int n = particles.Count;

        // Indices por sinal de carga
        var heavies = new List<int>(n / 2);
        var lights = new List<int>(n / 2);
        for (int i = 0; i < n; i++)
        {
            float c = particles[i].Charge;
            if (c > 0) heavies.Add(i);
            else if (c < 0) lights.Add(i);
        }

        // ── Critério 1: vizinho mais próximo entre cargas opostas (mútuo) ──
        var nearestHeavyOf = new int[n];
        var nearestLightOf = new int[n];
        var bestDistL = new float[n];
        var bestDistH = new float[n];
        Array.Fill(nearestHeavyOf, -1);
        Array.Fill(nearestLightOf, -1);
        Array.Fill(bestDistL, float.MaxValue);
        Array.Fill(bestDistH, float.MaxValue);

        foreach (int li in lights)
        {
            var pl = particles[li].Position;
            foreach (int hi in heavies)
            {
                var dir = Physics.MinImage(particles[hi].Position - pl);
                float dSq = dir.LengthSquared();
                if (dSq < bestDistL[li])
                {
                    bestDistL[li] = dSq;
                    nearestHeavyOf[li] = hi;
                }
                if (dSq < bestDistH[hi])
                {
                    bestDistH[hi] = dSq;
                    nearestLightOf[hi] = li;
                }
            }
        }

        var mutualPairs = new List<(int L, int H)>();
        foreach (int li in lights)
        {
            int hi = nearestHeavyOf[li];
            if (hi == -1) continue;
            if (nearestLightOf[hi] != li) continue;
            mutualPairs.Add((li, hi));
        }

        // ── Critério 2: energia ligada (E_par < 0) ─────────────────────────
        var bound = new List<(int L, int H)>();
        foreach (var (li, hi) in mutualPairs)
        {
            var pl = particles[li];
            var ph = particles[hi];

            var dir = Physics.MinImage(ph.Position - pl.Position);
            float dist = MathF.Sqrt(dir.LengthSquared() + 0.01f);

            float ml = Physics.Mass(pl, field);
            float mh = Physics.Mass(ph, field);
            float mu = (ml * mh) / (ml + mh); // massa reduzida

            var vRel = ph.Velocity - pl.Velocity;
            float kineticRel = 0.5f * mu * vRel.LengthSquared();
            float potential = Physics.K * pl.Charge * ph.Charge / dist; // negativo para cargas opostas

            if (kineticRel + potential < 0)
                bound.Add((li, hi));
        }

        // ── Critério 4: isolamento (sem terceiro corpo dentro de R) ────────
        // (faço antes da persistência pra não poluir o dicionário com pares
        //  em regiões densas que nunca seriam "átomos")
        var isolated = new List<(int L, int H)>();
        float isoSq = IsolationRadius * IsolationRadius;
        foreach (var (li, hi) in bound)
        {
            var pl = particles[li].Position;
            var ph = particles[hi].Position;
            var dir = Physics.MinImage(ph - pl);
            var mid = pl + dir * 0.5f;

            bool ok = true;
            for (int k = 0; k < n; k++)
            {
                if (k == li || k == hi) continue;
                var d = Physics.MinImage(particles[k].Position - mid);
                if (d.LengthSquared() < isoSq) { ok = false; break; }
            }
            if (ok) isolated.Add((li, hi));
        }

        // ── Critério 3: persistência ────────────────────────────────────────
        var atomicSet = new HashSet<int>();
        foreach (var (li, hi) in isolated)
        {
            long key = PairKey(particles[li].Id, particles[hi].Id);
            int age = (_pairAge.TryGetValue(key, out var prev) ? prev : 0) + 1;
            _nextAge[key] = age;

            if (age >= PersistenceThreshold)
            {
                Atoms.Add((li, hi));
                atomicSet.Add(li);
                atomicSet.Add(hi);
            }
        }

        // Trocar buffers (pares que sumiram resetam pra 0 automaticamente)
        (_pairAge, _nextAge) = (_nextAge, _pairAge);

        // Contagens livres (pra HUD)
        FreeHeavies = 0;
        FreeLights = 0;
        foreach (int i in heavies) if (!atomicSet.Contains(i)) FreeHeavies++;
        foreach (int i in lights) if (!atomicSet.Contains(i)) FreeLights++;
    }

    private static long PairKey(int a, int b)
    {
        int lo = Math.Min(a, b), hi = Math.Max(a, b);
        return ((long)lo << 32) | (uint)hi;
    }
}
