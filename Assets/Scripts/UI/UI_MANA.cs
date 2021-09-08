using UnityEngine;
using UnityEngine.UI;

public class UI_MANA : MonoBehaviour
{
    public Slider manaBar;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        manaBar.value = Player.Instance.GetMana();
    }
}
