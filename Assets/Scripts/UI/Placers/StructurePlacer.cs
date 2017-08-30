using UnityEngine;
using System.Collections;


namespace Rochester.ARTable.UI
{

    public class StructurePlacer : MonoBehaviour
    {


        private World world;
        public GameObject placedPrefab;

        private GameObject placing;
        private Structure placingScript;

        public float inputPeriod = 0.05f;
        public float inputPostDelay = 0.05f;

        protected enum States { Sleeping, Ready, PlacingInvalid, PlacingValid };
        private States state;

        // Use this for initialization
        void Start()
        {

            if (world == null)
            {
                world = GameObject.Find("World").GetComponent<World>();
            }

            state = States.Ready;
            StartCoroutine(SlowUpdate());
        }

        public void Update() //rapid updates go here
        {
            if (state == States.PlacingValid || state == States.PlacingInvalid)
            {
                Vector3 p = world.GetMousePosition();
                if (p != Vector3.zero)
                    placingScript.SetPosition(world.GetMousePosition());
            }
        }

        public IEnumerator SlowUpdate()
        {
            for (;;)
            {

                if (state == States.Sleeping && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
                {
                    state = States.Ready;
                    if (placing != null)
                        state = States.PlacingInvalid;
                }

                if (state == States.PlacingValid || state == States.PlacingInvalid)
                {
                    state = placingScript.CanPlace() ? States.PlacingValid : States.PlacingInvalid;

                    //right-click for cancel
                    if (Input.GetMouseButton(1))
                    {
                        placingScript.CancelPlace();
                        finishPlace();

                        state = States.Sleeping;
                        yield return new WaitForSeconds(inputPostDelay);
                    }

                    //left-click for place
                    if (state == States.PlacingValid && Input.GetMouseButton(0))
                    {
                        //we try to place
                        if (placingScript.Place())
                            finishPlace();

                        state = States.Sleeping;
                        yield return new WaitForSeconds(inputPostDelay);
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
            placing = (GameObject)GameObject.Instantiate(placedPrefab, location, new Quaternion());
            placingScript = placing.GetComponent<Structure>();

            placingScript.StartPlace();
        }

        private void finishPlace()
        {
            Cursor.visible = true;

            placing = null;
            placingScript = null;
        }
    }

}