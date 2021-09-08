using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

class Credits : MonoBehaviour
{
    float count = 0;
    bool done;

    [SerializeField]
    RectTransform toScroll;

    void Start()
    {
        Reset();
    }

    public void Reset()
    {
        count = 0;
        done = false;
        var pos = toScroll.localPosition;
        toScroll.localPosition = new Vector3(pos.x, 0, pos.z);
    }

    void Update()
    {
        if (done)
        {
            return;
        }

        var pos = toScroll.localPosition;

        if (pos.y >= 1900)
        {
            done = true;
            SceneManager.LoadSceneAsync("Main Menu");
            return;
        }

        if (count > 5)
        {
            toScroll.localPosition = new Vector3(pos.x, pos.y + (25 * Time.deltaTime), pos.z);
        }

        count += Time.deltaTime;
    }
}