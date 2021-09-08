using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, IOnBeat
{
    public static Player Instance { get; private set; }

    public const int MAX_HEALTH = 3;
    private static int health = MAX_HEALTH;

    public const int MAX_MANA = 10;
    private int mana = 10;

    public int DamageBoost { get; private set; } = 0;

    public ITarget target;

    public const int MAX_SPEED = 500;

    private Rigidbody rb;

    private Camera camera;
    private Transform cameraTransform;

    public PlayerWeapon currentWeapon = PlayerWeapon.SWORD;

    [SerializeField]
    public GameObject fireSpell;

    [SerializeField]
    public GameObject iceSpell;

    [SerializeField]
    public Transform cannon;

    [SerializeField]
    public Animator anim;

    [SerializeField]
    public bool inBoss = false;

    private GameObject crosshair;
    private bool mouseCaptured = true;

    public const float PLAYER_RANGE = 2;
    public const float PLAYER_SPELL_RANGE = 15;
    public const float SWEEP_ARC = 180; // Degrees

    [SerializeField]
    public GameObject sword;

    [SerializeField]
    public GameObject shield;

    [SerializeField]
    public AudioSource audio;

    public AudioClip attack_clip;
    public AudioClip attack_clip2;

    public AudioClip whiff_clip;
    public AudioClip whiff_clip2;

    public AudioClip block_clip;
    public AudioClip block_clip2;

    public AudioClip kick_clip;
    public AudioClip kick_clip2;

    public AudioClip spell_clip;
    public AudioClip spell_clip2;

    public AudioClip damage_clip;
    public AudioClip damage_clip2;

    [SerializeField]
    public RawImage crosshairImage;

    [SerializeField]
    public Texture onTarget;

    [SerializeField]
    public Texture defaultCrossHair;

    // Start is called before the first frame update
    void Start()
    {
        target = null;
        Instance = this;
        rb = GetComponentInChildren<Rigidbody>();

        if (!inBoss)
        {
            camera = GetComponentInChildren<Camera>();
            cameraTransform = camera.transform;
        }

        crosshair = GameObject.Find("Crosshair");

        TimeManager.RegisterEntity(this);

        if (!inBoss)
        {
            if (!CheckpointManager.FirstSave)
            {
                CheckpointManager.FirstSave = true;
                CheckpointManager.SaveGame();
            }
            else
            {
                CheckpointManager.LoadGame();
            }
        }
        else
        {
            if (!CheckpointManager.AtBoss)
            {
                CheckpointManager.AtBoss = true;
                CheckpointManager.SaveGame();
            }
            else
            {
                CheckpointManager.LoadGame();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            mouseCaptured = !mouseCaptured;
        }

        if (mouseCaptured)
        {
            Cursor.lockState = CursorLockMode.Locked; // freeze cursor on screen centre
            Cursor.visible = false; // invisible cursor
        }
        else
        {
            Cursor.lockState = CursorLockMode.None; // unlock cursor
            Cursor.visible = true; // visible cursor
        }

        if (!inBoss)
        {
            Vector3 the_position = transform.position;
            the_position.y += 1;

            float cur_range = 0;

            switch (currentWeapon)
            {
                case PlayerWeapon.SWORD:
                    cur_range = PLAYER_RANGE;
                    break;
                case PlayerWeapon.MAGIC:
                    cur_range = PLAYER_SPELL_RANGE;
                    break;
            }

            if (currentWeapon == PlayerWeapon.MAGIC)
            {
                var pos = camera.ViewportToWorldPoint(new Vector3(.5f, .5f, 0));
                if (Physics.Raycast(pos, cameraTransform.forward, out RaycastHit hit, cur_range, ~(1 << 3))) // Ignore player
                {
                    Debug.DrawRay(pos, cameraTransform.forward * hit.distance, Color.green);

                    target = hit.collider.GetComponent<Enemy>();

                    if (target != null)
                        crosshairImage.texture = onTarget;
                    else
                        crosshairImage.texture = defaultCrossHair;
                }
                else
                {
                    Debug.DrawRay(pos, cameraTransform.forward * cur_range, Color.red);

                    target = null;

                    crosshairImage.texture = defaultCrossHair;
                }
            }
            else
            {
                var myPos = transform.position;
                var targets = FindObjectsOfType<Minion>()
                              .Where(m => Vector3.Distance(myPos, m.transform.position) <= PLAYER_RANGE
                                  && Mathf.Abs(Vector3.Angle(m.transform.position - myPos, transform.forward)) < SWEEP_ARC / 2.0f
                              ).ToList();

                if (targets.Count == 0)
                {
                    target = null;
                }
                else
                {
                    target = new MultiTarget(targets);
                }
            }

            crosshair.SetActive(currentWeapon == PlayerWeapon.MAGIC);

            var desiredVel = cameraTransform.rotation * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            desiredVel.y = 0;

            // Don't need straferunning
            desiredVel = desiredVel.normalized;

            if (desiredVel.magnitude >= 0.1f)
            {
                desiredVel = MAX_SPEED * desiredVel * .013f;
                rb.velocity = new Vector3(desiredVel.x, rb.velocity.y, desiredVel.z);
            }
            else
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }

            transform.eulerAngles = new Vector3(0.0f, cameraTransform.eulerAngles.y, 0.0f);

            if (Input.GetKeyDown(KeyCode.Q) && (currentWeapon == PlayerWeapon.SWORD))
            {
                anim.SetTrigger("sheathing");
                StartCoroutine("DetachSwordAndShield");
                currentWeapon = PlayerWeapon.MAGIC;
            }

            if (Input.GetKeyDown(KeyCode.E) && (currentWeapon == PlayerWeapon.MAGIC))
            {
                currentWeapon = PlayerWeapon.SWORD;
                anim.SetTrigger("drawing");
                StartCoroutine("AttachSwordAndShield");
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 25))
                {
                    var interactive = hit.collider.GetComponent<IInteractive>();

                    if (interactive == null || interactive.Range < hit.distance)
                    {
                        Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.red);
                    }
                    else
                    {
                        Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.green);

                        interactive.OnInteract(gameObject);
                    }
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.forward * 25, Color.white);
                }
            }
        }
    }

    public void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        if (failure.IsInvalid())
        {
            Damage(1);
        }
    }

    public void Damage(int amount)
    {
        health -= amount;
        if (Random.value < .5f)
        {
            audio.clip = damage_clip;
        }
        else
        {
            audio.clip = damage_clip2;
        }
        audio.Play();

        if (health <= 0)
        {
            GameOverManager.GameOver();
        }
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int newHealth)
    {
        health = newHealth;
    }

    public int GetMana()
    {
        return mana;
    }

    public void SetMana(int newMana)
    {
        mana = newMana;
    }

    public void Heal()
    {
        Heal(MAX_HEALTH);
    }

    public void AddMana(int amt)
    {
        mana = Mathf.Min(mana + amt, MAX_MANA);
    }

    public void SetMaxMana()
    {
        mana = MAX_MANA;
    }

    public void Heal(int amt)
    {
        health = Mathf.Min(health + amt, MAX_HEALTH);
    }

    public void AddDamageBoost(int damageBoost)
    {
        DamageBoost += damageBoost;
    }

    public void SetBossState(Enemy new_target)
    {
        target = new_target;

        inBoss = true;
    }

    public void SpawnSpell(PlayerMove spell)
    {
        GameObject cur_projectile = null;

        switch (spell)
        {
            case PlayerMove.FIRE:
                cur_projectile = fireSpell;
                break;
            case PlayerMove.ICE:
                cur_projectile = iceSpell;
                break;
            default:
                return;
        }

        GameObject newSpell = GameObject.Instantiate(cur_projectile, cannon.position, cannon.rotation) as GameObject;

        if (target != null)
        {
            newSpell.GetComponent<SpellController>().target = target.GetGameObject().transform;
        }

        AddMana(-1);
        anim.SetTrigger("casting");
        if (Random.value < .5f)
        {
            audio.clip = spell_clip;
        }
        else
        {
            audio.clip = spell_clip2;
        }
        audio.Play();
    }

    IEnumerator DetachSwordAndShield()
    {
        float duration = .75f;
        yield return new WaitForSeconds(duration);
        shield.SetActive(false);
        sword.SetActive(false);
    }

    IEnumerator AttachSwordAndShield()
    {
        float duration = .65f;
        yield return new WaitForSeconds(duration);
        shield.SetActive(true);
        sword.SetActive(true);
    }
}
