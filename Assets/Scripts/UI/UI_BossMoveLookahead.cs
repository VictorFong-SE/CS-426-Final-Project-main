using UnityEngine;
using UnityEngine.UI;

public class UI_BossMoveLookahead : MonoBehaviour
{
    public static UI_BossMoveLookahead Instance {get; private set;}
    public Text textField;

    [SerializeField]
    Sprite attack_img;

    [SerializeField]
    Sprite block_img;

    [SerializeField]
    Sprite idle_img;

    [SerializeField]
    Sprite stun_img;

    [SerializeField]
    Image current;

    [SerializeField]
    Image next;

    [SerializeField]
    Image next_next;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    public void OnBeat()
    {
        Boss the_boss = FindObjectOfType<Boss>();

        if(the_boss != null){
            if(the_boss.IsStunned()){
                current.sprite = stun_img;
                next.sprite = stun_img;
                next_next.sprite = stun_img;
            }
            else{
                EnemyMove[] the_moves = the_boss.GetMoves();

                current.sprite = DetermineImage(the_moves[0]);
                next.sprite = DetermineImage(the_moves[1]);
                next_next.sprite = DetermineImage(the_moves[2]);
            }      
        }
        
    }

    private Sprite DetermineImage(EnemyMove the_move){
        switch(the_move){
            case EnemyMove.ATTACK:
                return attack_img;
            case EnemyMove.BLOCK:
                return block_img;
            case EnemyMove.IDLE:
                return idle_img;
            default:
                return stun_img;
        }
    }
}
