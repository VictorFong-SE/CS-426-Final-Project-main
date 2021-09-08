using UnityEngine;
using UnityEngine.UI;

public class UI_ComboManager : MonoBehaviour
{
    public static UI_ComboManager Instance {get; private set;}
    public Text textField;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        textField.text = "";
    }

    // Update is called once per frame
    public void OnBeat()
    {
        Boss the_boss = FindObjectOfType<Boss>();

        textField.text = "";

        if(the_boss != null){
            if(the_boss.IsStunned()){
                textField.text += "STUN\n";
            }
            else{
                foreach (var move in the_boss.GetMoves())
                {

                    textField.text += $"{move}\n";
                }
            }      
        }
        
    }
}
