using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellController : MonoBehaviour
{
    public Transform target = null;
    public GameObject muzzlePrefab;
    public GameObject hitPrefab;
    public float speed;
    public float bounceForce = 10;

    private Vector3 startPos;
    private Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();

        if (muzzlePrefab != null)
        {
            var muzzleVFX = Instantiate(muzzlePrefab, transform.position, Quaternion.identity);
            muzzleVFX.transform.forward = gameObject.transform.forward;
            var ps = muzzleVFX.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(muzzleVFX, ps.main.duration);
            else
            {
                var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzleVFX, psChild.main.duration);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target != null)
        {
            transform.LookAt(target);
        }

        if (speed != 0 && rb != null)
            rb.position += (transform.forward) * (speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player")
        {
            Enemy temp = collision.gameObject.GetComponent<Enemy>();

            if (temp != null)
            {
                temp.Damage(4);
            }

            rb.useGravity = true;
            rb.drag = 0.5f;
            ContactPoint contact = collision.contacts[0];
            rb.AddForce(Vector3.Reflect((contact.point - startPos).normalized, contact.normal) * bounceForce, ForceMode.Impulse);
            Destroy(gameObject);
        }

    }
}
