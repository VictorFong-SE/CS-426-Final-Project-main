using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMesh))]
public class UI_PowerupCountdown : MonoBehaviour
{
    public Text text;

    void Start()
    {
    }

    void Update()
    {
        var val = TimeManager.Instance.GetBeatsRemaining("Slowdown");

        if (val == null)
        {
            text.text = "";
        }
        else
        {
            text.text = $"{val}";
        }
    }
}