using System.Linq;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource combatMusic;
    public AudioSource ambientSounds;

    void Start()
    {
        combatMusic.pitch = TimeManager.GetPitch(80);
    }

    void Update()
    {
        combatMusic.pitch = TimeManager.GetPitch(80);

        var minions = FindObjectsOfType<Minion>();

        if (minions.Length == 0)
        {
            ambientSounds.volume = 1;
            combatMusic.volume = 0;
        }
        else
        {
            var dist = minions.Min(m => Vector3.Distance(Player.Instance.transform.position, m.transform.position));
            var p = Mathf.Clamp01(Mathf.InverseLerp(2, 8, dist));
            ambientSounds.volume = p;
            combatMusic.volume = 1 - p;
        }
    }
}