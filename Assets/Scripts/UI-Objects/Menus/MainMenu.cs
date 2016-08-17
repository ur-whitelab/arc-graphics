using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

     public void LoadLevel(string name)
    {
        SceneManager.LoadScene(name);
    }
}
