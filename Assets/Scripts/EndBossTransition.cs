using UnityEngine;
using UnityEngine.UI;

public class EndBossTransition : MonoBehaviour
{
    public Image blackPanel;
    public GameObject managers;

    public float Range { get; } = 5;

    private float count = 0;

    void Start()
    {
    }

    void Update()
    {
        blackPanel.color = new Color(blackPanel.color.r, blackPanel.color.g, blackPanel.color.b, Mathf.Max(blackPanel.color.a - (.5f * Time.deltaTime), 0));

        if (blackPanel.color.a == 0)
        {
            count += Time.deltaTime;
        }

        if (count >= 2)
        {
            managers.SetActive(true);
            Destroy(gameObject);
        }
    }
}
