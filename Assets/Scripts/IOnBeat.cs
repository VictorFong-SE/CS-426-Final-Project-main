/// <summary>
/// For classes that listen for beats
/// </summary>
public interface IOnBeat
{
    /// <summary>
    /// Update after a beat is settled. Allows scripts to react to other's moves.
    /// </summary>
    /// <param name="pmove">Player move on current beat</param>
    /// <param name="failure">If player move failed</param>
    void OnBeat(PlayerMove pmove, PlayerFail failure);
}