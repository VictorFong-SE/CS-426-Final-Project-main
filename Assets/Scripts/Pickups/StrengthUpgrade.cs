using UnityEngine;

public class StrengthUpgrade : MonoBehaviour
{
    public int boost;
    public GameObject hudElement;

    public void OnCollisionEnter(Collision collision)
    {
        var player = collision.transform.GetComponentInParent<Player>();
        if (player != null)
        {
            player.AddDamageBoost(boost);
            hudElement.SetActive(true); 
            Destroy(gameObject);
        }
    }
}
