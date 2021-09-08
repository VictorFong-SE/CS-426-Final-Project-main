using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    [SerializeField]
    Enemy the_boss;
    // Start is called before the first frame update
    void Update()
    {
        Player.Instance.SetBossState(the_boss);

        Destroy(this);
    }
}
