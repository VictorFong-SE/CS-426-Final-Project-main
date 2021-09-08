using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles learning and moves chosen
/// </summary>
public abstract class BossAI : IEnemyAI
{
    /// <summary>
    /// Current plan for next 3 moves
    /// </summary>
    public EnemyMove[] moves;
    public readonly Queue<EnemyMove> storedMoves = new Queue<EnemyMove>();
    public readonly Queue<EnemyMove> overrideQueue = new Queue<EnemyMove>();

    public AIState aiState = AIState.Improv;
    public int maxStamina = 5;
    public int stamina;
    public int stunDuration = 3;
    public bool IsStunned { get { return stun > 0; } }
    public int stun;
    public int stunEnded;
    public EnemyMove[] stunFollowup;

    protected BossAI()
    {
        if (maxStamina < Player.MAX_HEALTH)
        {
            throw new ArgumentException($"'maxStamina' must be greater than 'Player.MAX_HEALTH' ({Player.MAX_HEALTH})");
        }

        stamina = maxStamina;
    }

    /// <summary>
    /// Returns the next three moves planned by the boss
    /// </summary>
    /// <returns>
    /// An array of length 3 containing the boss's next 3 moves
    /// </returns>
    public EnemyMove[] GetMoves()
    {
        if (moves == null)
        {
            moves = InitializeMoves();
        }

        if (aiState == AIState.Directed)
        {
            if (stun > 0)
            {
                var actualMoves = Enumerable.Range(0, stun).Select(_ => EnemyMove.IDLE).ToList();
                if (stun < 3)
                {
                    actualMoves.AddRange(moves);
                }

                return actualMoves.GetRange(0, 3).ToArray();
            }

            if (stunEnded > 0)
            {
                return GetStunFollowup();
            }
        }

        return moves;
    }

    public virtual void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        if (moves == null)
        {
            moves = InitializeMoves();
        }

        if (aiState == AIState.Directed)
        {
            if (stunEnded > 0)
            {
                stunEnded--;

                if (stunEnded == 0)
                {
                    aiState = AIState.Improv;
                }

                return;
            }

            if (stun > 0)
            {
                stun--;

                if (stun == 0)
                {
                    stunEnded = 1;
                }

                return;
            }
        }

        storedMoves.Enqueue(GetNextMove(pmove, failure));

        if (moves[0] == EnemyMove.IDLE && pmove == PlayerMove.ATTACK && failure == PlayerFail.NONE)
        {
            aiState = AIState.Directed;
            stun = stunDuration;
            PlayerMoveTracker.Unpause();
            PlayerMoveTracker.ForcedOnBeat(pmove, failure);
        }

        moves[0] = moves[1];
        moves[1] = moves[2];
        moves[2] = GetMoveOverride() ?? storedMoves.Dequeue();

        // Avoid 3-peats
        if (moves[0] == moves[1] && moves[1] == moves[2])
        {
            moves[2] = moves[2].GetDual();
        }

        stamina--;
    }

    public EnemyMove GetMove()
    {
        var moves = GetMoves();

        if (moves[0] == EnemyMove.IDLE)
        {
            stamina = maxStamina - (Player.MAX_HEALTH - Player.Instance.GetHealth());
        }

        return moves[0];
    }

    public EnemyMove? GetMoveOverride()
    {
        if (stamina == 0)
        {
            overrideQueue.Enqueue(EnemyMove.IDLE);
        }

        if (overrideQueue.Count != 0)
        {
            aiState = AIState.Directed;
            return overrideQueue.Dequeue();
        }

        return null;
    }

    private EnemyMove[] GetStunFollowup()
    {
        if (stunFollowup != null)
        {
            var tmp = stunFollowup[1];
            stunFollowup = null;
            return new EnemyMove[] { tmp, moves[0], moves[1] };
        }

        var inProgressCombos = ComboManager.GetInProgressCombos();

        if (!inProgressCombos.Any(p => p.Count == 3))
        {
            return new EnemyMove[] { GetStunFollowupSingleMove(), moves[0], moves[1] };
        }

        var fiveCombos = inProgressCombos
                            .Where(p => p.Count == 3)
                            .Select(c => ComboManager.GetPossibleCombos(c)
                                .Where(x => x.Count == 5))
                            .Where(x => x.Any())
                            .SelectMany(cs => cs);

        if (!fiveCombos.Any())
        {
            return new EnemyMove[] { GetStunFollowupSingleMove(), moves[0], moves[1] };
        }

        if (fiveCombos.Count() > 1)
        {
            throw new NotSupportedException("Should never have multiple 5 combos which share their first 3 moves");
        }

        stunEnded = 2;
        stunFollowup = GetStunFollowupTwoMoves(fiveCombos.First());
        return new EnemyMove[] { stunFollowup[0], stunFollowup[1], moves[0] };
    }

    private EnemyMove GetStunFollowupSingleMove()
    {
        var predictedMove = PlayerMoveTracker.TryPredictMove();
        var lenientMoves = ComboManager.GetLenientEnemyMoves();

        if (predictedMove == null || lenientMoves.Count == 0)
        {
            return EnemyMove.BLOCK;
        }

        var intersection = ComboManager.GetCompatibleMoves((PlayerMove)predictedMove);
        intersection.IntersectWith(lenientMoves);
        if (intersection.Count != 0)
        {
            if (intersection.Contains(EnemyMove.ATTACK))
            {
                return EnemyMove.ATTACK;
            }

            return EnemyMove.BLOCK;
        }

        return lenientMoves.OrderBy(_ => new Random().Next()).First();
    }

    private EnemyMove[] GetStunFollowupTwoMoves(List<PlayerMove> combo)
    {
        return new EnemyMove[] {
            ComboManager.GetCompatibleMoves(combo[3])
                .Contains(EnemyMove.ATTACK)
                    ? EnemyMove.ATTACK
                    : EnemyMove.BLOCK,

            ComboManager.GetCompatibleMoves(combo[4])
                .Contains(EnemyMove.ATTACK)
                    ? EnemyMove.ATTACK
                    : EnemyMove.BLOCK
        };
    }

    public abstract EnemyMove GetNextMove(PlayerMove pmove, PlayerFail failure);

    public virtual EnemyMove[] InitializeMoves()
         => new EnemyMove[] { EnemyMove.IDLE }.Concat(Enumerable.Range(0, 2).Select(_ => GetRandomMove())).ToArray();

    /// <summary>
    /// Gets random EnemyMove
    /// </summary>
    /// <returns>A random move</returns>
    protected EnemyMove GetRandomMove()
    {
        return new Random().Next(0, 2) == 1 ? EnemyMove.ATTACK : EnemyMove.BLOCK;
    }
}