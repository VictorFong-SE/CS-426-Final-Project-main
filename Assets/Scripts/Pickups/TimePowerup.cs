using UnityEngine;

public class TimePowerup : MonoBehaviour
{
    public uint slowdown;
    public int length;
    public GameObject hudElement;

    private Vector3 initialPos;
    private Transform cameraTransform;

    void Start()
    {
        initialPos = transform.position;
        cameraTransform = GameObject.Find("Main Camera").transform;
    }

    void Update()
    {
        transform.position = new Vector3(initialPos.x, initialPos.y + .1f * Mathf.Sin(Time.deltaTime), initialPos.z);

        var camPos = cameraTransform.position;
        transform.LookAt(new Vector3(camPos.x, transform.position.y, camPos.z), Vector3.up);

        var angles = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(90, angles.y, 0);
    }

    public void OnCollisionEnter(Collision collision)
    {
        var player = collision.transform.GetComponentInParent<Player>();
        if (player != null)
        {
            TimeManager.Instance.AddPowerup("Slowdown", -slowdown, length);
            hudElement.SetActive(true);
            Destroy(gameObject);
        }
    }
}
