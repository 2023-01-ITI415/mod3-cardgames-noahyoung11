using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class ScoreBoard : MonoBehaviour
{
    private static ScoreBoard S;

    [Header("Dynamic")]
    [SerializeField]
    private int _score = 0;

    public int score {
        get{return(_score);}
        set{
            _score = value;
            textMP.text = _score.ToString("#,##0");
        }
    }

    private TMP_Text textMP;

    void Awake(){
        if(S != null) Debug.LogError("ScoreBoard.S is already set!");
        S = this;

        textMP = GetComponent<TMP_Text>();
    }
    
    public static int SCORE{
        get{return S.score;}
        set{S.score = value;}
    }

    static public void FS_CALLBACK(FloatingScore fs){
        SCORE += fs.score;
    }
}
