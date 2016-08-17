using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComputeSpawn : Compute {

   public class SourceInfo
    {
        public int instancedParticles;
        public int availableParticles;
        public int timer;

        public SourceInfo(int available)
        {
            this.instancedParticles = 0;
            this.availableParticles = available;
            this.timer = 0;
        }

        public void update(ShaderConstants.Source s)
        {
            timer += 1;
            if(timer % s.spawnPeriod == 0)
            {
                instancedParticles += s.spawnAmount;
                timer = 0;
            }
        }

    }

    
    public ComputeShader spawnShader;

    public int InstancedParticles
    {
        get
        {
            if (sourceInfo == null)
                return 0;
            int sum = 0;
            foreach (SourceInfo s in sourceInfo)
                sum += s.instancedParticles;
            return sum;
        }
    }

    public int AvailableParticles
    {
        get
        {
            if (sourceInfo == null)
                return 0;
            int sum = 0;
            foreach (SourceInfo s in sourceInfo)
                sum += s.availableParticles;
            return sum;
        }
    }

    private int spawnHandle;
    private int maxSourceNumber = 4; //MUST BE MULTIPLE OF BLOCKSIZE!

    //compute buffers for spawning particles
    private ComputeBuffer spawnTimers;
    private ComputeBuffer sources;

    private List<SourceInfo> sourceInfo;
    private List<ShaderConstants.Source> cpuSourcers;

    public override void SetupShader(ParticleManager pm)
    {

        cpuSourcers = new List<ShaderConstants.Source>();
        sourceInfo = new List<SourceInfo>();

        //testing spawn
        ShaderConstants.Source s = new ShaderConstants.Source();
        s.position = new Vector2(-30, 0);
        s.spawnPeriod = 5;
        s.spawnAmount = 10;
        s.velocity1.x = 4f;
        s.velocity2.x = 4f;
        s.velocity1.y = -4f;
        s.velocity2.y = 4f;
        s.lifeStart = -5f;

        cpuSourcers.Add(s);


        sourceInfo.Add(new SourceInfo(1000000));

        spawnHandle = spawnShader.FindKernel("Spawn");
        ShaderConstants.Source[] initial_sources = extendSources(cpuSourcers);


        maxSourceNumber = initial_sources.Length;

        sources = new ComputeBuffer(maxSourceNumber, ShaderConstants.SOURCE_STRIDE);
        spawnTimers = new ComputeBuffer(maxSourceNumber, ShaderConstants.UINT_STRIDE);

        sources.SetData(initial_sources);

        int[] izeros = new int[maxSourceNumber];
        spawnTimers.SetData(izeros);


        spawnShader.SetBuffer(spawnHandle, "positions", pm.positions);
        spawnShader.SetBuffer(spawnHandle, "velocities", pm.velocities);
        spawnShader.SetBuffer(spawnHandle, "properties", pm.properties);

        spawnShader.SetBuffer(spawnHandle, "sources", sources);
        spawnShader.SetBuffer(spawnHandle, "spawnTimers", spawnTimers);
    }

    /*
     * Need to extend soucres so that it's a multiple of the blocksize
     */
    private static ShaderConstants.Source[] extendSources(List<ShaderConstants.Source> list)
    {
        int length = ShaderConstants.SPAWN_BLOCKSIZE_X * Mathf.CeilToInt((float)list.Count / ShaderConstants.SPAWN_BLOCKSIZE_X);
        ShaderConstants.Source[] sload = new ShaderConstants.Source[length];
        for (int i = 0; i < list.Count; i++)
            sload[i] = list[i];
        for (int i = list.Count; i < length; i++)
            sload[i].spawnPeriod = 0x7FFFFFFF;
        return sload;
    }

    public override void UpdatePostIntegrate(int nx)
    {
        int ns = Mathf.CeilToInt((float)maxSourceNumber / ShaderConstants.SPAWN_BLOCKSIZE_X);        
        spawnShader.Dispatch(spawnHandle, ns, 1, 1);

        //This code mimicks the GPU code
        //Need to keep track of how many particles I've created, etc.

        for (int i = 0; i < sourceInfo.Count; i++)
        {
            sourceInfo[i].update(cpuSourcers[i]);
        }
    }

    public override void ReleaseBuffers()
    {
        sources.Release();
        spawnTimers.Release();
    }
}
