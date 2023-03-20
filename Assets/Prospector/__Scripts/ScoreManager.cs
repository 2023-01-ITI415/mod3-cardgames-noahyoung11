using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eScoreEvent{
    draw,
    mine,
    gameWin,
    gameLoss
}

public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int SCORE_THIS_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Inscribed")]
    [Tooltip("If true, then score events are logged to the Console.")]
    public bool logScoreEvents = true;

    [Header("Dynamic")]
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    [Header("Check this box to reset the ProspectorHighScore to 100")]
    public bool checkToResetHighScore = false;

    void Awake(){
        if(S != null) Debug.LogError("ScoreManager.S is already set!");
        S = this;

        if(PlayerPrefs.HasKey("ProspectorHighScore")){
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        score += SCORE_FROM_PREV_ROUND;
        SCORE_THIS_ROUND = 0;
    }

    static public void TALLY(eScoreEvent evt){
        S.Tally(evt);
    }

    void Tally(eScoreEvent evt){
        switch(evt){
            case eScoreEvent.mine:
                chain++;
                scoreRun += chain;
                break;
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                break;
        }

        string scoreStr = score.ToString("#,##0");
        switch(evt){
            case eScoreEvent.gameWin:
                SCORE_THIS_ROUND = score - SCORE_FROM_PREV_ROUND;
                Log($"You won this round! Round score: {SCORE_THIS_ROUND}");

                SCORE_FROM_PREV_ROUND = score;

                if(HIGH_SCORE <= score){
                    Log($"Game Win. Your new high score is: {scoreStr}");
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                break;
            case eScoreEvent.gameLoss:
                if(HIGH_SCORE <= score){
                    Log($"Game Over. Your new high score is: {scoreStr}");
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }else{
                    Log($"Game Over. Your final score was: {scoreStr}");
                }
                break;
            default:
                Log($"score:{scoreStr}  scoreRun:{scoreRun}  chain:{chain}");
                break;
        }
    }

    void Log(string str){
        if(logScoreEvents) Debug.Log(str);
    }

    void OnDrawGizmos(){
        if(checkToResetHighScore){
            checkToResetHighScore = false;
            PlayerPrefs.SetInt("ProspectorHighScore", 100);
            Debug.LogWarning("PlayerPrefs.ProspectorHighScore reset to 100!");
        }
    }

    static public int CHAIN {get {return S.chain;}}
    static public int SCORE {get {return S.score;}}
    static public int SCORE_RUN {get {return S.scoreRun;}}
}
