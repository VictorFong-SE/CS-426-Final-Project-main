using UnityEngine;

public abstract class Enemy : MonoBehaviour, IEnemyAI, ITarget
{
    protected int health;
    public int maxHealth = 6;

    private EnemyMove prevMove;
    private EnemyMove nextMove;
    private bool getNext = true;

    private float initialY;

    public virtual void Start()
    {
        health = maxHealth;
        TimeManager.RegisterEntity(this);
        initialY = transform.position.y;
    }

    public virtual void Update()
    {
        var t = transform;
        var pos = t.position;

        t.position = new Vector3(pos.x, initialY, pos.z);
    }

    /// <summary>
    /// Reduces the Enemy's health, calling Kill once it hits zero.
    /// </summary>
    public bool Damage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Kill();
            return true;
        }

        return false;
    }

    public bool IsTarget(ITarget target)
    {
        return target == this;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    /// <summary>
    /// Destroys the Enemy's GameObject.
    /// </summary>
    public virtual void Kill()
    {
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        if (TimeManager.IsRegistered(this))
        {
            TimeManager.UnregisterEntity(this);
            Destroy(gameObject);
        }
    }

    public virtual void OnBeat(PlayerMove pmove, PlayerFail failure)
    {
        prevMove = nextMove;
        getNext = true;
    }

    /// <summary>
    /// Allows multiple calls to get the next move planned by the enemy.
    /// </summary>
    /// <returns>
    /// Which EnemyMove was chosen
    /// </returns>
    public EnemyMove PeekMove()
    {
        if (getNext)
        {
            nextMove = GetMove();
            getNext = false;
        }

        return nextMove;
    }

    public virtual EnemyMove GetMove()
    {
        if (!getNext)
        {
            throw new System.MethodAccessException("Must not call 'GetMove' twice in 1 beat, as it performs decision making");
        }

        return EnemyMove.IDLE;
    }

    public EnemyMove GetPrevMove()
    {
        return prevMove;
    }

    public int GetHealth()
    {
        return health;
    }
}
