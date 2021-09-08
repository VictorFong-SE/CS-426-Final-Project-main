/// <summary>
/// Gives moves chosen
/// </summary>
public interface IEnemyAI : IOnBeat
{
    /// <summary>
    /// Returns the next move planned by the enemy. Must be called only ONCE per beat.
    /// </summary>
    /// <returns>
    /// Which EnemyMove was chosen
    /// </returns>
    EnemyMove GetMove();
    
}
