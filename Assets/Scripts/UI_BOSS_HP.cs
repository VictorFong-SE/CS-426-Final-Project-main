using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BOSS_HP : MonoBehaviour
{
    public Slider healthBar;
    public Boss the_boss;

    // Update is called once per frame
    void Update()
    {
        healthBar.value = the_boss.GetHealth();
    }
}
