using UnityEngine;

public interface IInteractive
{
    float Range { get; }

    void OnInteract(GameObject interactor);
}