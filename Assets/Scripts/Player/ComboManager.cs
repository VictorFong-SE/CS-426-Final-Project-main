using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComboManager : MonoBehaviour, IOnBeat
{
    public static ComboManager Instance { get; private set; }
    public static List<PlayerMove> CompletedCombo { get; private set; }
    public static bool ComboBroken { get; private set; }

    public const int COMBO_MIN = 3;
    public const int COMBO_MAX = 5;

    private readonly List<List<PlayerMove>> currentBranches = new List<List<PlayerMove>>();
    public static readonly Trie<PlayerMove> validCombos = new Trie<PlayerMove>();
    private static HashSet<EnemyMove> possibleMoves = new HashSet<EnemyMove>();


    // Start is called before the first frame update
    void Start()
    {
        foreach (var combo in COMBOS)
        {
            validCombos.Add(combo);
        }

        Instance = this;
        TimeManager.RegisterSystem(this);
    }

    // Update is called once per frame
    public void Update()
    {
        var longestCombo = GetCurrentLongestCombo();

        if (COMBO_MIN - 1 <= longestCombo.Count && longestCombo.Count <= COMBO_MAX - 1)
        {
            var possibleCombos = GetPossibleCombos(longestCombo);

            if (possibleCombos.Count != 0)
            {
                possibleMoves = new HashSet<EnemyMove>();
                var maxCount = possibleCombos.Max(c => c.Count);
                foreach (var combo in possibleCombos.Where(c => c.Count == maxCount))
                {
                    if (combo.Count != longestCombo.Count + 1)
                    {
                        continue;
                    }

                    possibleMoves.UnionWith(GetCompatibleMoves(combo[combo.Count - 1]));
                }
            }
            else
            {
                possibleMoves = new HashSet<EnemyMove>() { EnemyMove.ATTACK, EnemyMove.BLOCK };
            }
        }
        else
        {
            possibleMoves = new HashSet<EnemyMove>() { EnemyMove.ATTACK, EnemyMove.BLOCK };
        }
    }

    /// <summary>
    /// Gives set of EnemyMove's that allow given PlayerMove
    /// </summary>
    /// <param name="pmove">Move player wants</param>
    /// <returns>Set of enemy moves with allow given player move</returns>
    public static HashSet<EnemyMove> GetCompatibleMoves(PlayerMove pmove)
    {
        switch (pmove)
        {
            case PlayerMove.ATTACK:
            case PlayerMove.KICK:
                return new HashSet<EnemyMove>() { EnemyMove.BLOCK };

            case PlayerMove.BLOCK:
                return new HashSet<EnemyMove>() { EnemyMove.ATTACK };

            default:
                return new HashSet<EnemyMove>() { EnemyMove.ATTACK, EnemyMove.BLOCK };
        }
    }

    public void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        List<PlayerMove> winner = null;
        CompletedCombo = null;
        ComboBroken = false;

        if (failure != PlayerFail.NONE || pmove == PlayerMove.IDLE)
        {
            currentBranches.Clear();
            ComboBroken = true;
        }
        else
        {
            currentBranches.ForEach(l => l.Add(pmove));
            currentBranches.Add(new List<PlayerMove>() { pmove });

            for (var i = currentBranches.Count - 1; 0 <= i; i--)
            {
                var b = currentBranches[i];

                if (b.Count < COMBO_MIN)
                {
                    continue;
                }

                if (b.Count > COMBO_MAX)
                {
                    currentBranches.RemoveAt(i);
                    continue;
                }

                if (IsValidCombo(b))
                {
                    winner = b;
                    break;
                }

                if (!IsPossibleCombo(b))
                {
                    currentBranches.RemoveAt(i);
                }
            }
        }

        var target = Player.Instance.target;
        if (winner != null)
        {
            currentBranches.Clear();
            CompletedCombo = winner;
            var boss = target is Boss;
            var killed = target?.Damage(GetComboDamage(winner) + Player.Instance.DamageBoost);

            if (killed == true)
            {
                if (boss)
                {
                    Player.Instance.Heal();
                    TimeManager.Instance.AddBPM(10);
                }
                else { Player.Instance.Heal(1); }
            }
        }
        else
        {
            if (target != null && !failure.IsOffbeat() && (pmove == PlayerMove.ATTACK || pmove == PlayerMove.KICK))
            {
                if (target is Enemy enemy)
                {
                    if (enemy.PeekMove() != EnemyMove.BLOCK)
                    {
                        target?.Damage(2 + Player.Instance.DamageBoost);
                    }
                    else
                    {
                        target?.Damage(1 + Player.Instance.DamageBoost);
                    }
                }
                else if (target is MultiTarget targets)
                {
                    var ts = targets.GetTargets();

                    for (var i = ts.Count - 1; i >= 0; i--)
                    {
                        if (((Enemy)ts[i]).PeekMove() != EnemyMove.BLOCK)
                        {
                            ts[i]?.Damage(2 + Player.Instance.DamageBoost);
                        }
                        else
                        {
                            ts[i]?.Damage(1 + Player.Instance.DamageBoost);
                        }
                    }
                }
            }
        }
    }

    private static readonly PlayerMove[][] COMBOS = new PlayerMove[][] {
        new PlayerMove[] {PlayerMove.ATTACK, PlayerMove.ATTACK, PlayerMove.ATTACK},
        new PlayerMove[] {PlayerMove.ATTACK, PlayerMove.ATTACK, PlayerMove.KICK},
        new PlayerMove[] {PlayerMove.BLOCK, PlayerMove.BLOCK, PlayerMove.ATTACK, PlayerMove.ATTACK},
        new PlayerMove[] {PlayerMove.ATTACK, PlayerMove.KICK, PlayerMove.ATTACK, PlayerMove.BLOCK},
        new PlayerMove[] {PlayerMove.BLOCK, PlayerMove.ATTACK, PlayerMove.BLOCK, PlayerMove.ATTACK, PlayerMove.BLOCK},
        new PlayerMove[] {PlayerMove.KICK, PlayerMove.BLOCK, PlayerMove.BLOCK, PlayerMove.ATTACK, PlayerMove.KICK},
        new PlayerMove[] {PlayerMove.KICK, PlayerMove.ATTACK, PlayerMove.ATTACK, PlayerMove.BLOCK, PlayerMove.KICK},
    };

    public static int GetComboDamage(IEnumerable<PlayerMove> playerMoves)
    {
        return GetComboDamage(playerMoves.ToArray());
    }

    public static int GetComboDamage(PlayerMove[] playerMoves)
    {
        if (playerMoves.Length < COMBO_MIN)
        {
            throw new System.ArgumentException($"'playerMoves' must be of length greater than or equal to {COMBO_MIN}");
        }
        else if (playerMoves.Length > COMBO_MAX)
        {
            throw new System.ArgumentException($"'playerMoves' must be of length less than or equal to {COMBO_MAX}");
        }

        if (!IsValidCombo(playerMoves))
        {
            return 0;
        }

        switch (playerMoves.Length)
        {
            case 3: return 10;
            case 4: return 14;
            case 5: return 28;
            default: return 0;
        }
    }

    /// <summary>
    /// Says if a list of PlayerMove's is a valid combo
    /// </summary>
    /// <param name="pmoves">List of PlayerMove's to check</param>
    /// <returns>If given moves form a completed combo.</returns>
    public static bool IsValidCombo(IEnumerable<PlayerMove> pmoves)
    {
        return validCombos.IsComplete(pmoves);
    }

    /// <summary>
    /// Says if a list of PlayerMove's is the beginning of a combo
    /// </summary>
    /// <param name="pmoves">List of PlayerMove's to check</param>
    /// <returns>If given moves are the start of a combo.</returns>
    public static bool IsPossibleCombo(IEnumerable<PlayerMove> pmoves)
    {
        return GetPossibleCombos(pmoves).Count != 0;
    }

    /// <summary>
    /// Gives all move sequences that could become combos.
    /// </summary>
    /// <returns>List of List's of PlayerMove's that are the start of a combo</returns>
    public static List<List<PlayerMove>> GetInProgressCombos()
    {
        return Instance.currentBranches.ConvertAll(l => l.Concat(new List<PlayerMove>()).ToList());
    }

    /// <summary>
    /// Returns a list of combos, the given list of PlayerMove could become.
    /// </summary>
    /// <param name="pmoves">List of PlayerMove's to check.</param>
    /// <returns>List of combos which the given moves could end up as.</returns>
    public static List<List<PlayerMove>> GetPossibleCombos(IEnumerable<PlayerMove> pmoves)
    {
        return validCombos.GetCompletions(pmoves);
    }

    /// <summary>
    /// Gives longest move sequence that could become a combo.
    /// </summary>
    /// <returns>Longest List of PlayerMove's that are the start of a combo</returns>
    public static List<PlayerMove> GetCurrentLongestCombo()
    {
        return Instance.currentBranches.OrderBy(l => l.Count).LastOrDefault() ?? new List<PlayerMove>();
    }

    /// <summary>
    /// Gives set of EnemyMove that allow a combo to continue.
    /// </summary>
    /// <returns>Set of moves that won't force the player to lose all in-progress combos.</returns>
    public static HashSet<EnemyMove> GetLenientEnemyMoves()
    {
        return possibleMoves;
    }
}
