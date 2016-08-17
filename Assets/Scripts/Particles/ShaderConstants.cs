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
    public const int SOURCE_STRIDE = 2 * 3 * FLOAT_STRIDE + FLOAT_STRIDE + 1 * UINT_STRIDE + 1 * INT_STRIDE;
    public const int PROP_STRIDE = UINT_STRIDE + FLOAT_STRIDE + 4 * FLOAT_STRIDE;
    public const int ATTRACTOR_STRIDE = 2 * FLOAT_STRIDE + FLOAT_STRIDE;
    public const int WALL_STRIDE = 2 * 2 * FLOAT_STRIDE;
    public const int SPAWN_BLOCKSIZE_X = 4;
    public const int SPAWN_BLOCKSIZE_Y = 128;
    public const int PARTICLE_BLOCK_SIZE = 256;
    public const int QUAD_STRIDE = 12;

    public struct Source
    {
        public Vector2 position;
        public Vector2 velocity1;
        public Vector2 velocity2;
        public float lifeStart;
        public uint spawnPeriod;
        public int spawnAmount;

    }

    public struct Prop
    {
        public uint alive;
        public float life;
        public Vector4 color;

        public Prop(uint alive, float life, Vector4 color)
        {
            this.alive = alive;
            this.life = life;
            this.color = color;
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
}
