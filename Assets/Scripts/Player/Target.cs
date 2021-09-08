using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITarget
{
    bool Damage(int damage);

    bool IsTarget(ITarget target);

    GameObject GetGameObject();
}

public class MultiTarget : ITarget
{
    private readonly List<ITarget> targets;

    public MultiTarget(IEnumerable<ITarget> targets)
    {
        this.targets = targets.ToList();
    }

    public bool Damage(int damage)
    {
        bool ret = false;
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            ret |= targets[i].Damage(damage);
        }
        return ret;
    }

    public bool IsTarget(ITarget target)
    {
        return targets.Contains(target);
    }

    public GameObject GetGameObject()
    {
        return targets[0].GetGameObject();
    }

    public List<ITarget> GetTargets()
    {
        return targets.Concat(new List<ITarget>()).ToList();
    }
}