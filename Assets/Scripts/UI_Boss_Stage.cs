using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Boss_Stage : MonoBehaviour
{
    public Text textBoss;
    public Boss boss;
    
    // Start is called before the first frame update
    void Start()
    {
        textBoss.text = "Stage: 1";
    }

    // Update is called once per frame
    void Update()
    {
        int hp = boss.GetHealth();
        switch (hp > 40 && hp < 60 ? "High" :
            hp > 20 && hp < 40 ? "Mid" :
            hp > 0 && hp < 20 ? "Low" : "Floor")
        {
            case "High":
                textBoss.text = "Stage: 1";
                break;
            case "Mid":
                textBoss.text = "Stage: 2";
                break;
            case "Low":
                textBoss.text = "Stage: 3";
                break;
            case "Floor":
                break;
        }
    }
    
}
