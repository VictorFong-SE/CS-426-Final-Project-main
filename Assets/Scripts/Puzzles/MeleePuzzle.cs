using System.Collections;
using UnityEngine;

public class MeleePuzzle : MonoBehaviour, IOnBeat
{
    public PlayerMove[] pattern;

    public Renderer controlObject;

    private int counter = 0;
    private int soFar = 0;

    public PlayerMove CurState { get; private set; }

    void Start()
    {
        TimeManager.RegisterEntity(this);
    }

    void Update()
    {
        CurState = pattern[counter];
        counter = (counter + 1) % pattern.Length;
    }

    public void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        if (soFar == pattern.Length)
        {
            GetComponent<AudioSource>().Play();
            StartCoroutine("killing");
        }

        var pTransform = Player.Instance.transform;
        if (pmove == CurState && Physics.Raycast(pTransform.position + new Vector3(0, 1, 0), pTransform.forward, out RaycastHit hit, 5, ~(1 << 3)) && hit.collider.gameObject == gameObject)
        {
            soFar++;
        }
        else
        {
            soFar = 0;
        }
    }

    void OnDestroy()
    {
        TimeManager.UnregisterEntity(this);
    }

    IEnumerator killing()
    {
        yield return new WaitForSeconds(.25f);
        Destroy(controlObject);
    }
}