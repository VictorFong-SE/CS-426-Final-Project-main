using System.Collections.Generic;
using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractive
{
    public float Range => 5;

    public bool isOpen;
    public KeyColor keyColor;
    public float yLimit;

    public float count;

    public UI_LockStatus LockStatus;
    public void OnInteract(GameObject interactor)
    {
        if (KeyManager.HasKey(keyColor))
        {
            isOpen = true;
            GetComponent<AudioSource>().Play();
        }
        else
        {
            LockStatus.text.text = "This door is locked...";
            count = 3;
        }
    }

    void Update()
    {
        if (count > 0)
        {
            count -= Time.deltaTime;
        }
        else
        {
            count = 0;
            LockStatus.text.text = "";
        }

        var myPos = transform.position;
        if (isOpen)
        {
            transform.position = new Vector3(myPos.x, myPos.y - (5 * Time.deltaTime), myPos.z);

            if (myPos.y < yLimit)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            var normed = Mathf.InverseLerp(5, 0, Mathf.Clamp(Vector3.Distance(myPos, Player.Instance.transform.position), 0, 5));
            var renderer = GetComponent<MeshRenderer>();
            var oldColor = renderer.material.GetColor("_OutlineColor");
            renderer.material.SetColor("_OutlineColor", new Color(oldColor.r, oldColor.g, oldColor.b, normed));
        }
    }
}