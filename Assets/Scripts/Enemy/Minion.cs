using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = System.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class Minion : Enemy
{
    public Slider slider;

    public bool activated;
    public bool engaged;

    private readonly LinkedList<EnemyMove> prevMoves = new LinkedList<EnemyMove>();
    private NavMeshAgent navAgent;
    private Animator animator;
    private Vector2 velocity = new Vector2();
    private Vector2 deltaPos;
    private bool dead, deadAnim;
    private bool canSeePlayer;

    [SerializeField]
    private TextMesh moveText;

    public const float MAX_SPEED = 3;
    public const float MAX_STEERING = 10;
    public const float HIT_RANGE = 1.5f;
    public const float ENGAGEMENT_RANGE = 4;
    public const float SIGHT_RANGE = 10;
    public const float SEARCH_MAX = 25;

    public AudioSource audio;
    public AudioClip attack_clip;
    public AudioClip attack_clip2;
    public AudioClip whif_clip;
    public AudioClip whif_clip2;
    public AudioClip block_clip;
    public AudioClip block_clip2;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        moveText = GetComponentInChildren<TextMesh>();
        moveText.gameObject.SetActive(false);

        slider.value = maxHealth;

        navAgent = GetComponent<NavMeshAgent>();
        navAgent.autoRepath = true;
        navAgent.updatePosition = false;
        navAgent.stoppingDistance = HIT_RANGE;

        animator = GetComponentInChildren<Animator>();

        deltaPos = transform.position;
    }

    // Update is called once per frame
    public override void Update()
    {
        animator.SetBool("Engaged", engaged);

        var myPos = transform.position;

        if (dead)
        {
            if (deadAnim)
            {
                var pos = transform.position;
                transform.position = new Vector3(pos.x, pos.y - (Time.deltaTime / 2), pos.z);
            }
            return;
        }

        if (!activated)
        {
            if (FindObjectOfType<MeleePuzzle>() != null)
            {
                return;
            }

            var pPos = Player.Instance.transform.position;
            if ((pPos - myPos).magnitude < 7 * Player.PLAYER_RANGE)
            {
                activated = true;
            }
            else
            {
                if (Mathf.Abs(Vector3.Angle(pPos - myPos, transform.forward)) > 90) // 180 degree cone
                {
                    return;
                }

                if (Physics.Raycast(myPos + new Vector3(0, 1.75f, 0), pPos - myPos, out RaycastHit h, SIGHT_RANGE))
                {
                    Debug.DrawRay(transform.position + new Vector3(0, 1.75f, 0), transform.forward * h.distance, Color.green);

                    if (h.collider.gameObject.tag == "Player")
                    {
                        activated = true;
                    }
                }
                else
                {
                    Debug.DrawRay(transform.position + new Vector3(0, 1.75f, 0), transform.forward * HIT_RANGE, Color.white);
                    return;
                }
            }
        }

        base.Update();

        navAgent.destination = Player.Instance.transform.position;

        var pvec = Player.Instance.transform.position - transform.position;
        if (Physics.Raycast(transform.position + new Vector3(0, 1.75f, 0), transform.forward, out RaycastHit hit, SIGHT_RANGE))
        {
            canSeePlayer = hit.collider.tag == "Player";
            if (canSeePlayer)
            {
                Debug.DrawRay(transform.position + new Vector3(0, 1.75f, 0), transform.forward * hit.distance, Color.green);
                transform.forward = pvec;
            }
            else
            {
                Debug.DrawRay(transform.position + new Vector3(0, 1.75f, 0), transform.forward * SIGHT_RANGE, Color.red);
            }
        }

        if (Vector3.Distance(transform.position, Player.Instance.transform.position) <= ENGAGEMENT_RANGE)
        {
            moveText.gameObject.SetActive(true);
        }
        else
        {
            moveText.gameObject.SetActive(false);
        }

        var playerDist = Vector3.Distance(transform.position, Player.Instance.transform.position);
        if (playerDist <= 3 * Player.PLAYER_RANGE)
        {
            engaged = MinionBlackboard.Instance.GetEngagementTokenForced(this);
        }

        if (engaged)
        {
            if (!navAgent.pathPending && (navAgent.pathStatus == NavMeshPathStatus.PathPartial))
            {
                MinionBlackboard.Instance.ReturnEngagementToken(this);
                engaged = false;
            }
        }

        if (PeekMove() == EnemyMove.IDLE)
        {
            moveText.gameObject.SetActive(false);
        }

        // Seeking
        var nextPos = navAgent.nextPosition;
        var goalPos = nextPos;

        if (engaged && playerDist <= HIT_RANGE - .1f)
        {
            goalPos += (transform.position - Player.Instance.transform.position).normalized * ((1.05f * HIT_RANGE) - playerDist);
        }
        else if (playerDist <= 2 * HIT_RANGE)
        {
            goalPos += (transform.position - Player.Instance.transform.position).normalized * (3 * HIT_RANGE);
        }

        if (NavMesh.SamplePosition(goalPos, out NavMeshHit hit1, 1, 0))
        {
            goalPos = hit1.position;
        }
        else
        {
            goalPos = nextPos;
        }

        // Local space
        var delta = goalPos - myPos;
        float dx = Vector3.Dot(transform.right, delta);
        float dz = Vector3.Dot(transform.forward, delta);
        delta = new Vector2(dx, dz);

        // Smoothing
        float smoothing = Mathf.Min(1f, Time.deltaTime / .15f);
        deltaPos = Vector2.Lerp(deltaPos, delta, smoothing);

        // Update velocity
        if (Time.deltaTime > 1e-5f)
        {
            velocity = deltaPos / Time.deltaTime;
        }

        animator.SetFloat("Vel X", dx);
        animator.SetFloat("Vel Z", dz);

        // Make sure simulation doesn't outpace minion
        if (Vector3.Distance(navAgent.nextPosition, myPos) > 2)
        {
            navAgent.nextPosition = (navAgent.nextPosition - myPos).normalized * 2 + myPos;
        }
    }

    public override void Kill()
    {
        if (prevMoves.Last != null)
        {
            MinionBlackboard.Instance.ReturnMoveToken(prevMoves.Last.Value);
        }

        MinionBlackboard.Instance.ReturnEngagementToken(this);
        TimeManager.UnregisterEntity(this);
        dead = true;
        if (slider != null)
        {
            Destroy(slider.gameObject);
        }
        Destroy(moveText);
        animator.applyRootMotion = false;
        animator.SetTrigger("Dying");
        StartCoroutine(Cleanup());
    }

    private IEnumerator<WaitForSeconds> Cleanup()
    {
        yield return new WaitForSeconds(5);
        deadAnim = true;
        yield return new WaitForSeconds(2);
        Destroy(animator.gameObject);
    }

    public override void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        base.OnBeat(pmove, failure);

        var prevMove = GetPrevMove();

        MinionBlackboard.Instance.ReturnMoveToken(prevMove);

        animator.speed = 1 / TimeManager.TimeForAnimation;

        if (moveText.gameObject.activeSelf)
        {
            switch (prevMove)
            {
                case EnemyMove.ATTACK:
                    if (IsPlayerInRange())
                    {
                        if (UnityEngine.Random.value < .5f)
                        {
                            audio.clip = attack_clip;
                        }
                        else
                        {
                            audio.clip = attack_clip2;
                        }
                    }
                    else
                    {
                        if (UnityEngine.Random.value < .5f)
                        {
                            audio.clip = whif_clip;
                        }
                        else
                        {
                            audio.clip = whif_clip2;
                        }
                    }
                    audio.Play();
                    animator.SetTrigger("Attacking");
                    break;

                case EnemyMove.BLOCK:
                    animator.SetTrigger("Blocking");
                    break;
            }
        }

        if (pmove == PlayerMove.ATTACK && Player.Instance.target == this)
        {
            if (UnityEngine.Random.value < .5f)
            {
                audio.clip = block_clip;
            }
            else
            {
                audio.clip = block_clip2;
            }
            audio.Play();
            animator.SetBool("Hit", true);
        }
        else
        {
            animator.SetBool("Hit", false);
        }

        EnemyMove currMove = PeekMove();

        if (currMove == EnemyMove.ATTACK)
        {
            moveText.text = "ATTACK";
        }
        else if (currMove == EnemyMove.BLOCK)
        {
            moveText.text = "BLOCK";
        }
        else
        {
            moveText.text = "IDLE";
        }

        slider.value = health;
    }

    public override EnemyMove GetMove()
    {
        base.GetMove();

        if (!engaged)
        {
            engaged = MinionBlackboard.Instance.GetEngagementToken(this);
        }

        if (!engaged)
        {
            return EnemyMove.IDLE;
        }

        var moves = ComboManager.GetLenientEnemyMoves();
        var rnd = new Random();

        if (moves.Count == 0)
        {
            var enumVals = Enum.GetValues(typeof(EnemyMove));
            return (EnemyMove)enumVals.GetValue(rnd.Next(enumVals.Length));
        }

        var nextMove = moves.ToArray()[MinionBlackboard.Instance.GetRand(moves.Count)];

        while (!MinionBlackboard.Instance.GetMoveToken(nextMove) || Last2Same(nextMove))
        {
            moves.Remove(nextMove);

            if (moves.Count == 0)
            {
                var enumVals = Enum.GetValues(typeof(EnemyMove));
                return (EnemyMove)enumVals.GetValue(rnd.Next(enumVals.Length));
            }

            nextMove = moves.ToArray()[rnd.Next(moves.Count)];
        }

        prevMoves.AddLast(nextMove);

        if (prevMoves.Count == 3)
        {
            prevMoves.RemoveFirst();
        }

        return nextMove;
    }

    private bool Last2Same(EnemyMove nextMove)
    {
        if (prevMoves.Count < 2)
        {
            return false;
        }

        var first = prevMoves.First;
        return first.Value == nextMove && first.Next.Value == nextMove;
    }

    public bool IsPlayerInRange()
    {
        if (Physics.Raycast(transform.position + new Vector3(0, 1, 0), transform.forward, out RaycastHit hit, HIT_RANGE))
        {
            Debug.DrawRay(transform.position + new Vector3(0, 1, 0), transform.forward * hit.distance, Color.green);

            return hit.collider.gameObject.tag == "Player";
        }
        else
        {
            Debug.DrawRay(transform.position + new Vector3(0, 1, 0), transform.forward * HIT_RANGE, Color.white);

            return false;
        }
    }
}
