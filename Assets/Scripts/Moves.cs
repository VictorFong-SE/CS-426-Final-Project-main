using System;

/// <summary>
/// Possible moves for player
/// </summary>
public enum PlayerMove
{
    IDLE,
    ATTACK,
    BLOCK,
    KICK,
    FIRE,
    ICE
}

/// <summary>
/// Possible reasons for player failing a move
/// </summary>
[Flags]
public enum PlayerFail
{
    NONE,
    OFFBEAT,
    INVALID,
}

public static class PlayerFailExtensions
{
    public static bool IsOffbeat(this PlayerFail failure)
    {
        return (failure & PlayerFail.OFFBEAT) != 0;
    }

    public static bool IsInvalid(this PlayerFail failure)
    {
        return (failure & PlayerFail.INVALID) != 0;
    }
}

/// <summary>
/// Possible moves for enemy
/// </summary>
public enum EnemyMove
{
    IDLE,
    ATTACK,
    BLOCK
}

public static class EnemyMoveExtensions
{
    public static EnemyMove GetDual(this EnemyMove move)
    {
        switch (move)
        {
            case EnemyMove.ATTACK:
                return EnemyMove.BLOCK;

            case EnemyMove.BLOCK:
                return EnemyMove.ATTACK;

            case EnemyMove.IDLE:
                return EnemyMove.IDLE;
        }

        throw new ArgumentException($"'move' ({move}) is not handled by 'GetDual'");
    }
}
