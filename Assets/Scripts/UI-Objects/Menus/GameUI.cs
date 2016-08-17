using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public GameObject ComputeSpawn;
    public float UpdateDelay = 0.05f;
    private ComputeSpawn cs;

    public Text InstancedParticlesText;

    void Start()
    {
        cs = ComputeSpawn.GetComponent<ComputeSpawn>();
        StartCoroutine(SlowUpdate());
    }

    public IEnumerator SlowUpdate()
    {
        for(;;)
        {
            yield return new WaitForSeconds(UpdateDelay);
            ((Text)InstancedParticlesText).text = cs.InstancedParticles.ToString();
        }
        
    }

}
