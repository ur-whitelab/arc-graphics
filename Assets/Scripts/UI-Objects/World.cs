using UnityEngine;
using System.Collections;

public class World : MonoBehaviour {

    public Vector2 boundariesLow;
    public Vector2 boundariesHigh;
    public Vector2 size;
    private new Collider collider;

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

        collider = GetComponent<Collider>();
    }

    /*
     * Returns the mouse position or zero if it can't find it.
     */
    public Vector3 GetMousePosition()
    {
        RaycastHit hit;
        if (collider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000.0f))
        {
            return hit.point;
        }

        //return center of screen otherwise. Rely on clamping
        if (collider.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, 1000.0f))
        {
            return hit.point;
        }

        return Vector3.zero;
    }
	
}
