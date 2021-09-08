using AISwitcher;
using UnityEngine;

public class StateCopyCat : State<SwitcherAI>
{
    private static StateCopyCat instance;

    private StateCopyCat()
    {
        if (instance != null)
        {
            return;
        }

        instance = this;
    }


    public static StateCopyCat Instance
    {
        get
        {
            if (instance == null)
            {
                new StateCopyCat();
            }
            return instance;
        }
    }

    public override void EnterState(SwitcherAI owner)
    {
        owner.boss.setBrain(new RandomBossAI());
        Debug.Log("StateCopyCat: Entered");
    }

    public override void ExitState(SwitcherAI owner)
    {
        Debug.Log("StateCopyCat: Exited");
    }

    public override void UpdateState(SwitcherAI owner)
    {
        switch (owner.switchState)
        {
            case SwitcherAI.CombatAI.Random:
                owner.StateMachine.ChangeState(StateRandom.Instance);
                break;
            case SwitcherAI.CombatAI.MiniMax:
                owner.StateMachine.ChangeState(StateMiniMax.Instance);
                break;
        }
    }
}