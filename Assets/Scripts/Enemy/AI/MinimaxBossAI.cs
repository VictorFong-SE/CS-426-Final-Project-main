
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Selects next move based on Minimax Algo
/// </summary>
public class MinimaxBossAI : BossAI
{
    private const int MAX_DEPTH = 3;

    public MinimaxBossAI()
        : base()
    {
        if (MAX_DEPTH <= 2)
        {
            throw new ArgumentException($"'MAX_DEPTH' must be greater than 2, is ({MAX_DEPTH})");
        }
    }

    /// <summary>
    /// Gets next EnemyMove by minimax
    /// </summary>
    /// <returns>A minimaxed move</returns>
    public override EnemyMove GetNextMove(PlayerMove pmove, PlayerFail failure)
    {
        // build a list of the moves the enemy is about to account for it in algo
        List<EnemyMove> leftOverEnemyMoves = GetMoves().ToList().GetRange(1, 2);

        // create empty lists to be used in figuring out static evaluation of moves when depth reached
        List<PlayerMove> curPlayerMoves = new List<PlayerMove>();
        List<EnemyMove> curEnemyMoves = new List<EnemyMove>();

        // call minimax to get the next move
        return Minimax(leftOverEnemyMoves, curEnemyMoves, curPlayerMoves, 0, true).Item2;
    }

    private (int, EnemyMove) Minimax(List<EnemyMove> leftOverEnemyMoves, List<EnemyMove> curEnemyMoves, List<PlayerMove> curPlayerMoves, int depth, bool isMaximizing)
    {
        // if we've reached our max depth then evaluate the positions
        if (depth == MAX_DEPTH)
        {
            return (EvaluateMoves(curEnemyMoves, curPlayerMoves), curEnemyMoves[curEnemyMoves.Count - 1]);
        }

        // if we are at the enemy ture
        if (isMaximizing)
        {
            // if we still have enemy moves to process then do this
            if (leftOverEnemyMoves.Count > 0)
            {
                // add moves to the current move list (essentially always choose this move), remove from leftover, then move to player turn
                EnemyMove temp = leftOverEnemyMoves[0];
                leftOverEnemyMoves.RemoveAt(0);
                return Minimax(leftOverEnemyMoves, curEnemyMoves.Concat(new List<EnemyMove>() { temp }).ToList(), curPlayerMoves, depth, false);
            }

            // if we don't have any precalculated moves left then do this
            else
            {
                // Attack and Block start a branch

                // start attack branch
                var score1 = Minimax(leftOverEnemyMoves, curEnemyMoves.Concat(new List<EnemyMove>() { EnemyMove.ATTACK }).ToList(), curPlayerMoves, depth, false);

                // start block branch and do the same thing
                var score2 = Minimax(leftOverEnemyMoves, curEnemyMoves.Concat(new List<EnemyMove>() { EnemyMove.BLOCK }).ToList(), curPlayerMoves, depth, false);

                // find the best score and return appropriate values
                if (score1.Item1 > score2.Item1)
                {
                    if (depth == 2)
                        return (score1.Item1, EnemyMove.ATTACK);
                    else
                        return score1;
                }
                else
                {
                    if (depth == 2)
                        return (score2.Item1, EnemyMove.BLOCK);
                    else
                        return score2;
                }
            }
        }
        else
        {
            // get the most recent enemy move and make branches on that
            switch (curEnemyMoves[curEnemyMoves.Count - 1])
            {
                // assuming optimal play the player would only block in this case
                case EnemyMove.ATTACK:
                    return Minimax(leftOverEnemyMoves, curEnemyMoves, curPlayerMoves.Concat(new List<PlayerMove>() { PlayerMove.BLOCK }).ToList(), depth + 1, true);
                // in this case the user could do anything
                case EnemyMove.IDLE:
                case EnemyMove.BLOCK:
                default:
                    int newDepth = depth + 1;

                    // start attack branch
                    (int, EnemyMove) scoreAttack = Minimax(leftOverEnemyMoves, curEnemyMoves, curPlayerMoves.Concat(new List<PlayerMove>() { PlayerMove.ATTACK }).ToList(), newDepth, true);

                    // start kick branch
                    (int, EnemyMove) scoreKick = Minimax(leftOverEnemyMoves, curEnemyMoves, curPlayerMoves.Concat(new List<PlayerMove>() { PlayerMove.KICK }).ToList(), newDepth, true);

                    // start block branch
                    (int, EnemyMove) scoreBlock = Minimax(leftOverEnemyMoves, curEnemyMoves, curPlayerMoves.Concat(new List<PlayerMove>() { PlayerMove.BLOCK }).ToList(), newDepth, true);

                    // find the minimum score
                    int minScore = Math.Min(scoreAttack.Item1, Math.Min(scoreKick.Item1, scoreBlock.Item1));

                    // based on the minimum score return appropriate tuple
                    if (minScore == scoreAttack.Item1)
                    {
                        return scoreAttack;
                    }
                    else if (minScore == scoreKick.Item1)
                    {
                        return scoreKick;
                    }
                    else
                    {
                        return scoreBlock;
                    }
            }
        }
    }

    // unfinished
    private int EvaluateMoves(List<EnemyMove> curEnemyMoves, List<PlayerMove> curPlayerMoves)
    {
        List<PlayerMove> selected = null;

        foreach (var i in Enumerable.Range(1, curPlayerMoves.Count))
        {
            bool ret = false;

            foreach (var inprog in ComboManager.GetInProgressCombos())
            {
                int tempCount = inprog.Count;

                inprog.AddRange(curPlayerMoves);

                var playerMoves = inprog.GetRange(0, tempCount + i);

                if (ComboManager.IsValidCombo(playerMoves))
                {
                    selected = inprog;
                    ret = true;
                    break;
                }
            }

            if (ret)
                break;
        }
        int score = 0;
        HashSet<int> comboIndices = new HashSet<int>();

        if (selected != null)
        {
            int inprogLen = selected.Count - curPlayerMoves.Count;

            List<PlayerMove> temp = new List<PlayerMove>();
            for (int i = 0; i < selected.Count; i++)
            {
                temp.Add(selected[i]);

                if (ComboManager.IsValidCombo(temp))
                {
                    comboIndices.Add(i - inprogLen);
                    score -= ComboManager.GetComboDamage(temp);
                    temp.Clear();
                }
            }
        }

        for (int i = 0; i < curEnemyMoves.Count; i++)
        {
            if (comboIndices.Contains(i))
                continue;

            EnemyMove curEnem = curEnemyMoves[i];
            PlayerMove curPlay = curPlayerMoves[i];

            if (curEnem == EnemyMove.IDLE && (curPlay == PlayerMove.ATTACK || curPlay == PlayerMove.KICK))
                score--;
        }

        return score;
    }
}
