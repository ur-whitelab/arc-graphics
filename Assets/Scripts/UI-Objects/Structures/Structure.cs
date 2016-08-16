using UnityEngine;
using System.Collections;

public abstract class Structure : MonoBehaviour {

    public abstract void EnableInteractions();

    public abstract bool CanEnableInteractions();
}
