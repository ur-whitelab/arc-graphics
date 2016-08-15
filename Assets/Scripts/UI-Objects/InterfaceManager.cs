using UnityEngine;
using System.Collections;

public class InterfaceManager : MonoBehaviour {

    public GameObject world;
    public GameObject particleManager; 

    private Collider worldCollider;

    public GameObject placedPrefab;

    public float inputPeriod = 0.05f;
    public float inputPostDelay = 0.5f;

	// Use this for initialization
	void Start () {

        if(world == null)
            world = GameObject.Find("World");
        if (particleManager == null)
            particleManager = GameObject.Find("ParticleManager");

        worldCollider = (Collider)world.GetComponent(typeof(Collider));

        if (worldCollider == null)
            throw new System.Exception("Must have collider attached to world");

        StartCoroutine(doUpdate());
	}
    
    public IEnumerator doUpdate()
    {
        for (;;)
        {
            if (checkForPlacement())
                yield return new WaitForSeconds(inputPostDelay);
            else
                yield return new WaitForSeconds(inputPeriod);
        }
    }

    private bool checkForPlacement()
    {
        //check where we hit. Add objet there.
        if (Input.GetMouseButton(1))
        {
            RaycastHit hit;
            if (!worldCollider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f))
                return false;

            GameObject placed = (GameObject)GameObject.Instantiate(placedPrefab, hit.point, world.transform.rotation);

            return true;
        }
        return false;
    }
}
