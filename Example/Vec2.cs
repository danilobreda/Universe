namespace Universo;

public struct Vec2(float x, float y)
{
    public float X = x;
    public float Y = y;

    public readonly float LengthSquared() => X * X + Y * Y;

    public readonly float Length() => MathF.Sqrt(LengthSquared());

    public readonly Vec2 Normalized()
    {
        var len = Length();
        return len > 0 ? new Vec2(X / len, Y / len) : default;
    }

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator *(Vec2 v, float s) => new(v.X * s, v.Y * s);
    public static Vec2 operator *(float s, Vec2 v) => v * s;
    public static Vec2 operator /(Vec2 v, float s) => new(v.X / s, v.Y / s);

    public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;

    public readonly System.Numerics.Vector2 ToVector2() => new(X, Y);
}
