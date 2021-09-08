using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [SerializeField]
    GameObject movePanel;
    public static TimeManager Instance { get; private set; }
    private float curTime;
    private bool playerMoved;

    public bool spellReady = true;
    private PlayerMove state = PlayerMove.IDLE;
    private PlayerFail failure = PlayerFail.NONE;

    private static readonly List<IOnBeat> onBeatSystems = new List<IOnBeat>();
    private static readonly List<IOnBeat> onBeatEntities = new List<IOnBeat>();
    private bool prebeat;

    private const float WINDOW_SIZE = .4f;
    private static float bpm = 60;
    private static float tmpBpm = 0;
    private static float BeatLength { get { return 60 / (bpm + tmpBpm); } }
    public static float TimeForAnimation { get { return BeatLength - WINDOW_SIZE; } }

    private readonly Dictionary<string, float> powerups = new Dictionary<string, float>();
    private readonly Dictionary<string, int> beatsRemaining = new Dictionary<string, int>();

    // Awake is called before Start
    void Awake()
    {
        Instance = this;
        movePanel.SetActive(false);
    }

    public static void RegisterSystem(IOnBeat onBeatSystem)
    {
        onBeatSystems.Add(onBeatSystem);
    }

    public static void RegisterEntity(IOnBeat onBeatEntity)
    {
        onBeatEntities.Add(onBeatEntity);
    }

    public static bool IsRegistered(IOnBeat onBeat)
    {
        return onBeatEntities.Contains(onBeat) || onBeatSystems.Contains(onBeat);
    }

    public static bool UnregisterEntity(IOnBeat onBeatEntity)
    {
        if (!onBeatEntities.Contains(onBeatEntity))
        {
            //throw new System.ArgumentException("Can't unregister entity that was not registered");
            return false;
        }

        return onBeatEntities.Remove(onBeatEntity);
    }

    public static float GetPitch(float currentBpm)
    {
        return (bpm + tmpBpm) / currentBpm;
    }

    void UpdateOnBeats(PlayerMove curState, PlayerFail failure)
    {
        for (int i = onBeatSystems.Count - 1; i >= 0; i--)
        {
            if (i >= onBeatSystems.Count)
            {
                if (onBeatSystems.Count == 0)
                {
                    break;
                }

                i = onBeatSystems.Count - 1;
            }

            onBeatSystems[i].OnBeat(curState, failure);
        }

        for (int i = onBeatEntities.Count - 1; i >= 0; i--)
        {
            if (i >= onBeatEntities.Count)
            {
                if (onBeatEntities.Count == 0)
                {
                    break;
                }

                i = onBeatEntities.Count - 1;
            }

            onBeatEntities[i].OnBeat(curState, failure);
        }

        UI_BossMoveLookahead.Instance?.OnBeat();
    }

    // Update is called once per frame
    void Update()
    {
        curTime += Time.deltaTime;

        if (BeatLength - WINDOW_SIZE <= curTime && curTime <= BeatLength)
        {
            if (!playerMoved && !failure.IsOffbeat())
            {
                if (Input.GetKeyDown("space"))
                {
                    Player.Instance.anim.SetTrigger("kicking");
                    if (Random.value < .5f)
                    {
                        Player.Instance.audio.clip = Player.Instance.kick_clip;
                    }
                    else
                    {
                        Player.Instance.audio.clip = Player.Instance.kick_clip2;
                    }
                    Player.Instance.audio.Play();
                    state = PlayerMove.KICK;
                    playerMoved = true;
                }
                else if (Player.Instance.currentWeapon == PlayerWeapon.SWORD)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        Player.Instance.anim.SetTrigger("attacking");
                        if (Player.Instance.target != null)
                        {
                            if (Random.value < .5f)
                            {
                                Player.Instance.audio.clip = Player.Instance.attack_clip;
                            }
                            else
                            {
                                Player.Instance.audio.clip = Player.Instance.attack_clip2;
                            }
                        }
                        else
                        {
                            if (Random.value < .5f)
                            {
                                Player.Instance.audio.clip = Player.Instance.whiff_clip;
                            }
                            else
                            {
                                Player.Instance.audio.clip = Player.Instance.whiff_clip2;
                            }
                        }
                        Player.Instance.audio.Play();
                        state = PlayerMove.ATTACK;
                        playerMoved = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        if (Random.value < .5f)
                        {
                            Player.Instance.audio.clip = Player.Instance.block_clip;
                        }
                        else
                        {
                            Player.Instance.audio.clip = Player.Instance.block_clip2;
                        }
                        Player.Instance.audio.Play();
                        Player.Instance.anim.SetTrigger("blocking");
                        state = PlayerMove.BLOCK;
                        playerMoved = true;
                    }
                }
                else if (Player.Instance.currentWeapon == PlayerWeapon.MAGIC)
                {
                    if (spellReady)
                    {
                        if (Input.GetKeyDown(KeyCode.Mouse0) && Player.Instance.GetMana() > 0)
                        {
                            state = PlayerMove.FIRE;
                            Player.Instance.SpawnSpell(state);
                            playerMoved = true;
                            spellReady = false;
                        }
                        else if (Input.GetKeyDown(KeyCode.Mouse1) && Player.Instance.GetMana() > 0)
                        {
                            state = PlayerMove.ICE;
                            Player.Instance.SpawnSpell(state);
                            playerMoved = true;
                            spellReady = false;
                        }
                    }
                }
            }

            if (playerMoved && !prebeat)
            {
                prebeat = true;
                HandleBeat();

                movePanel.SetActive(true);
                var color = Color.green;
                color.a = .5f;
                movePanel.GetComponent<Image>().color = color;
            }

            if (!prebeat)
            {
                movePanel.SetActive(true);
            }
        }
        else
        {
            movePanel.SetActive(false);

            if (curTime > BeatLength)
            {
                if (!spellReady && (state == PlayerMove.IDLE) && (failure == PlayerFail.NONE))
                {
                    spellReady = true;
                }

                if (!prebeat)
                {
                    HandleBeat();
                }

                prebeat = false;
                curTime = 0.0f;
                state = PlayerMove.IDLE;
                failure = PlayerFail.NONE;

                var color = Color.gray;
                color.a = .5f;
                movePanel.GetComponent<Image>().color = color;
            }
            else if (Input.GetKeyDown(KeyCode.Mouse0)
                    || Input.GetKeyDown(KeyCode.Mouse1)
                    || Input.GetKeyDown("space"))
            {
                var color = Color.red;
                color.a = .5f;
                movePanel.GetComponent<Image>().color = color;
                failure |= PlayerFail.OFFBEAT;
            }
        }
    }

    private void HandleBeat()
    {
        foreach (IOnBeat element in onBeatEntities)
        {
            if (element is Enemy curEnemy)
            {
                EnemyMove currentMove = curEnemy.PeekMove();

                bool minion_in_range = true;

                if (curEnemy is Minion tempMinion)
                {
                    if (!tempMinion.IsPlayerInRange())
                    {
                        minion_in_range = false;
                    }
                }

                if (currentMove == EnemyMove.ATTACK && minion_in_range)
                {
                    if (state != PlayerMove.BLOCK || failure.IsOffbeat())
                    {
                        failure |= PlayerFail.INVALID;
                    }
                }
            }
        }

        UpdateOnBeats(state, failure);

        playerMoved = false;

        // Update and Cleanup Powerups
        var toDestroy = new HashSet<string>();
        foreach (var key in powerups.Keys)
        {
            if (beatsRemaining[key] == 1)
            {
                tmpBpm -= powerups[key];
                toDestroy.Add(key);
            }
            else
            {
                beatsRemaining[key]--;
            }
        }

        foreach (var key in toDestroy)
        {
            powerups.Remove(key);
            beatsRemaining.Remove(key);

            if (key == "Slowdown")
            {
                GameObject.Find("Powerup").SetActive(false);
            }
        }
    }

    public void AddBPM(float bpmAddition)
        => bpm += bpmAddition;

    public void AddPowerup(string name, float bpmChange, int numBeats)
    {
        if (numBeats <= 0)
        {
            return;
        }

        powerups[name] = bpmChange;
        beatsRemaining[name] = numBeats;
        tmpBpm += bpmChange;
    }

    public int? GetBeatsRemaining(string powerupName)
    {
        if (!beatsRemaining.ContainsKey(powerupName))
        {
            return null;
        }

        return beatsRemaining[powerupName];
    }

    public void OnDestroy()
    {
        onBeatSystems.Clear();
        onBeatEntities.Clear();
    }
}


