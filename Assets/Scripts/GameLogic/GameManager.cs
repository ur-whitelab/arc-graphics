using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public int maxSpawnAmount = 8;
    public int minSpawnPeriod = 1;
    [Tooltip("The frame-rate below which there will be throttling")]
    public int UncappedFPS = 60;

	// Use this for initialization
	void Start () {

        List<StructureSource> sources = new List<StructureSource>();        

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Source"))
        {
            sources.Add(g.GetComponent<StructureSource>());
        }

        int lastLevel = 0;
        ParticleStatistics ps = GameObject.Find("ParticleManager").GetComponentInChildren<ParticleStatistics>();
        ps.ComputeTargetStatistics(0, (s, e) =>
         {
             int level = (int) Mathf.Log(((ParticleStatisticsTargetEventArgs)e).sum, 1.03f);
             if (level - lastLevel != 0)
             {
                 lastLevel = level;
                 foreach (StructureSource c in sources)
                 {
                     if (level > 100 && level % 2 == 0 && c.spawnPeriod > minSpawnPeriod)
                     {
                         c.spawnPeriod--;
                     }
                 }
             }
         });
	}

    void update()
    {
        //if above uncapped fps, slow down to account for faster frame rate
        //otherwise, make it fixed
        Time.timeScale = Mathf.Clamp(Time.smoothDeltaTime * UncappedFPS, 1.0f, 0.00001f);        
    }

}
