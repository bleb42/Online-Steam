using UnityEngine;
using UnityEngine.SceneManagement;

public class InitBootstrap : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("Menu");
    }
}