using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public int maxSpawnAmount = 5;
    public int minSpawnPeriod = 10;

	// Use this for initialization
	void Start () {

        List<StructureSource> sources = new List<StructureSource>();        

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Source"))
        {
            sources.Add(g.GetComponent<StructureSource>());
        }

        ParticleStatistics ps = GameObject.Find("ParticleManager").GetComponentInChildren<ParticleStatistics>();
        ps.ComputeTargetStatistics(0, (s, e) =>
         {
             int level = ((ParticleStatisticsTargetEventArgs)e).sum / 25;
             foreach(StructureSource c in sources)
             {
                 if (level % 5 == 0 && c.spawnPeriod > minSpawnPeriod)
                 {
                     c.spawnPeriod--;
                 }else if(level % 20 == 0)
                 {
                     c.spawnAmount = Mathf.Clamp(c.spawnAmount + 1, 0, maxSpawnAmount);
                 }                 
             }
         });
	}

}
