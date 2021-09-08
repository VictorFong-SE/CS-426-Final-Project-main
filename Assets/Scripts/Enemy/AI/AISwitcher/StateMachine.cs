
using UnityEngine;

namespace AISwitcher
{
    public class StateMachine<T>
    {
        public State<T> currentState { get; private set; }
        public T owner;

        public StateMachine(T _owner)
        {
            owner = _owner;
            currentState = null;
        }

        public void ChangeState(State<T> newState)
        {
            if (currentState != null && currentState.GetType() == newState.GetType())
            {
                return;
            }

            Debug.Log("Changing state to" + newState.ToString());
            currentState?.ExitState(owner);

            currentState = newState;
            currentState.EnterState(owner);
        }

        public void Update()
        {
            currentState?.UpdateState(owner);

        }
    }


    public abstract class State<T>
    {
        public abstract void EnterState(T owner);
        public abstract void ExitState(T owner);
        public abstract void UpdateState(T owner);
    }
}
