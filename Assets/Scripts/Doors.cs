using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
    public HingeJoint leftDoor;
    public HingeJoint rightDoor;

    public List<Collider> surrounding;

    public float Range => 2;

    void Start()
    {
        var lc = leftDoor.GetComponent<Collider>();
        var rc = rightDoor.GetComponent<Collider>();

        Physics.IgnoreCollision(lc, rc);

        foreach (var collider in surrounding)
        {
            Physics.IgnoreCollision(lc, collider);
            Physics.IgnoreCollision(rc, collider);
        }

        lc.attachedRigidbody.constraints = RigidbodyConstraints.None;
        rc.attachedRigidbody.constraints = RigidbodyConstraints.None;
    }
}