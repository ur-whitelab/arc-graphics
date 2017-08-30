using UnityEngine;
using System.Collections;

namespace Rochester.ARTable.UI
{

    public class CameraControls : MonoBehaviour
    {

        public Vector3 startPosition;
        public float mouseSensitivity = 0.05f;
        public float zoomSensitivity = 0.2f;

        private World world;
        private Vector3 lastPosition;
        private Camera worldCamera;

        private Vector2 clampLow;
        private Vector2 clampHigh;
        private float maxZoom = 0;

        public int resWidth = 960;
        public int resHeight = 540;

        private bool takeShot = false;
        public string name = "default";

        public static string ScreenShotName(int width, int height)
        {
            return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.jpg",
                                 Application.dataPath,
                                 width, height,
                                 System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ms"));
        }

        public void TakeShot(string ImgName)//invoke this in CommClient to take photo
        {
            takeShot = true;
            name = ImgName;
        }

        void LateUpdate()
        {
            takeShot |= Input.GetKeyDown("k");
            if (takeShot)
            {
                RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
                worldCamera.targetTexture = rt;
                Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
                worldCamera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                worldCamera.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                Destroy(rt);
                byte[] bytes = screenShot.EncodeToJPG();
                string filename;
                if (name != "default")
                {
                    filename = (Application.dataPath + "/screenshots/" + name + ".jpg");
                }
                else
                {
                    filename = ScreenShotName(resWidth, resHeight);
                }
                //System.IO.File.WriteAllBytes(filename, bytes);
                Debug.Log(string.Format("Took screenshot but did NOT save to: {0}", filename));

                takeShot = false;
                name = "default";
            }
        }

        public float MaxZoom
        {
            get
            {
                if (maxZoom == 0)
                    Start();
                return maxZoom;
            }

        }

        public Vector2 UnitToWorld(Vector2 coords)
        {
            //coords must be scaled from 0 to 1 to use Matf.Lerp
            Vector2 ScaledCoords;
            ScaledCoords = new Vector2(Mathf.Lerp(clampLow.x, clampHigh.x, coords.x), Mathf.Lerp(clampLow.y, clampHigh.y, coords.y));
            return (ScaledCoords);
        }

        // Use this for initialization
        void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();
            lastPosition = startPosition;
            worldCamera = GetComponent<Camera>();
            updateCameraClamp();
        }


        public void Update()
        {
            if (Input.GetMouseButtonDown(2))//mouse is first pressed
            {
                lastPosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(2)) //mouse is pressed
            {
                Vector3 new_position = -(Input.mousePosition - lastPosition);
                new_position.z = 0;
                //we divide by size so when we zoom in the steps are smaller.
                new_position = new_position * mouseSensitivity / worldCamera.orthographicSize + transform.position;
                new_position.x = Mathf.Clamp(new_position.x, clampLow.x, clampHigh.x);
                new_position.y = Mathf.Clamp(new_position.y, clampLow.y, clampHigh.y);
                transform.position = new_position;
            }

            float da = Input.GetAxis("Mouse ScrollWheel");
            if (da < 0)
            {
                if (worldCamera.orthographicSize + zoomSensitivity < maxZoom)
                {
                    worldCamera.orthographicSize += zoomSensitivity;
                    //move towards origin in case this zoom could bring us out of clamp
                    //zooming changes clamp by zoomsensitivity / 2
                    transform.Translate(-zoomSensitivity / 4 * transform.position.x, -zoomSensitivity / 4 * transform.position.y, 0);
                    updateCameraClamp();
                }

            }
            else if (da > 0)
            {
                worldCamera.orthographicSize -= zoomSensitivity;
                updateCameraClamp();
            }

        }

        private void updateCameraClamp()
        {
            float vextent = worldCamera.orthographicSize;
            float hextent = worldCamera.orthographicSize * Screen.height / Screen.width;

            /*
             * Precompute min-max for camera. This assumes world is centered at origin
             */

            clampLow = new Vector2(world.boundariesLow.x + vextent * 2, world.boundariesLow.y + hextent * 2);
            clampHigh = new Vector2(world.boundariesHigh.x - vextent * 2, world.boundariesHigh.y - hextent * 2);
            maxZoom = Mathf.Min(world.size.x, world.size.y) / 4;
        }
    }

}