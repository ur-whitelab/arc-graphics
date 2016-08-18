using UnityEngine;
using System.Collections;

public class StructureSource : MonoBehaviour
{
    public int spawnAmount {
        get
        {
            return Source.spawnAmount;
        }
        set
        {
            ShaderConstants.Source s = _source;
            s.spawnAmount = value;
            Source = s;
        }
    }

    public uint spawnPeriod
    {
        get
        {
            return Source.spawnPeriod;
        }
        set
        {
            ShaderConstants.Source s = _source;
            s.spawnPeriod = value;
            Source = s;
        }
    }

    public Vector2 Velocity
    {
        get
        {
            return Source.velocity1;
        }
        set
        {
            ShaderConstants.Source s = _source;
            s.velocity1 = value;
            Source = s;
        }
    }

    public int SourceIndex {
        get; private set;
    }

    public Vector2 StartVelocity1;
    public Vector2 StartVelocity2;
    public int startSpawnAmount;

    private ComputeSource ct;

    private ShaderConstants.Source _source = new ShaderConstants.Source();
    private ShaderConstants.Source Source {
        get { return _source; }
        set
        {
            _source = value;
            ct.updateSource(SourceIndex, value);
        }
        }

    void Start()
    {
        ct = GameObject.Find("ParticleManager").GetComponentInChildren<ComputeSource>();
        _source = new ShaderConstants.Source(new Vector2(transform.position.x, transform.position.y), StartVelocity1, StartVelocity2, 0, 60, startSpawnAmount);
        SourceIndex = ct.AddSource(_source);
    }
}
