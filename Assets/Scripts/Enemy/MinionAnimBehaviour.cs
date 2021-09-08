using UnityEngine;

public class MinionAnimBehaviour : StateMachineBehaviour
{
    void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.enabled = false;
    }
}