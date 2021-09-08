using System.Collections;
using UnityEngine;
public class Boss : Enemy
{
    private BossAI brain;

    private SwitcherAI AISwitcher;

    public GameObject creditsScreen;

    public AudioSource audio;
    public AudioClip attack_clip;
    public AudioClip attack_clip2;
    public AudioClip block_clip;
    public AudioClip block_clip2;

    [SerializeField] public Animator anim;

    public override void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        brain.OnBeat(pmove, failure);
        base.OnBeat(pmove, failure);
        AISwitcher.OnBeat();
        switch (GetPrevMove())
        {
            case EnemyMove.ATTACK:
                if (Random.value < .5f)
                {
                    audio.clip = attack_clip;
                }
                else
                {
                    audio.clip = attack_clip2;
                }
                audio.Play();
                anim.SetTrigger("Attack");
                break;
            case EnemyMove.BLOCK:
                if (pmove == PlayerMove.ATTACK || pmove == PlayerMove.KICK)
                {
                    if (Random.value < .5f)
                    {
                        audio.clip = block_clip;
                    }
                    else
                    {
                        audio.clip = block_clip2;
                    }
                }
                anim.SetTrigger("Block");
                break;
            case EnemyMove.IDLE:
                if (IsStunned())
                {
                    anim.SetTrigger("Stun");
                }
                break;
            default:
                break;
        }
    }

    public override void Start()
    {
        base.Start();
        AISwitcher = new SwitcherAI(this);
    }

    public override void Kill()
    {
        anim.SetTrigger("Death");
        TimeManager.UnregisterEntity(this);
        Destroy(TimeManager.Instance.gameObject);
        Destroy(gameObject, 4.0f);
    }

    void OnDestroy()
    {
        if(creditsScreen != null){
            creditsScreen.SetActive(true);
        }
    }

    public override EnemyMove GetMove()
    {
        base.GetMove();
        return brain.GetMove();
    }

    public EnemyMove[] GetMoves()
    {

        return brain.GetMoves();
    }

    public bool IsStunned()
    {
        return brain.IsStunned;
    }

    public void setBrain(BossAI _bossAI)
    {
        if (brain != null)
        {
            _bossAI.moves = brain.moves;
            foreach (var x in brain.storedMoves)
            {
                _bossAI.storedMoves.Enqueue(x);
            }
            foreach (var x in brain.overrideQueue)
            {
                _bossAI.overrideQueue.Enqueue(x);
            }
            _bossAI.aiState = brain.aiState;
            _bossAI.stamina = brain.stamina;
            _bossAI.stun = brain.stun;
            _bossAI.stunEnded = brain.stunEnded;
            _bossAI.stunFollowup = brain.stunFollowup;
        }
        brain = _bossAI;
    }
}