using UnityEngine;
using System.Collections;
using System;

namespace Rochester.ARTable.UI
{

    public class CameraScreenshotModifierEventArgs : EventArgs
    {
        public byte[] jpg {get; set;}
    }

    public class CameraControls : MonoBehaviour
    {

        public Vector3 startPosition;
        public float mouseSensitivity = 0.05f;
        public float zoomSensitivity = 0.2f;

        public int resWidth = 960;
        public int resHeight = 540;

        public event EventHandler<CameraScreenshotModifierEventArgs> TakeScreenshot;

        private World world;
        private Vector3 lastPosition;
        private Camera worldCamera;

        private Vector2 clampLow;
        private Vector2 clampHigh;
        private float vextent, hextent;
        private float maxZoom = 0;


        private bool takeShot = false;

        public int[] ScreenshotResolution {
            get{
                return _screenshotResolution;
            }
            set {
                if (_screenshotResolution[0] != value[0] || _screenshotResolution[1] != value[1])
                {
                    _screenshotResolution = value;
                    screenshotRT = new RenderTexture(_screenshotResolution[0], _screenshotResolution[1], 24);
                    screenshot = new Texture2D(_screenshotResolution[0], _screenshotResolution[1], TextureFormat.RGB24, false);
                    screenshotRect = new Rect(0, 0, _screenshotResolution[0], _screenshotResolution[1]);
                }
            }

        }

        private int[] _screenshotResolution = new int[] { 100, 100 };
        private RenderTexture screenshotRT;
        private Texture2D screenshot;
        private Rect screenshotRect;

        void LateUpdate()
        {
            if(TakeScreenshot != null)
            {
                //set camera to render onto rt
                worldCamera.targetTexture = screenshotRT;
                //set background to black
                Color temp = worldCamera.backgroundColor;
                worldCamera.backgroundColor = Color.black;
                //render
                worldCamera.Render();
                //set active rendertexture to be the one just rendered onto
                RenderTexture.active = screenshotRT;
                //read from active rendertexture into screenshot
                screenshot.ReadPixels(screenshotRect, 0, 0);
                //reset
                worldCamera.targetTexture = null;
                worldCamera.backgroundColor = temp;
                RenderTexture.active = null; // JC: added to avoid errors
                byte[] bytes = screenshot.EncodeToJPG(97);
                takeShot = false;
                CameraScreenshotModifierEventArgs e = new CameraScreenshotModifierEventArgs();
                e.jpg = bytes;
                TakeScreenshot(this, e);
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
            vextent = worldCamera.orthographicSize;
            hextent = worldCamera.orthographicSize * Screen.width / Screen.height;
            ScaledCoords = new Vector2(Mathf.Lerp(-hextent, hextent, coords.x), Mathf.Lerp(-vextent, vextent, coords.y));
            return (ScaledCoords);
        }

        // Use this for initialization
        void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();
            lastPosition = startPosition;
            worldCamera = GetComponent<Camera>();
            updateCameraClamp();
            ScreenshotResolution = new int[] {resWidth, resHeight};
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
            vextent = worldCamera.orthographicSize;
            hextent = worldCamera.orthographicSize * Screen.width / Screen.height;

            /*
             * Precompute min-max for camera. This assumes world is centered at origin
             */

            clampLow = new Vector2(world.boundariesLow.x + vextent * 2, world.boundariesLow.y + hextent * 2);
            clampHigh = new Vector2(world.boundariesHigh.x - vextent * 2, world.boundariesHigh.y - hextent * 2);
            maxZoom = Mathf.Min(world.size.x, world.size.y) / 4;
        }
    }

}