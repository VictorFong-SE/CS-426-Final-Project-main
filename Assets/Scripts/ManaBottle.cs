using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBottle : MonoBehaviour
{
    void OnTriggerEnter(Collider collision){
        if(collision.gameObject.tag == "Player")
        {
            if(Player.Instance.GetMana() < Player.MAX_MANA)
            {
                Player.Instance.AddMana(1);
                Destroy(gameObject);
            }
        }
    }
}
