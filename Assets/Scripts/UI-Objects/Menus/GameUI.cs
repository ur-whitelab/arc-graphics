using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public GameObject ParticleManager;

    public float UpdateDelay = 0.05f;

    private ComputeSpawn cs;
    private ParticleStatistics ps;

    public Text InstancedParticlesText;
    public Text AliveParticlesText;

    void Start()
    {
        cs = ParticleManager.GetComponentsInChildren<ComputeSpawn>()[0];
        ps = ParticleManager.GetComponentsInChildren<ParticleStatistics>()[0];

        //Just use ananyomous event handlers
        ps.ComputeModifierStatistics(0, 0, (s, e) => {
            AliveParticlesText.text = ((ParticleStatisticsModifierEventArgs)e).sum[1].ToString();
        });

        StartCoroutine(SlowUpdate());
    }

    public IEnumerator SlowUpdate()
    {
        for(;;)
        {
            yield return new WaitForSeconds(UpdateDelay);
            InstancedParticlesText.text = cs.InstancedParticles.ToString();
        }
        
    }

}
