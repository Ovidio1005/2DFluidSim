using System.Numerics;

namespace _2DFluidSim.Fluid;
internal class FluidBox {
    // TODO:
    //   Particles are currently ending up overlapping and then being apparently stuck together for some reason.
    //   Basically everything collapses in on itself. Figure out why it's happening and how to fix it.
    //   That's quite weird though. If particles don't overlap perfectly, they should be pushed out of eachother, and
    //   if they do, I should get a divide by zero error. Unless NaN or Infinity?
    protected const int PARTICLE_ARRAY_STEP = 65536;
    protected const float INTERACTION_THRESHOLD = 0.01f;
    protected static Random RNG = new Random();

    public float InteractionRadius { get; private set; }
    public int ParticleCount { get; private set; } = 0;

    public float Width { get => TRCorner.X - BLCorner.X; }
    public float Height { get => TRCorner.Y - BLCorner.Y; }

    public float SimulationTime { get; private set; } = 0;

    public float TimeStep = 0.01f;
    public float Viscosity = 1;
    public float Dampening = 0.5f;
    public float ParticleRadius = 0.01f;
    public float ParticleMass = 1; // Unused for now
    public Vector2 Gravity = new(0, -9.81f);

    Vector2 BLCorner;
    Vector2 TRCorner;

    private float _interactionRange = 0.1f;
    public float InteractionRange {
        get => _interactionRange;
        set {
            _interactionRange = value;
            InteractionRadius = (float) (_interactionRange * Math.Sqrt(-1 * Math.Log(INTERACTION_THRESHOLD)));
        }
    }

    protected Particle[] Particles = new Particle[65536];

    public FluidBox(Vector2 blCorner, Vector2 trCorner, float particleRadius) {
        BLCorner = blCorner;
        TRCorner = trCorner;
        ParticleRadius = particleRadius;
    }

    public FluidBox(float width, float height, float particleRadius) : this(new Vector2(0, 0), new Vector2(width, height), particleRadius) { }

    public List<Particle> GetParticles() => Particles.Take(ParticleCount).ToList();

    public void AddFluid(Vector2 bl, Vector2 tr) {
        for(float x = bl.X; x <= tr.X; x += ParticleRadius) {
            for(float y = bl.Y; y <= tr.Y; y += ParticleRadius) {
                if(ParticleCount >= Particles.Length) ExpandParticleArray();

                Particles[ParticleCount] = new(ParticleMass, new(x, y), Vector2.Zero);
                ParticleCount++;
            }
        }
    }

    public void Step() {
        ApplyViscosity();
        ApplyGravity();
        ApplyDampening();
        ////MoveParticles();
        ////HandleCollisions();
        ////MoveAndHandleCollisions();
        //MoveAndHandleCollisions2();
        //// TODO remove
        ////DebugCheck();
        MoveParticles2();
        HandleCollisions2();
        HandleCollisions2();
        HandleCollisions2();
        HandleCollisions2();
        SimulationTime += TimeStep;
    }

    protected void ApplyViscosity() {
        for(int i = 0; i < ParticleCount; i++) {
            for(int j = 0; j < ParticleCount; j++) {
                if(i == j) continue;

                float r = Vector2.Distance(Particles[i].Position, Particles[j].Position);
                if(r < InteractionRadius) {
                    Vector2 avgVelocity = (Particles[i].Velocity + Particles[j].Velocity) / 2;
                    float gauss = Gauss(r);

                    Particles[i].Velocity = Vector2.Lerp(Particles[i].Velocity, avgVelocity, gauss * Viscosity * TimeStep);
                    Particles[j].Velocity = Vector2.Lerp(Particles[j].Velocity, avgVelocity, gauss * Viscosity * TimeStep);
                }
            }
        }
    }

    protected void ApplyDampening() {
        for(int i = 0; i < ParticleCount; i++) {
            Particles[i].Velocity *= 1 - (Dampening * TimeStep);
        }
    }

    protected void ApplyGravity() {
        for(int i = 0; i < ParticleCount; i++) {
            Particles[i].Velocity += Gravity * TimeStep;
        }
    }

