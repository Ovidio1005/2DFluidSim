using System.Numerics;

namespace _2DFluidSim.Fluid;
internal struct Particle
{
    public float Mass;
    public Vector2 Position;
    public Vector2 Velocity;

    public Particle() { }
    public Particle(Particle particle)
    {
        Mass = particle.Mass;
        Position = particle.Position;
        Velocity = particle.Velocity;
    }
    public Particle(float mass, Vector2 position, Vector2 velocity)
    {
        Mass = mass;
        Position = position;
        Velocity = velocity;
    }

    public static Particle ApplyAcceleration(Particle particle, Vector2 acceleration, float dt) => new Particle(particle.Mass, particle.Position, particle.Velocity + acceleration * dt);
    public static Particle ApplyForce(Particle particle, Vector2 force, float dt) => ApplyAcceleration(particle, force / particle.Mass, dt);
    public static Particle Move(Particle particle, float dt) => new Particle(particle.Mass, particle.Position + particle.Velocity * dt, particle.Velocity);

    public static float Distance(Particle p1, Particle p2) => Vector2.Distance(p1.Position, p2.Position);
    public static Vector2 Offset(Particle p1, Particle p2) => p2.Position - p1.Position;
    public static float DeltaV(Particle p1, Particle p2) => Vector2.Distance(p1.Velocity, p2.Velocity);
    public static Vector2 RelativeVelocity(Particle p1, Particle p2) => p2.Velocity - p1.Velocity;

    public Particle ApplyAcceleration(Vector2 acceleration, float dt) => ApplyAcceleration(this, acceleration, dt);
    public Particle ApplyForce(Vector2 force, float dt) => ApplyForce(this, force, dt);
    public Particle Move(float dt) => Move(this, dt);

    public float Distance(Particle other) => Distance(this, other);
    public Vector2 Offset(Particle other) => Offset(this, other);
    public float DeltaV(Particle other) => DeltaV(this, other);
    public Vector2 RelativeVelocity(Particle other) => RelativeVelocity(this, other);
}
