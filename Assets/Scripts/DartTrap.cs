using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartTrap : MonoBehaviour, IOnBeat
{
    private TrapMove[] sequence = new TrapMove[] { TrapMove.ACTIVATE, TrapMove.IDLE, TrapMove.IDLE, TrapMove.ACTIVATE, TrapMove.IDLE, TrapMove.IDLE };

    private int count = 0;

    [SerializeField]
    public LineRenderer the_line;

    // Start is called before the first frame update
    void Start()
    {
        TimeManager.RegisterEntity(this);
    }

    public void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        if (sequence[count] == TrapMove.ACTIVATE)
        {
            Vector3 pos = transform.position;

            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 20))
            {
                if (hit.collider.gameObject.tag == "Player")
                {
                    Player.Instance.Damage(1);
                }

                Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.green, 1);

                Vector3 new_start_pos = new Vector3(0, 0, 0);
                Vector3 new_end_pos = new Vector3(0, 0, hit.distance);

                the_line.SetPositions(new Vector3[] { new_start_pos, new_end_pos });
                GameObject line_obj = GameObject.Instantiate(the_line.gameObject, transform.position, transform.rotation) as GameObject;
                Destroy(line_obj, .15f);
            }
        }

        if (count < sequence.Length - 1)
        {
            count++;
        }
        else
        {
            count = 0;
        }
    }
}
