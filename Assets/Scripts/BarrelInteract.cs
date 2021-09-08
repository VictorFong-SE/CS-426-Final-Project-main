using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelInteract : MonoBehaviour, IInteractive
{
    public float Range { get; private set;} = 3.0f;

    public AudioSource audio;

    public AudioClip clip1;
    public AudioClip clip2;

    public bool firstSound = false;

    public void OnInteract(GameObject interactor){
        if(interactor.tag == "Player"){
            Player.Instance.SetMaxMana();

            if(firstSound){
                audio.clip = clip1;
                audio.Play();
            }
            else{
                audio.clip = clip2;
                audio.Play();
            }

            firstSound = !firstSound;
        }
    }
}