    protected void MoveParticles() {
        for(int i = 0; i < ParticleCount; i++) {
            Particles[i].Position += Particles[i].Velocity * TimeStep;

            if(Particles[i].Position.X < BLCorner.X) {
                Particles[i].Position.X = BLCorner.X;
                Particles[i].Velocity.X = Math.Max(0, Particles[i].Velocity.X);
            } else if(Particles[i].Position.X > TRCorner.X) {
                Particles[i].Position.X = TRCorner.X;
                Particles[i].Velocity.X = Math.Min(0, Particles[i].Velocity.X);
            }

            if(Particles[i].Position.Y < BLCorner.Y) {
                Particles[i].Position.Y = BLCorner.Y;
                Particles[i].Velocity.Y = Math.Max(0, Particles[i].Velocity.Y);
            } else if(Particles[i].Position.Y > TRCorner.Y) {
                Particles[i].Position.Y = TRCorner.Y;
                Particles[i].Velocity.Y = Math.Min(0, Particles[i].Velocity.Y);
            }
        }
    }

    protected void MoveParticles2() {
        for(int i = 0; i < ParticleCount; i++) {
            Particles[i].Position += Particles[i].Velocity * TimeStep;
        }
    }

    protected void HandleCollisions() {
        for(int i = 0; i < ParticleCount; i++) {
            for(int j = 0; j < ParticleCount; j++) {
                if(i == j) continue;

                Vector2 offset = Particles[j].Position - Particles[i].Position;
                float r = offset.Length();

                if(r < ParticleRadius) {
                    Particles[i].Position = Particles[j].Position - ((ParticleRadius / r) * offset);

                    Vector2 deltaV = Project(Particles[i].Velocity - Particles[j].Velocity, offset);
                    Particles[i].Velocity -= deltaV;
                    Particles[j].Velocity += deltaV;
                }
            }
        }
    }

    protected void HandleCollisions2() {
        List<(int index, float priority)> priorities = new();

        for(int i = 0; i < ParticleCount; i++) {
            float pr = 0;
            for(int j = 0; j < ParticleCount; j++) {
                if(i == j) continue;

                float r = Vector2.Distance(Particles[i].Position, Particles[j].Position);

                //if(r < ParticleRadius) {
                //    pr += ParticleRadius - r;
                //}

                pr += 1 / r;
            }

            priorities.Add((i, pr));
        }

        foreach(int i in priorities.OrderByDescending(p => p.priority).Select(p => p.index)) {
            for(int j = 0; j < ParticleCount; j++) {
                if(i == j) continue;

                Vector2 offset = Particles[j].Position - Particles[i].Position;
                float r = offset.Length();

                if(r < ParticleRadius) {
                    if(r <= 0 || !float.IsFinite(r)) { // stupid fucking C# and your stupid NaNs and infinities, if I divide by 0 I expect an error >:(
                        Particles[i].Position = Particles[j].Position - (ParticleRadius * RandomNormal());
                        continue;
                    }

                    Particles[i].Position = Particles[j].Position - ((ParticleRadius / r) * offset);

                    Vector2 deltaV = Project(Particles[i].Velocity - Particles[j].Velocity, offset);
                    Particles[i].Velocity -= deltaV / 2;
                    Particles[j].Velocity += deltaV / 2;
                }
            }

            if(Particles[i].Position.X < BLCorner.X) {
                Particles[i].Position.X = BLCorner.X;
                Particles[i].Velocity.X = Math.Max(0, Particles[i].Velocity.X);
            } else if(Particles[i].Position.X > TRCorner.X) {
                Particles[i].Position.X = TRCorner.X;
                Particles[i].Velocity.X = Math.Min(0, Particles[i].Velocity.X);
            }

            if(Particles[i].Position.Y < BLCorner.Y) {
                Particles[i].Position.Y = BLCorner.Y;
                Particles[i].Velocity.Y = Math.Max(0, Particles[i].Velocity.Y);
            } else if(Particles[i].Position.Y > TRCorner.Y) {
                Particles[i].Position.Y = TRCorner.Y;
                Particles[i].Velocity.Y = Math.Min(0, Particles[i].Velocity.Y);
            }
        }
    }

