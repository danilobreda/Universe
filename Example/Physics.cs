namespace Universo;

public static class Physics
{
    // Constantes fundamentais — você é deus, edite aqui
    public const float G = 0.05f;                  // gravidade (fraca)
    public const float K = 1.0f;                   // eletricidade (forte localmente)
    public const float SpeedLimit = 50f;           // limite tipo relativístico (Seção 8.5)
    public const float CollisionRadius = 3f;      // raio de "encontro próximo"
    public const float CollisionDamping = 0.45f;  // fração da componente radial dissipada (Seção 8.2)

    // Tamanho do universo (toroidal). Setado por Simulation no construtor.
    public static float WorldSize = 500f;

    public static float Mass(Particle p, Field field)
    {
        return p.BaseMass + field.GetIntensity(p.Position) * p.InteractionHiggs;
    }

    public static Vec2 Gravity(Particle a, Particle b, Field field)
    {
        var dir = MinImage(b.Position - a.Position);
        float distSq = dir.LengthSquared() + 0.01f;
        float force = G * Mass(a, field) * Mass(b, field) / distSq;
        return dir.Normalized() * force;
    }

    public static Vec2 Electric(Particle a, Particle b)
    {
        var dir = MinImage(b.Position - a.Position);
        float distSq = dir.LengthSquared() + 0.01f;
        float force = K * a.Charge * b.Charge / distSq;
        return dir.Normalized() * force;
    }

    public static void ClampSpeed(Particle p)
    {
        var speedSq = p.Velocity.LengthSquared();
        if (speedSq > SpeedLimit * SpeedLimit)
            p.Velocity = p.Velocity.Normalized() * SpeedLimit;
    }

    /// <summary>
    /// Convenção de mínima imagem para um vetor diferença em universo toroidal.
    /// Garante que a "menor distância" entre dois pontos respeite as bordas periódicas.
    /// </summary>
    public static Vec2 MinImage(Vec2 d)
    {
        float w = WorldSize;
        float h = w * 0.5f;
        if (d.X >  h) d.X -= w;
        if (d.X < -h) d.X += w;
        if (d.Y >  h) d.Y -= w;
        if (d.Y < -h) d.Y += w;
        return d;
    }
}
