using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject loadingScreen;
    public List<GameObject> toEnable;
    public List<GameObject> toDisable;
    static bool done;

    public static void GameOver()
    {
        var inst = FindObjectOfType<GameOverManager>();

        foreach (var go in inst.toEnable)
        {
            go.SetActive(true);
        }
        foreach (var go in inst.toDisable)
        {
            go.SetActive(false);
        }

        TimeManager.Instance.OnDestroy();

        done = true;
    }

    void Update()
    {
        if (done && Input.GetKeyDown(KeyCode.Mouse0))
        {
            done = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            loadingScreen.SetActive(true);
        }
    }
}