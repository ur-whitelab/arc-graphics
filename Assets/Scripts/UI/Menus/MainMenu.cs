using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Rochester.ARTable.UI
{

    public class MainMenu : MonoBehaviour
    {

        public void LoadLevel(string name)
        {
            SceneManager.LoadScene(name);
        }
    }

}