using System;

/// <summary>
/// Mimics the player's moves on a 3 move delay
/// </summary>
public class CopycatBossAI : BossAI
{
    /// <summary>
    /// Gets matching EnemyMove for pmove
    /// </summary>
    /// <param name="pmove">PlayerMove to find matching EnenmyMove for.</param>
    /// <returns>The matching EnemyMove</returns>
    public override EnemyMove GetNextMove(PlayerMove pmove, PlayerFail failure)
    {
        switch (pmove)
        {
            case PlayerMove.IDLE:
                return EnemyMove.IDLE;

            case PlayerMove.ATTACK:
            case PlayerMove.KICK:
                return EnemyMove.ATTACK;

            case PlayerMove.BLOCK:
                return EnemyMove.BLOCK;

            default:
                throw new NotSupportedException($"'{pmove}' not supported by '{GetType().Name}'");
        }
    }
}