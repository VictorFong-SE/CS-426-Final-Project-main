using UnityEngine;

public class Lever : MonoBehaviour
{
    public GameObject controlled;
    public MeshRenderer toRender;

    Transform t;
    Animator animator;

    void Start()
    {
        t = transform;
        animator = GetComponent<Animator>();
        toRender = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        var playerPos = Player.Instance.transform.position;
        var oldColor = toRender.material.GetColor("_OutlineColor");
        if (Vector3.Distance(playerPos, t.position) <= 2 && Input.GetKeyDown(KeyCode.F))
        {
            toRender.material.SetColor("_OutlineColor", new Color(oldColor.r, oldColor.g, oldColor.b, 0));
            BossTransition.Instance.count++;
            animator.SetBool("LeverUp", true);
            gameObject.GetComponentInParent<AudioSource>().Play();
            Destroy(controlled);
            Destroy(this);
        }
        else
        {
            var normed = Mathf.InverseLerp(3, 0, Mathf.Clamp(Vector3.Distance(transform.position, playerPos), 0, 5));
            toRender.material.SetColor("_OutlineColor", new Color(oldColor.r, oldColor.g, oldColor.b, normed));
        }
    }
}