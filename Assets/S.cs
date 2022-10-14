using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class S : MonoBehaviour
{
    public Button startButton;

    void Start()
    {
        startButton.onClick.AddListener(Switch);
    }


    void Switch()
    {
        SceneManager.LoadScene(1);
    }
}
