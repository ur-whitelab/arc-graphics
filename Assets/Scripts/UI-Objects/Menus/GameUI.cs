using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public GameObject ParticleManager;

    public float UpdateDelay = 0.05f;

    private ComputeSource cs;
    private ParticleStatistics ps;

    public Text AvailableParticlesText;
    public Text AliveParticlesText;
    public Text TargetParticlesText;

    void Start()
    {
        cs = ParticleManager.GetComponentInChildren<ComputeSource>();
        ps = ParticleManager.GetComponentInChildren<ParticleStatistics>();

        //Just use ananyomous event handlers
        ps.ComputeModifierStatistics(0, 0, (s, e) => {
            AliveParticlesText.text = ((ParticleStatisticsModifierEventArgs)e).sum[1].ToString();
        });

        ps.ComputeTargetStatistics(0, (s, e) => {
            TargetParticlesText.text = ((ParticleStatisticsTargetEventArgs)e).sum.ToString();
        });

        StartCoroutine(SlowUpdate());
    }

    public IEnumerator SlowUpdate()
    {
        for(;;)
        {
            yield return new WaitForSeconds(UpdateDelay);
            AvailableParticlesText.text = Mathf.Max(0,cs.AvailableParticles).ToString();
        }
        
    }

}
