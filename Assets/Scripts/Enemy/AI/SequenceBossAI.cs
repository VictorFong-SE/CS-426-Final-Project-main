using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Repeats preset moves
/// </summary>
public class SequenceBossAI : BossAI
{
    /// <summary>
    /// Extended plan for future N moves after current 3, mattering on preset sequence length.
    /// </summary>
    private List<EnemyMove> futureMoves;

    /// <summary>
    /// List of preset move sequences
    /// </summary>
    private readonly EnemyMove[][] presetSequences;

    public SequenceBossAI(EnemyMove[][] presetSequences)
    {
        if (presetSequences.Select(x => x.Length).Sum() < 3)
        {
            throw new ArgumentException("'presetSequences' must have a sum of sequence length greater than 3.");
        }

        this.presetSequences = presetSequences;
    }

    public override EnemyMove[] InitializeMoves()
    {
        futureMoves = GenerateMoves().ToList();

        var ret = new EnemyMove[] { EnemyMove.IDLE }.Concat(futureMoves.GetRange(0, 2)).ToArray();
        futureMoves.RemoveRange(0, 3);
        return ret;
    }

    public override EnemyMove GetNextMove(PlayerMove pmove, PlayerFail failure)
    {
        if (futureMoves.Count <= 1)
        {
            futureMoves.AddRange(GenerateMoves());
        }

        var ret = futureMoves[0];
        futureMoves.RemoveAt(0);
        return ret;
    }

    /// <summary>
    /// Gives a shuffled list of EnemyMove based on the preset sequences
    /// </summary>
    /// <returns>An IEnumerable of moves.</returns>
    private IEnumerable<EnemyMove> GenerateMoves()
    {
        var rnd = new Random();
        return presetSequences
                .OrderBy(_ => rnd.Next())
                .Select(x => x.AsEnumerable())
                .Aggregate((a, b) => a.Concat(b));
    }
}