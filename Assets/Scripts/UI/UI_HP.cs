using UnityEngine;
using UnityEngine.UI;

public class UI_HP : MonoBehaviour
{
    public Slider healthBar;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (healthBar.value != Player.Instance.GetHealth() && Player.Instance.GetHealth() >= 0)
        {
            healthBar.value = Player.Instance.GetHealth();
        }
    }
}
