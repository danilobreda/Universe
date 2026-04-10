namespace Universo;

/// <summary>
/// Métricas derivadas — NÃO são variáveis do sistema, apenas observações.
/// </summary>
public static class Diagnostics
{
    public static float KineticEnergy(Particle p, Field field)
    {
        var mass = Physics.Mass(p, field);
        return 0.5f * mass * p.Velocity.LengthSquared();
    }

    public static float TotalKineticEnergy(List<Particle> particles, Field field)
    {
        float total = 0;
        foreach (var p in particles)
            total += KineticEnergy(p, field);
        return total;
    }

    public static float Temperature(List<Particle> particles)
    {
        if (particles.Count == 0) return 0;
        float sum = 0;
        foreach (var p in particles)
            sum += p.Velocity.LengthSquared();
        return sum / particles.Count;
    }
}