    protected void MoveAndHandleCollisions() {
        for(int i = 0; i < ParticleCount; i++) {
            Particles[i].Position += Particles[i].Velocity * TimeStep;

            if(Particles[i].Position.X < BLCorner.X) {
                Particles[i].Position.X = BLCorner.X;
                Particles[i].Velocity.X = Math.Max(0, Particles[i].Velocity.X);
            } else if(Particles[i].Position.X > TRCorner.X) {
                Particles[i].Position.X = TRCorner.X;
                Particles[i].Velocity.X = Math.Min(0, Particles[i].Velocity.X);
            }

            if(Particles[i].Position.Y < BLCorner.Y) {
                Particles[i].Position.Y = BLCorner.Y;
                Particles[i].Velocity.Y = Math.Max(0, Particles[i].Velocity.Y);
            } else if(Particles[i].Position.Y > TRCorner.Y) {
                Particles[i].Position.Y = TRCorner.Y;
                Particles[i].Velocity.Y = Math.Min(0, Particles[i].Velocity.Y);
            }

            for(int j = 0; j < ParticleCount; j++) {
                if(i == j) continue;

                Vector2 offset = Particles[j].Position - Particles[i].Position;
                float r = offset.Length();

                if(r < ParticleRadius) {
                    Particles[i].Position = Particles[j].Position - ((ParticleRadius / r) * offset);

                    Vector2 deltaV = Project(Particles[i].Velocity - Particles[j].Velocity, offset);
                    Particles[i].Velocity -= deltaV;
                    Particles[j].Velocity += deltaV;
                }
            }
        }
    }

    protected void MoveAndHandleCollisions2() { // No longer conserves momentum
        for(int i = 0; i < ParticleCount; i++) {
            Particles[i].Position += Particles[i].Velocity * TimeStep;

            if(Particles[i].Position.X < BLCorner.X) {
                Particles[i].Position.X = BLCorner.X;
                Particles[i].Velocity.X = Math.Max(0, Particles[i].Velocity.X);
            } else if(Particles[i].Position.X > TRCorner.X) {
                Particles[i].Position.X = TRCorner.X;
                Particles[i].Velocity.X = Math.Min(0, Particles[i].Velocity.X);
            }

            if(Particles[i].Position.Y < BLCorner.Y) {
                Particles[i].Position.Y = BLCorner.Y;
                Particles[i].Velocity.Y = Math.Max(0, Particles[i].Velocity.Y);
            } else if(Particles[i].Position.Y > TRCorner.Y) {
                Particles[i].Position.Y = TRCorner.Y;
                Particles[i].Velocity.Y = Math.Min(0, Particles[i].Velocity.Y);
            }

            for(int j = 0; j < ParticleCount; j++) {
                if(i == j) continue;

                Vector2 offset = Particles[j].Position - Particles[i].Position;
                float r = offset.Length();

                if(r < ParticleRadius) {
                    if(r <= 0 || float.IsNaN(r) || float.IsInfinity(r)) { // stupid fucking C# and your stupid NaNs and infinities, if I divide by 0 I expect an error >:(
                        Particles[i].Position = Particles[j].Position - (ParticleRadius * RandomNormal());
                        continue;
                    }

                    Particles[i].Position = Particles[j].Position - ((ParticleRadius / r) * offset);

                    Vector2 deltaV = Project(Particles[i].Velocity - Particles[j].Velocity, offset);
                    Particles[i].Velocity -= deltaV;
                    // Removing this line is what makes it not conserve momentum
                    // Particles[j].Velocity += deltaV;
                }
            }
        }
    }

    protected float Gauss(float x) => Gauss(x, InteractionRange);

    protected Vector2 Project(Vector2 v, Vector2 onto) {
        Vector2 direction = Vector2.Normalize(onto);
        return Vector2.Dot(v, direction) * direction;
    }

    protected static float Gauss(float x, float r) => (float) Math.Exp(-(x * x) / (r * r));

    protected static Vector2 RandomNormal() {
        double angle = RNG.NextDouble() * 2 * Math.PI;
        (double y, double x) = Math.SinCos(angle);
        return new Vector2((float) x, (float) y);
    }

    private void ExpandParticleArray() {
        int newLength = Particles.Length + PARTICLE_ARRAY_STEP;
        Particle[] newParticles = new Particle[newLength];
        for(int i = 0; i < Particles.Length; i++) {
            newParticles[i] = Particles[i];
        }
        Particles = newParticles;
    }

    private void DebugCheck() {
        for(int i = 0; i < ParticleCount; i++) {
            Particle p = Particles[i];
            if(
                p.Position.X < BLCorner.X - ParticleRadius ||
                p.Position.X > TRCorner.X + ParticleRadius ||
                p.Position.Y < BLCorner.Y - ParticleRadius ||
                p.Position.Y > TRCorner.Y + ParticleRadius
            ) Console.WriteLine($"\nParticle out of bounds at ({p.Position.X}, {p.Position.Y})!");

            for(int j = 0; j < ParticleCount; j++) {
                if(i == j) continue;
                
                Particle q = Particles[j];
                if(p.Position.X == q.Position.X && p.Position.Y == q.Position.Y) Console.WriteLine($"\nParticles overlapping at ({p.Position.X}, {p.Position.Y})!");
            }
        }
    }
}
