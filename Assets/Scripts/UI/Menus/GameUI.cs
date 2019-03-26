using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Rochester.ARTable.Particles;


namespace Rochester.ARTable.UI
{

    public class GameUI : MonoBehaviour
    {

        public float UpdateDelay = 0.05f;

        private ComputeSource cs;
        private ParticleStatistics ps;

        public Text AvailableParticlesText;
        public Text AliveParticlesText;
        public Text TargetParticlesText;

        void Start()
        {
            ParticleManager pm = GameObject.Find("ParticleManager").GetComponent<ParticleManager>();
            cs = pm.GetComponentInChildren<ComputeSource>();
            ps = pm.GetComponentInChildren<ParticleStatistics>();

            //Just use ananyomous event handlers
            ps.ComputeModifierStatistics((s, e) =>
            {
                AliveParticlesText.text = ((ParticleStatisticsModifierEventArgs)e).sum[2].ToString();
            });

            ps.ComputeModifierStatistics(ShaderConstants.PARTICLE_MODIFIER_TARGET, (s, e) =>
            {
                TargetParticlesText.text = ((ParticleStatisticsModifierEventArgs)e).sum[0].ToString();
            });

            StartCoroutine(SlowUpdate());
        }

        public IEnumerator SlowUpdate()
        {
            for (;;)
            {
                yield return new WaitForSeconds(UpdateDelay);
                AvailableParticlesText.text = Mathf.Max(0, cs.AvailableParticles).ToString();
            }

        }

    }

}