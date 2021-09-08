using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MinionBlackboard : MonoBehaviour, IOnBeat
{
    public static MinionBlackboard Instance { get; private set; }

    public const uint ATTACKING_MAX = 1;
    public const uint BLOCKING_MAX = 1;
    public const uint ENGAGING_MAX = 2;
    public const uint GROUP_STAMINA_MAX = 5;

    private readonly Dictionary<EnemyMove, uint> moveTokenCount = new Dictionary<EnemyMove, uint>();

    private readonly HashSet<Minion> engaged = new HashSet<Minion>();
    private readonly HashSet<Minion> engagedForced = new HashSet<Minion>();

    private static uint stamina = GROUP_STAMINA_MAX;

    void Start()
    {
        Instance = this;

        foreach (var val in Enum.GetValues(typeof(EnemyMove)))
        {
            moveTokenCount[(EnemyMove)val] = 0;
        }

        TimeManager.RegisterSystem(this);
    }

    public void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        if (stamina != 0)
        {
            stamina--;
        }
        else
        {
            stamina = GROUP_STAMINA_MAX;
        }

        randVal = null;
    }

    public bool GetMoveToken(EnemyMove enemyMove)
    {
        if (stamina == 0)
        {
            if (moveTokenCount[EnemyMove.IDLE] == 0 && enemyMove != EnemyMove.IDLE)
            {
                return false;
            }

            if (enemyMove == EnemyMove.ATTACK)
            {
                return false;
            }

            return true;
        }

        switch (enemyMove)
        {
            case EnemyMove.IDLE:
                moveTokenCount[EnemyMove.IDLE]++;
                return true;

            case EnemyMove.ATTACK:
                if (moveTokenCount[EnemyMove.ATTACK] < ATTACKING_MAX)
                {
                    moveTokenCount[EnemyMove.ATTACK]++;
                    return true;
                }
                return false;

            case EnemyMove.BLOCK:
                if (moveTokenCount[EnemyMove.BLOCK] < BLOCKING_MAX)
                {
                    moveTokenCount[EnemyMove.BLOCK]++;
                    return true;
                }
                return false;
        }

        return false;
    }

    public void ReturnMoveToken(EnemyMove enemyMove)
    {
        switch (enemyMove)
        {
            case EnemyMove.IDLE:
                moveTokenCount[EnemyMove.IDLE]--;
                return;

            case EnemyMove.ATTACK:
                moveTokenCount[EnemyMove.ATTACK]--;
                return;

            case EnemyMove.BLOCK:
                moveTokenCount[EnemyMove.BLOCK]--;
                return;
        }
    }

    public bool GetEngagementToken(Minion asker)
    {
        if (engaged.Count + engagedForced.Count < ENGAGING_MAX)
        {
            engaged.Add(asker);
            return true;
        }

        return false;
    }

    public bool GetEngagementTokenForced(Minion asker)
    {
        if (engagedForced.Contains(asker))
        {
            return true;
        }

        if (engaged.Contains(asker))
        {
            engaged.Remove(asker);
            engagedForced.Add(asker);
            return true;
        }

        if (engaged.Count + engagedForced.Count < ENGAGING_MAX)
        {
            engagedForced.Add(asker);
            return true;
        }

        if (engaged.Count > 0)
        {
            ReturnEngagedFrom(engaged);
        }
        else
        {
            ReturnEngagedFrom(engagedForced);
        }

        engagedForced.Add(asker);

        return true;
    }

    private void ReturnEngagedFrom(HashSet<Minion> minions)
    {
        var playerPos = Player.Instance.transform.position;
        var toRemove = minions.OrderBy(m => Vector3.Distance(playerPos, m.transform.position)).Last();

        toRemove.engaged = false;
        minions.Remove(toRemove);
    }

    public void ReturnEngagementToken(Minion returner)
    {
        if (engaged.Contains(returner))
        {
            engaged.Remove(returner);
        }
        else if (engagedForced.Contains(returner))
        {
            engagedForced.Remove(returner);
        }
    }

    private int? randVal = null;

    public int GetRand(int size)
    {
        if (randVal == null || size <= randVal)
        {
            randVal = Random.Range(0, size);
        }

        return (int)randVal;
    }

    public HashSet<Minion> GetEngagedMinions()
    {
        var ret = new HashSet<Minion>();

        foreach (var minion in engaged)
        {
            ret.Add(minion);
        }

        return ret;
    }
}