using UnityEngine;
using System.Collections;

public static class ShaderConstants
{
    // BEFORE DEBUGGING!!!!!
    //NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE
    //Please make sure these are consistent with what is in the shader
    public const int FLOAT_STRIDE = 4;
    public const int UINT_STRIDE = 4;
    public const int INT_STRIDE = 4;
    public const int QUAD_STRIDE = 12;

    public const int SOURCE_STRIDE = 2 * 3 * FLOAT_STRIDE + FLOAT_STRIDE + 1 * UINT_STRIDE + 1 * INT_STRIDE;
    public const int PROP_STRIDE = 3 * UINT_STRIDE + FLOAT_STRIDE + 4 * FLOAT_STRIDE;
    public const int ATTRACTOR_STRIDE = 2 * FLOAT_STRIDE + FLOAT_STRIDE;
    public const int WALL_STRIDE = 2 * 2 * FLOAT_STRIDE;
    public const int TARGET_STRIDE = 2 * FLOAT_STRIDE + FLOAT_STRIDE;

    public const int SPAWN_BLOCKSIZE_X = 4;
    public const int SPAWN_BLOCKSIZE_Y = 128;
    public const int PARTICLE_BLOCK_SIZE = 256;
    public const int REDUCTION_BLOCKSIZE = 128;


    public const int PARTICLE_STATE_DEAD = 0;
    public const int PARTICLE_STATE_ALIVE = 1;
    public const int PARTICLE_STATE_DYING = 2;

    public const int PARTICLE_MODIFIER_SPAWN = 0;
    public const int PARTICLE_MODIFIER_INTEGRATOR = 1;
    public const int PARTICLE_MODIFIER_TARGET = 2;
    public const int PARTICLE_MODIFIER_PARTICLE = 3;

    public static readonly int[] PARTICLE_STATES = { PARTICLE_STATE_DEAD, PARTICLE_STATE_ALIVE, PARTICLE_STATE_DYING };
    public static readonly int[] PARTICLE_MODIFIERS = { PARTICLE_MODIFIER_SPAWN, PARTICLE_MODIFIER_INTEGRATOR, PARTICLE_MODIFIER_TARGET, PARTICLE_MODIFIER_PARTICLE };

    public struct Source
    {
        public Vector2 position;
        public Vector2 velocity1;
        public Vector2 velocity2;
        public float lifeStart;
        public uint spawnPeriod;
        public int spawnAmount;

        public Source(Vector2 position, Vector2 velocity1, Vector2 velocity2, float lifeStart = 0f, uint spawnPeriod = 60, int spawnAmount = 1)
        {
            this.position = position;
            this.velocity1 = velocity1;
            this.velocity2 = velocity2;
            this.lifeStart = lifeStart;
            this.spawnPeriod = spawnPeriod;
            this.spawnAmount = spawnAmount;
        }

        public Source(Vector2 position, Vector2 velocity, float lifeStart = 0f, uint spawnPeriod = 60, int spawnAmount = 1)
        {
            this.position = position;
            this.velocity1 = velocity;
            this.velocity2 = velocity;
            this.lifeStart = lifeStart;
            this.spawnPeriod = spawnPeriod;
            this.spawnAmount = spawnAmount;
        }

    }

    public struct Prop
    {
        public uint state;
        public uint lastModifier;
        public uint lastModifierIndex;
        public float life;
        public Vector4 color;

        public Prop(uint state, float life, Vector4 color)
        {
            this.state = state;
            this.life = life;
            this.color = color;
            this.lastModifier = 0;
            this.lastModifierIndex = 0;
        }
    }

    public struct Wall
    {
        public Vector2 position;
        public Vector2 norm;

        public Wall(Vector2 position, Vector2 norm)
        {
            this.position = position;
            this.norm = norm;
        }
    }

    public struct Attractor
    {
        public Vector2 position;
        public float magnitude;

        public Attractor(Vector2 position, float magnitude) : this()
        {
            this.position = position;
            this.magnitude = magnitude;
        }
    }

    public struct Target
    {
        public Vector2 position;
        public float radius;

        public Target(Vector2 position, float radius)
        {
            this.position = position;
            this.radius = radius;
        }
    }
}
