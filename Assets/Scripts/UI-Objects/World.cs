using UnityEngine;
using System.Collections;

public class World : MonoBehaviour {

    public Vector2 boundariesLow;
    public Vector2 boundariesHigh;
    public Vector2 size;

	// Use this for initialization
	void Awake () {
        if (boundariesHigh.Equals(boundariesLow))
        {
            Vector3 bounds_min = GetComponent<Collider>().bounds.min;
            Vector3 bounds_max = GetComponent<Collider>().bounds.max;
            boundariesLow = new Vector2(bounds_min.x, bounds_min.y);
            boundariesHigh = new Vector2(bounds_max.x, bounds_max.y);
            size = boundariesHigh - boundariesLow;
        }
    }
	
}
