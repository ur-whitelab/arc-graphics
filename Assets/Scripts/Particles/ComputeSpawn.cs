using UnityEngine;
using System.Collections;

public class ComputeSpawn : Compute {

    private int _spawnHandle;
    public ComputeShader spawnShader;
    private int _maxSourceNumber = 1;

    //compute buffers for spawning particles
    private ComputeBuffer _spawnTimers;
    private ComputeBuffer _sources;

    //bookkeeping buffers
    private ComputeBuffer _sourceCount; //Should be SPAWN_BLOCKSIZE_X dimension

    public override void SetupShader(ParticleManager pm)
    {

        _spawnHandle = spawnShader.FindKernel("Spawn");

        _spawnTimers = new ComputeBuffer(_maxSourceNumber, ShaderConstants.UINT_STRIDE);
        _sourceCount = new ComputeBuffer(ShaderConstants.SPAWN_BLOCKSIZE_X, ShaderConstants.UINT_STRIDE);
        _sources = new ComputeBuffer(_maxSourceNumber, ShaderConstants.SOURCE_STRIDE);

        //initialize source data
        ShaderConstants.Source[] sources = new ShaderConstants.Source[_maxSourceNumber];
        for (int i = 0; i < _maxSourceNumber; i++)
            sources[i].spawn_period = 0x7FFFFFFF;
        sources[0].spawn_period = 60;
        sources[0].spawn_amount = 10;
        sources[0].velocity_1.x = -1f;
        sources[0].velocity_2.x = 1f;
        sources[0].velocity_1.y = -2f;
        sources[0].velocity_2.y = -2f;
        sources[0].life_start = -5f;
        _sources.SetData(sources);

        uint[] izeros = new uint[_maxSourceNumber];
        _spawnTimers.SetData(izeros);


        izeros = new uint[ShaderConstants.SPAWN_BLOCKSIZE_X];
        _sourceCount.SetData(izeros);


        spawnShader.SetBuffer(_spawnHandle, "positions", pm._positions);
        spawnShader.SetBuffer(_spawnHandle, "velocities", pm._velocities);
        spawnShader.SetBuffer(_spawnHandle, "properties", pm._properties);

        spawnShader.SetBuffer(_spawnHandle, "sources", _sources);
        spawnShader.SetBuffer(_spawnHandle, "spawnTimers", _spawnTimers);
        spawnShader.SetBuffer(_spawnHandle, "sourceCount", _sourceCount);
    }

    public override void UpdatePostIntegrate(int nx)
    {
        nx = Mathf.CeilToInt((float)_maxSourceNumber / ShaderConstants.SPAWN_BLOCKSIZE_X);
        spawnShader.Dispatch(_spawnHandle, nx, 1, 1);
    }

    public override void ReleaseBuffers()
    {
        _sources.Release();
        _spawnTimers.Release();
        _sourceCount.Release();
    }
}
