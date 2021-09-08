using System;
using System.Linq;

/// <summary>
/// Randomly selects moves
/// </summary>
public class RandomBossAI : BossAI
{
    public override EnemyMove GetNextMove(PlayerMove pmove, PlayerFail failure)
        => GetRandomMove();
}