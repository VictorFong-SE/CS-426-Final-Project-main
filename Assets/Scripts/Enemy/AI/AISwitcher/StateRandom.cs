using System;

using AISwitcher;
using UnityEngine;

public class StateRandom : State<SwitcherAI>
{
    private static StateRandom _instance;

    private StateRandom()
    {
        if (_instance != null)
        {
            return;
        }

        _instance = this;
    }


    public static StateRandom Instance
    {
        get
        {
            if (_instance == null)
            {
                new StateRandom();
            }

            return _instance;
        }
    }

    public override void EnterState(SwitcherAI owner)
    {
        owner.boss.setBrain(new RandomBossAI());
        Debug.Log("StateRandom: Entered");
    }

    public override void ExitState(SwitcherAI owner)
    {
        Debug.Log("StateRandom: Exited");
    }

    public override void UpdateState(SwitcherAI owner)
    {
        switch (owner.switchState)
        {
            case SwitcherAI.CombatAI.CopyCat:
                owner.StateMachine.ChangeState(StateCopyCat.Instance);
                break;
            case SwitcherAI.CombatAI.MiniMax:
                owner.StateMachine.ChangeState(StateMiniMax.Instance);
                break;
            case SwitcherAI.CombatAI.Random:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        //other code here
    }
}