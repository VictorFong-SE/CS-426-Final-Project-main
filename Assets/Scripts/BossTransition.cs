using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossTransition : MonoBehaviour
{
    public static BossTransition Instance { get; private set; }

    public Image blackPanel;

    public MeshRenderer toRender;

    public int count = 0;

    public float Range { get; } = 5;

    private bool fading;

    void Start()
    {
        Instance = this;

        var oldColor = toRender.material.GetColor("_OutlineColor");
        toRender.material.SetColor("_OutlineColor", new Color(oldColor.r, oldColor.g, oldColor.b, 0));
    }

    void Update()
    {
        if (fading)
        {
            blackPanel.color = new Color(blackPanel.color.r, blackPanel.color.g, blackPanel.color.b, Mathf.Min(blackPanel.color.a + (5 * Time.deltaTime), 1));
        }
        else if (count == 2)
        {

            var oldColor = toRender.material.GetColor("_OutlineColor");
            var normed = Mathf.InverseLerp(3, 0, Mathf.Clamp(Vector3.Distance(transform.position, Player.Instance.transform.position), 0, 5));
            toRender.material.SetColor("_OutlineColor", new Color(oldColor.r, oldColor.g, oldColor.b, normed));
        }

        if (blackPanel.color.a == 1)
        {
            SceneManager.LoadScene("Boss Arena");
            Destroy(gameObject);
        }

        if (count == 2 && Vector3.Distance(Player.Instance.transform.position, transform.position) <= 3 && Input.GetKeyDown(KeyCode.F))
        {
            fading = true;
        }
    }
}
