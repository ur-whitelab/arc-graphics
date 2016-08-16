using UnityEngine;
using System.Collections;

public class ComputeSpawn : Compute {

    private int spawnHandle;
    public ComputeShader spawnShader;
    private int maxSourceNumber = 4; //MUST BE MULTIPLE OF BLOCKSIZE!

    //compute buffers for spawning particles
    private ComputeBuffer spawnTimers;
    private ComputeBuffer sources;

    public override void SetupShader(ParticleManager pm)
    {

        spawnHandle = spawnShader.FindKernel("Spawn");

        spawnTimers = new ComputeBuffer(maxSourceNumber, ShaderConstants.UINT_STRIDE);
        sources = new ComputeBuffer(maxSourceNumber, ShaderConstants.SOURCE_STRIDE);

        //initialize source data
        ShaderConstants.Source[] initial_sources = new ShaderConstants.Source[maxSourceNumber];
        for (int i = 0; i < maxSourceNumber; i++)
            initial_sources[i].spawn_period = 0x7FFFFFFF;
        initial_sources[0].position = new Vector2(2, -3);
        initial_sources[0].spawn_period = 20;
        initial_sources[0].spawn_amount = 8;
        initial_sources[0].velocity_1.x = -2f;
        initial_sources[0].velocity_2.x = 2f;
        initial_sources[0].velocity_1.y = -4f;
        initial_sources[0].velocity_2.y = -4f;
        initial_sources[0].life_start = -5f;
        sources.SetData(initial_sources);

        int[] izeros = new int[maxSourceNumber];
        spawnTimers.SetData(izeros);


        spawnShader.SetBuffer(spawnHandle, "positions", pm.positions);
        spawnShader.SetBuffer(spawnHandle, "velocities", pm.velocities);
        spawnShader.SetBuffer(spawnHandle, "properties", pm.properties);

        spawnShader.SetBuffer(spawnHandle, "sources", sources);
        spawnShader.SetBuffer(spawnHandle, "spawnTimers", spawnTimers);
    }

    public override void UpdatePostIntegrate(int nx)
    {
        int ns = Mathf.CeilToInt((float)maxSourceNumber / ShaderConstants.SPAWN_BLOCKSIZE_X);
        //only have 1 group for y, because the SPAWN_BLOCKSIZE_Y will spread out over particles
        spawnShader.Dispatch(spawnHandle, ns, 1, 1);
    }

    public override void ReleaseBuffers()
    {
        sources.Release();
        spawnTimers.Release();
    }
}
