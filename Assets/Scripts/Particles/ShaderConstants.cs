using UnityEngine;
using System.Collections;

public static class ShaderConstants
{

    //Please make sure these are consistent with what is in the shader
    public const int FLOAT_STRIDE = 4;
    public const int UINT_STRIDE = 4;
    public const int SOURCE_STRIDE = 2 * 3 * FLOAT_STRIDE + FLOAT_STRIDE + 2 * UINT_STRIDE;
    public const int PROP_STRIDE = UINT_STRIDE + FLOAT_STRIDE;
    public const int ATTRACTOR_STRIDE = 2 * FLOAT_STRIDE + FLOAT_STRIDE;
    public const int SPAWN_BLOCKSIZE_X = 4;
    public const int SPAWN_BLOCKSIZE_Y = 128;
    public const int PARTICLE_BLOCK_SIZE = 256;
    public const int QUAD_STRIDE = 12;

    public struct Source
    {
        public Vector2 position;
        public Vector2 velocity_1;
        public Vector2 velocity_2;
        public float life_start;
        public uint spawn_period;
        public uint spawn_amount;

    }

    public struct Prop
    {
        public uint alive;
        public float life;
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
