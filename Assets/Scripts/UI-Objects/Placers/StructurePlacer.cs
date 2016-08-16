using UnityEngine;
using System.Collections;

public class StructurePlacer : MonoBehaviour {


    private World world;
    public GameObject placedPrefab;

    private GameObject placing;
    private Structure placingScript;

    public float inputPeriod = 0.05f;
    public float inputPostDelay = 0.5f;

    protected enum States {Sleeping, Ready, PlacingInvalid, PlacingValid};
    private States state;

	// Use this for initialization
	void Start () {

        if(world == null)
        {
            world = GameObject.Find("World").GetComponent<World>();
        }

        state = States.Ready;
        StartPlacement();
        StartCoroutine(SlowUpdate());
	}

    public void Update() //rapid updates go here
    {
        if (state == States.PlacingValid || state == States.PlacingInvalid)
        {         
            placing.transform.position = world.GetMousePosition();
        }
    }
    
    public IEnumerator SlowUpdate()
    {
        for (;;)
        {
            if(state == States.PlacingValid || state == States.PlacingInvalid)
            {                
                state = placingScript.CanEnableInteractions() ? States.PlacingValid : States.PlacingInvalid;

                if (state == States.PlacingValid && Input.GetMouseButton(0))
                {
                    placingScript.EnableInteractions();
                    Cursor.visible = true;

                    placing = null;
                    placingScript = null;

                    state = States.Sleeping;
                    yield return new WaitForSeconds(inputPostDelay);
                    state = States.Ready;
                    StartPlacement();
                }
                    
            }
            
            yield return new WaitForSeconds(inputPeriod);
        }
    }

    public void StartPlacement()
    {
        state = States.PlacingInvalid;
        Cursor.visible = false;

        Vector3 location = world.GetMousePosition();
        placing = (GameObject) GameObject.Instantiate(placedPrefab, location, new Quaternion());
        placingScript = placing.GetComponent<Structure>();
    }
}
