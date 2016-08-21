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
            return Source.velocity;
        }
        set
        {
            ShaderConstants.Source s = _source;
            s.velocity = value;
            Source = s;
        }
    }

    public int SourceIndex {
        get; private set;
    }

    public Vector2 StartVelocity;
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
        _source = new ShaderConstants.Source(new Vector2(transform.position.x, transform.position.y), StartVelocity, 0, 1, startSpawnAmount);
        SourceIndex = ct.AddSource(_source);
    }
}
