using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuControl : MonoBehaviour
{
    [SerializeField]
    string pickScene;

    public void Back()
    {
        SceneManager.LoadScene(pickScene);
    }
}
