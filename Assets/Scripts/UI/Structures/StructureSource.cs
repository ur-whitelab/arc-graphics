using Rochester.ARTable.Particles;
using UnityEngine;


namespace Rochester.ARTable.UI
{

    public class StructureSource : MonoBehaviour
    {

        public int spawnPeriod
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

        public int SourceIndex
        {
            get; private set;
        }

        public Vector2 StartVelocity;
        public Color particleColor = Color.grey;
        public int StartPeriod;
        public uint Group;

        private ComputeSource ct;

        private ShaderConstants.Source _source = new ShaderConstants.Source();
        private ShaderConstants.Source Source
        {
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
            StartPeriod = Mathf.Max(1, StartPeriod);
            _source = new ShaderConstants.Source(new Vector2(transform.position.x, transform.position.y), StartVelocity, particleColor, spawnPeriod: StartPeriod, group: Group);
            SourceIndex = ct.AddSource(_source);
        }
    }

}