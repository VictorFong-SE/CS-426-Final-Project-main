using UnityEngine;

public class MagicPuzzle : MonoBehaviour, IOnBeat
{
    public PlayerMove[] pattern;

    public Renderer controlObject;

    public Material fireMaterial;
    public Material iceMaterial;

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

        switch (CurState)
        {
            case PlayerMove.FIRE:
                controlObject.material = fireMaterial;
                break;

            case PlayerMove.ICE:
                controlObject.material = iceMaterial;
                break;
        }

        counter = (counter + 1) % pattern.Length;
    }

    public void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        if (soFar == pattern.Length)
        {
            Destroy(controlObject);
        }

        var pTransform = Player.Instance.transform;
        if (pmove == CurState && Physics.Raycast(pTransform.position, pTransform.forward, out RaycastHit hit, Player.PLAYER_SPELL_RANGE, ~(1 << 3)) && hit.collider.gameObject == gameObject)
        {
            soFar++;
        }
        else
        {
            soFar = 0;
        }
    }
}