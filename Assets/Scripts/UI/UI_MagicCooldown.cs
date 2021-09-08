using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_MagicCooldown : MonoBehaviour
{
    public GameObject uiCDR;
    // Start is called before the first frame update
    void Start()
    {
        uiCDR.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (TimeManager.Instance.spellReady)
        {
            
            uiCDR.SetActive(false);
        }
        else
        {
            uiCDR.SetActive(true);
        }
    }
}
