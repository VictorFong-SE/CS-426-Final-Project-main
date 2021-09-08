using AISwitcher;
using UnityEngine;


public class StateMiniMax : State<SwitcherAI>
{
    private static StateMiniMax instance;


    private StateMiniMax()
    {
        if (instance != null)
        {
            return;
        }

        instance = this;
    }


    public static StateMiniMax Instance
    {
        get
        {
            if (instance == null)
            {
                new StateMiniMax();
            }
            return instance;
        }
    }

    public override void EnterState(SwitcherAI owner)
    {
        owner.boss.setBrain(new MinimaxBossAI());
        Debug.Log("StateMiniMax: Entered");
    }

    public override void ExitState(SwitcherAI owner)
    {
        Debug.Log("StateMiniMax: Exited");
    }

    public override void UpdateState(SwitcherAI owner)
    {
        switch (owner.switchState)
        {
            case SwitcherAI.CombatAI.Random:
                owner.StateMachine.ChangeState(StateRandom.Instance);
                break;
            case SwitcherAI.CombatAI.CopyCat:
                owner.StateMachine.ChangeState(StateCopyCat.Instance);
                break;
        }
    }
}