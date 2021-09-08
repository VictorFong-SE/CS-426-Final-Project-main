using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Tracks player moves to predict their fourth move following a stun. For use by BossAI.
/// </summary>
public class PlayerMoveTracker : MonoBehaviour, IOnBeat
{
    private static PlayerMoveTracker Instance { get; set; }

    private static bool paused = true;
    private static readonly Dictionary<PlayerMove, Dictionary<PlayerMove, Dictionary<PlayerMove, Dictionary<PlayerMove, int>>>> bayesianNetworkTable = new Dictionary<PlayerMove, Dictionary<PlayerMove, Dictionary<PlayerMove, Dictionary<PlayerMove, int>>>>();
    private static PlayerMove[] playerMoves = new PlayerMove[] { };

    void Start()
    {
        Instance = this;
        TimeManager.RegisterSystem(this);
    }

    public static void ForcedOnBeat(PlayerMove pmove, PlayerFail failure)
        => Instance.OnBeat(pmove, failure);

    public void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        if (paused || failure.IsOffbeat())
        {
            return;
        }

        switch (playerMoves.Length)
        {
            case 0:
                playerMoves = new PlayerMove[] { pmove };
                break;

            case 1:
                playerMoves = new PlayerMove[] { playerMoves[0], pmove };
                break;

            case 2:
                playerMoves = new PlayerMove[] { playerMoves[0], playerMoves[1], pmove };
                break;

            default:
                LogPlayerMove(pmove);
                playerMoves = new PlayerMove[] { playerMoves[1], playerMoves[2], pmove };
                break;
        }
    }

    private void LogPlayerMove(PlayerMove pmove)
    {
        if (playerMoves?.Length != 3)
        {
            throw new System.ArgumentException("'playerMoves' must have exactly 3 elements for LogPlayerMove");
        }

        if (!bayesianNetworkTable.ContainsKey(playerMoves[0]))
        {
            bayesianNetworkTable[playerMoves[0]] = new Dictionary<PlayerMove, Dictionary<PlayerMove, Dictionary<PlayerMove, int>>>();
        }

        if (!bayesianNetworkTable[playerMoves[0]].ContainsKey(playerMoves[1]))
        {
            bayesianNetworkTable[playerMoves[0]][playerMoves[1]] = new Dictionary<PlayerMove, Dictionary<PlayerMove, int>>();
        }

        if (!bayesianNetworkTable[playerMoves[0]][playerMoves[1]].ContainsKey(playerMoves[2]))
        {
            bayesianNetworkTable[playerMoves[0]][playerMoves[1]][playerMoves[2]] = new Dictionary<PlayerMove, int>();
        }

        if (!bayesianNetworkTable[playerMoves[0]][playerMoves[1]][playerMoves[2]].ContainsKey(pmove))
        {
            bayesianNetworkTable[playerMoves[0]][playerMoves[1]][playerMoves[2]][pmove] = 1;
        }
        else
        {
            bayesianNetworkTable[playerMoves[0]][playerMoves[1]][playerMoves[2]][pmove]++;
        }
    }

    /// <summary>
    /// Pauses tracker
    /// </summary>
    /// <returns>
    /// If a change occured.
    /// </returns>
    public static bool Pause()
    {
        playerMoves = new PlayerMove[] { };

        var oldPaused = paused;
        paused = true;
        return !oldPaused;
    }

    /// <summary>
    /// Unpauses tracker
    /// </summary>
    /// <returns>
    /// If a change occured.
    /// </returns>
    public static bool Unpause()
    {
        var oldPaused = paused;
        paused = false;
        return oldPaused;
    }

    private static bool CanPredict()
    {
        if (playerMoves?.Length != 3)
        {
            return false;
        }

        return bayesianNetworkTable.ContainsKey(playerMoves[0])
            && bayesianNetworkTable[playerMoves[0]].ContainsKey(playerMoves[1])
            && bayesianNetworkTable[playerMoves[0]][playerMoves[1]].ContainsKey(playerMoves[2]);
    }

    private static PlayerMove PredictMove()
    {
        if (playerMoves?.Length != 3)
        {
            throw new System.ArgumentException("'playerMoves' must have exactly 3 elements for LogPlayerMove");
        }

        var counts = bayesianNetworkTable[playerMoves[0]][playerMoves[1]][playerMoves[2]];
        var bestCount = counts.Values.Max();
        return counts.First(x => x.Value == bestCount).Key;
    }

    public static PlayerMove? TryPredictMove()
    {
        if (CanPredict())
        {
            return PredictMove();
        }
        else
        {
            return null;
        }
    }
}