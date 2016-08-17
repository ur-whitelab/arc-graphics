using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleManager : MonoBehaviour {

    public float TimeStep = 0.01f;
    public float ParticleLifeEnd = 25f;
    private float ParticleDiameter = 1.0f;

    private List<Compute> computes;

    private World world;
    public ComputeShader integrateShader;
    public Material particleMaterial;

    public const int ParticleNumber = 262144; //2^18. Must be multiple of 256 (2^8) due to particle reduction blocksize

    //compute buffers for particle information
    public ComputeBuffer positions;
    public ComputeBuffer lastPositions;
    public ComputeBuffer velocities;
    public ComputeBuffer forces;
    public ComputeBuffer properties;

    //buffer that contains geometry
    private ComputeBuffer quadPoints;

    //handles for calling kernels
    private int integrate1Handle;
    private int integrate2Handle;

    // Use this for initialization
    private void updateBoundary()
    {

        integrateShader.SetFloats("boundaryLow", new float[] { world.boundariesLow.x, world.boundariesLow.y });
        integrateShader.SetFloats("boundaryHigh", new float[] { world.boundariesHigh.x, world.boundariesHigh.y });
    }

    public IEnumerator slowUpdates()
    {
        for (;;)
        {
            updateBoundary();
            yield return new WaitForSeconds(1f);
        }
    }

    void Start() {

        if (world == null)
            world = GameObject.Find("World").GetComponent<World>();

        computes = new List<Compute>();
        foreach (Compute c in this.GetComponentsInChildren<Compute>())
            computes.Add(c);

        //set handles
        integrate1Handle = integrateShader.FindKernel("Integrate1");
        integrate2Handle = integrateShader.FindKernel("Integrate2");


        //cerate empty buffers
        positions = new ComputeBuffer(ParticleNumber, 2 * ShaderConstants.FLOAT_STRIDE);
        lastPositions = new ComputeBuffer(ParticleNumber, 2 * ShaderConstants.FLOAT_STRIDE);
        velocities = new ComputeBuffer(ParticleNumber, 2 * ShaderConstants.FLOAT_STRIDE);
        forces = new ComputeBuffer(ParticleNumber, 2 * ShaderConstants.FLOAT_STRIDE);
        properties = new ComputeBuffer(ParticleNumber, ShaderConstants.PROP_STRIDE);


        //initialize the per-particle data
        Vector2[] zeros = new Vector2[ParticleNumber]; //create a bunch of zero vectors
        positions.SetData(zeros);
        lastPositions.SetData(zeros);
        velocities.SetData(zeros);
        forces.SetData(zeros);

        //make positions interesting
        for (int i = 0; i < ParticleNumber; i++)
            zeros[i].Set(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        positions.SetData(zeros);

        //make velocities interesting
        for (int i = 0; i < ParticleNumber; i++)
            zeros[i].Set(Random.Range(-1f,1f), Random.Range(-1f,1f));
        velocities.SetData(zeros);

        //make forces interesting
        
        for (int i = 0; i < ParticleNumber; i++)
            zeros[i].Set(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        //_forces.SetData(zeros);
        

        ShaderConstants.Prop[] props = new ShaderConstants.Prop[ParticleNumber];
        for (int i = 0; i < ParticleNumber; i++)
        {
            props[i].state = ShaderConstants.PARTICLE_STATE_DEAD;
            props[i].color = new Vector4(1f, 1f, 1f, 1f);
        }
           
        properties.SetData(props);

       

        //set buffers
        integrateShader.SetBuffer(integrate1Handle, "positions", positions);
        integrateShader.SetBuffer(integrate1Handle, "lastPositions", lastPositions);
        integrateShader.SetBuffer(integrate1Handle, "velocities", velocities);
        integrateShader.SetBuffer(integrate1Handle, "forces", forces);
        integrateShader.SetBuffer(integrate1Handle, "properties", properties);

        integrateShader.SetBuffer(integrate2Handle, "properties", properties);
        integrateShader.SetBuffer(integrate2Handle, "positions", positions);
        integrateShader.SetBuffer(integrate2Handle, "velocities", velocities);
        integrateShader.SetBuffer(integrate2Handle, "forces", forces);

        //set constants
        integrateShader.SetFloat("timeStep", TimeStep);
        integrateShader.SetFloat("lifeEnd", ParticleLifeEnd);

        //set-up our geometry for drawing.
        quadPoints = new ComputeBuffer(6, ShaderConstants.QUAD_STRIDE);
        quadPoints.SetData(new[]
        {
            new Vector3(-ParticleDiameter / 2, ParticleDiameter / 2),
            new Vector3(ParticleDiameter / 2, ParticleDiameter / 2),
            new Vector3(ParticleDiameter / 2, -ParticleDiameter / 2),
            new Vector3(ParticleDiameter / 2, -ParticleDiameter / 2),
            new Vector3(-ParticleDiameter / 2, -ParticleDiameter / 2),
            new Vector3(-ParticleDiameter / 2, ParticleDiameter / 2),
        });

        // bind resources to material
        particleMaterial.SetBuffer("positions", positions);
        particleMaterial.SetBuffer("properties", properties);
        particleMaterial.SetBuffer("quadPoints", quadPoints);

        foreach (Compute c in computes)
            c.SetupShader(this);

        StartCoroutine(slowUpdates());
    }


    private void OnDestroy()
    {
        positions.Release();
        velocities.Release();
        forces.Release();
        properties.Release();
        lastPositions.Release();

        foreach (Compute c in computes)
            c.ReleaseBuffers();

    }
    private void updateBuffers(int i)
    {
        return;
    }    

	// Update is called once per frame
	void Update () {
        int nx = Mathf.CeilToInt((float) ParticleNumber / ShaderConstants.PARTICLE_BLOCK_SIZE);

        foreach (Compute c in computes)
            c.UpdatePreIntegrate(nx);

        integrateShader.Dispatch(integrate1Handle, nx, 1, 1);

        foreach (Compute c in computes)
            c.UpdateForces(nx);

        integrateShader.Dispatch(integrate2Handle, nx, 1, 1);

        foreach (Compute c in computes)
            c.UpdatePostIntegrate(nx);

    }

    private void OnRenderObject()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            return;
        }

        // set the pass -> there is only 1 pass here because we have a simple shader
        particleMaterial.SetPass(0);

        // draw
        Graphics.DrawProcedural(MeshTopology.Triangles, 6, ParticleNumber);
    }
}
