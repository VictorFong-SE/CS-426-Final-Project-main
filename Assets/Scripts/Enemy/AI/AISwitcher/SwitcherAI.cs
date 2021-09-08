using UnityEngine;
using AISwitcher;

[SerializeField]
public class SwitcherAI
{

    public enum CombatAI
    {
        Random = 0,
        CopyCat = 1,
        MiniMax = 2
    }

    public CombatAI switchState;

    public Boss boss;

    public StateMachine<SwitcherAI> StateMachine { get; set; }


    // Start is called before the first frame update
    public SwitcherAI(Boss boss)
    {
        Debug.Log("SwitcherAI: Start called.");
        this.boss = boss;
        StateMachine = new StateMachine<SwitcherAI>(this);
        StateMachine.ChangeState(StateRandom.Instance);
    }

    // Update is called once per frame
    public void OnBeat()
    {
        Debug.Log("SwitcherAI: Update called.");
        int hp = boss.GetHealth();
        switch (hp > 80 && hp < 120 ? "High" :
            hp > 40 && hp < 80 ? "Mid" :
            hp > 0 && hp < 40 ? "Low" : "Floor")
        {
            case "High":
                if (switchState != CombatAI.Random)
                    StateMachine.ChangeState(StateRandom.Instance);
                break;
            case "Mid":
                if (switchState != CombatAI.CopyCat)
                    StateMachine.ChangeState(StateCopyCat.Instance);

                break;
            case "Low":
                if (switchState != CombatAI.MiniMax)
                    StateMachine.ChangeState(StateMiniMax.Instance);

                break;
            case "Floor":
                break;
        }

    }
}