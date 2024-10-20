using _2DFluidSim.Fields;
using System.Numerics;

namespace _2DFluidSim.Fluid;
internal class FluidBox {
    private Random RNG = new Random();

    private PointDensityMapper PMapper;
    private FieldDifferentiator Differentiator = new();

    public readonly float Width;
    public readonly float Height;

    public float SimulationTime { get; private set; } = 0;

    // kg/m^2
    public float FluidDensity = 1000;
    public readonly float ParticleDensity = 100;
    public float ParticleMass {
        get => FluidDensity / (ParticleDensity * ParticleDensity);
    }

    public float PressureSensitivity = 1000;
    public float Damp = 1f;
    public float Noise = 0.01f;
    public Vector2 Gravity = new Vector2(0, -9.81f);

    public float WallRadius = 0.001f;

    public float TimeStep = 0.01f;

    public List<Particle> Particles = new List<Particle>();

    public FluidBox(float width, float height, float particleDensity = 100) {
        Width = width;
        Height = height;
        ParticleDensity = particleDensity;

        float pMapStep = 1 / ParticleDensity;
        int pMapResX = (int) (width / pMapStep) + 2;
        int pMapResY = (int) (height / pMapStep) + 2;
        Vector2 pMapCenter = new(width / 2, height / 2);

        PMapper = new(pMapResX, pMapResY, pMapStep, pMapCenter, 1);
    }

    public void ClearParticles() => Particles.Clear();

    public void AddFluid(Vector2 blCorner, Vector2 trCorner) {
        float step = 1 / ParticleDensity;
        float pMass = ParticleMass;

        for(float x = blCorner.X; x <= trCorner.X; x += step) {
            for(float y = blCorner.Y; y <= trCorner.Y; y += step) {
                Particles.Add(new(pMass, new(x, y), Vector2.Zero));
            }
        }
    }

    public void RemoveFluid(Vector2 blCorner, Vector2 trCorner) {
        List<Particle> particlesToRemove = new();
        for(int i = 0; i < Particles.Count; i++) {
            Vector2 p = Particles[i].Position;
            if(p.X >= blCorner.X && p.X <= trCorner.X && p.Y >= blCorner.Y && p.Y <= trCorner.Y) particlesToRemove.Add(Particles[i]);
        }

        foreach(Particle p in particlesToRemove) Particles.Remove(p);
    }

    public void Step() {
        ApplyGravity();
        ApplyPressure();
        ApplyNoise();
        ApplyDampening();
        MoveParticles();
        //DebugCheck();
        SimulationTime += TimeStep;
    }

    public float[,] Map(float step, float margin) {
        int resX = (int) ((Width + 2 * margin) / step);
        int resY = (int) ((Height + 2 * margin) / step);
        Vector2 center = new(Width / 2, Height / 2);
        float expectedDensity = (float) Math.Pow(step * ParticleDensity, 2);

        PointDensityMapper mapper = new(resX, resY, step, center, expectedDensity);
        return mapper.Map(Particles.Select(p => p.Position).ToArray());
    }

    private void ApplyGravity() {
        for(int i = 0; i < Particles.Count; i++) {
            Particles[i] = Particles[i].ApplyAcceleration(Gravity, TimeStep);
        }
    }

    private void ApplyNoise() {
        for(int i = 0; i < Particles.Count; i++) {
            Particles[i] = Particles[i].ApplyAcceleration(GetRandomNormal() * Noise,  TimeStep);
        }
    }

    private void ApplyPressure() {
        float[,] pressureField = PMapper.Map(Particles.Select(p => p.Position).ToArray());

        int width = pressureField.GetLength(0);
        int height = pressureField.GetLength(1);

        float[,] flooredPressureField = new float[width, height];
        for(int x = 0; x < width; x++) {
            for(int y = 0; y < height; y++) {
                flooredPressureField[x, y] = pressureField[x, y] < 1 ? 1 : pressureField[x, y];
            }
        }

        Vector2[,] diff = Differentiator.Differentiate(flooredPressureField);

        for(int i = 0; i < Particles.Count; i++) {
            (int x, int y) = PMapper.Pixel(Particles[i].Position);
            if(x >= 0 && x < width && y >= 0 && y < height) {
                Particles[i] = Particles[i].ApplyAcceleration(-1 * diff[x, y] * pressureField[x, y] * pressureField[x, y] * PressureSensitivity, TimeStep);
            } else {
                Console.WriteLine($"DEGUB WARNING: particle at ({Particles[i].Position.X:F5}, {Particles[i].Position.Y:F5}) outside of pressure field");
            }
        }
    }

    private void ApplyDampening() {
        for(int i = 0; i < Particles.Count; i++) {
            Vector2 dampAcceleration = -1 * Particles[i].Velocity * Damp;
            Particles[i] = Particles[i].ApplyAcceleration(dampAcceleration, TimeStep);
        }
    }

    private void MoveParticles() {
        for(int i = 0; i < Particles.Count; i++) {
            Particles[i] = Particles[i].Move(TimeStep);

            if(Particles[i].Position.X < WallRadius) {
                Particle p = new(Particles[i]);
                p.Position.X = WallRadius;
                p.Velocity.X = Math.Max(p.Velocity.X, 0);

                Particles[i] = p;
            } else if(Particles[i].Position.X > Width - WallRadius) {
                Particle p = new(Particles[i]);
                p.Position.X = Width - WallRadius;
                p.Velocity.X = Math.Min(p.Velocity.X, 0);

                Particles[i] = p;
            }

            if(Particles[i].Position.Y < WallRadius) {
                Particle p = new(Particles[i]);
                p.Position.Y = WallRadius;
                p.Velocity.Y = Math.Max(p.Velocity.Y, 0);

                Particles[i] = p;
            } else if(Particles[i].Position.Y > Height - WallRadius) {
                Particle p = new(Particles[i]);
                p.Position.Y = Height - WallRadius;
                p.Velocity.Y = Math.Min(p.Velocity.Y, 0);

                Particles[i] = p;
            }
        }
    }

    private Vector2 GetRandomNormal() {
        double angle = RNG.NextDouble() * 2 * Math.PI;
        (double y, double x) = Math.SinCos(angle);
        return new Vector2((float) x, (float) y);
    }

    private int ParticleCount = -1;
    private void DebugCheck() {
        int count = Particles.Count();
        if(ParticleCount >= 0 && ParticleCount != count) Console.WriteLine($"WARNING! Used to be {ParticleCount} particles, now there are {count}");
        ParticleCount = count;

        foreach(Particle p in Particles) {
            if(p.Position.X < 0 || p.Position.X > Width || p.Position.Y < 0 || p.Position.Y > Height) {
                Console.WriteLine($"WARNING! Particle at ({p.Position.X}, {p.Position.Y}), outside of bounding box");
            }
        }
    }
}
