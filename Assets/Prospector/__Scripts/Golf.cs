using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Golf : MonoBehaviour
{
    private static Golf S;

    [Header("Inscribed")]
    public float roundDelay = 2f;

    [Header("Dynamic")]
    public List<CardGolf> drawPile;
    public List<CardGolf> discardPile;
    public List<CardGolf> mine;
    public CardGolf target;

    private Transform layoutAnchor;
    private Deck deck;
    private JsonLayout jsonLayout;

    private Dictionary<int, CardGolf> mineIDToCardDict;

    // Start is called before the first frame update
    void Start()
    {
        if(S != null) Debug.LogError("Attempted to set S more than once!");
        S = this;

        jsonLayout = GetComponent<JsonParseLayout>().layout;

        deck = GetComponent<Deck>();

        deck.InitDeck();
        Deck.Shuffle(ref deck.cards);
        drawPile = ConvertCardsToCardGolfs(deck.cards);

        LayoutMine();

        MoveToTarget(Draw());

        UpdateDrawPile();
    }

    List<CardGolf> ConvertCardsToCardGolfs(List<Card> listCard){
        List<CardGolf> listCG = new List<CardGolf>();
        CardGolf cg;
        foreach(Card card in listCard){
            cg = card as CardGolf;
            listCG.Add(cg);
        }
        return(listCG);
    }

    CardGolf Draw(){
        CardGolf cg = drawPile[0];
        drawPile.RemoveAt(0);
        return(cg);
    }

    void LayoutMine(){
        if(layoutAnchor == null){
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
        }

        CardGolf cg;

        mineIDToCardDict = new Dictionary<int, CardGolf>();

        foreach(JsonLayoutSlot slot in jsonLayout.slots){
            cg = Draw();
            cg.faceUp = slot.faceUp;
            cg.transform.SetParent(layoutAnchor);

            int z = int.Parse(slot.layer[slot.layer.Length - 1].ToString());

            cg.SetLocalPos(new Vector3(jsonLayout.multiplier.x * slot.x, jsonLayout.multiplier.y * slot.y, -z));
            cg.layoutID = slot.id;
            cg.layoutSlot = slot;
            
            cg.state = gCardState.mine;

            cg.SetSpriteSortingLayer(slot.layer);

            mine.Add(cg);

            mineIDToCardDict.Add(slot.id, cg);
        }
    }

    void MoveToDiscard(CardGolf cg){
        cg.state = gCardState.discard;
        discardPile.Add(cg);
        cg.transform.SetParent(layoutAnchor);

        cg.SetLocalPos(new Vector3(jsonLayout.multiplier.x * jsonLayout.discardPile.x, jsonLayout.multiplier.y * jsonLayout.discardPile.y, 0));
        cg.faceUp = true;

        cg.SetSpriteSortingLayer(jsonLayout.discardPile.layer);
        cg.SetSortingOrder(-200 + (discardPile.Count * 3));
    }

    void MoveToTarget(CardGolf cg){
        if(target != null) MoveToDiscard(target);

        MoveToDiscard(cg);

        target = cg;
        cg.state = gCardState.target;

        cg.SetSpriteSortingLayer("Target");
        cg.SetSortingOrder(0);
    }

    void UpdateDrawPile(){
        CardGolf cg;

        for(int i = 0; i < drawPile.Count; i++){
            cg = drawPile[i];
            cg.transform.SetParent(layoutAnchor);

            Vector3 cgPos = new Vector3();
            cgPos.x = jsonLayout.multiplier.x * jsonLayout.drawPile.x;
            cgPos.x += jsonLayout.drawPile.xStagger * i;
            cgPos.y = jsonLayout.multiplier.y * jsonLayout.drawPile.y;
            cgPos.z = 0.1f * i;
            cg.SetLocalPos(cgPos);

            cg.faceUp = false;
            cg.state = gCardState.drawpile;
            cg.SetSpriteSortingLayer(jsonLayout.drawPile.layer);
            cg.SetSortingOrder(-10 * i);
        }
    }

    public void SetMinefaceUps(){
        foreach(CardGolf cg in mine){
            bool faceUp = true;
            cg.faceUp = faceUp;
        }
    }

    void CheckForGameOver(){
        if(mine.Count == 0){
            GameOver(true);
            return;
        }
        if(drawPile.Count > 0) return;

        foreach(CardGolf cg in mine){
            if(target.AdjacentTo(cg)) return;
        }

        GameOver(false);
    }

    void GameOver(bool won){
        if(won){
            ScoreManager.TALLY(eScoreEvent.gameWin);
        }else{
            ScoreManager.TALLY(eScoreEvent.gameLoss);
        }

        CardSpritesSO.RESET();

        Invoke("ReloadLevel", roundDelay);

        UITextManager.GAME_OVER_UI(won);
    }

    void ReloadLevel(){
        SceneManager.LoadScene("__Golf_Scene_0");
    }

    static public void CARD_CLICKED(CardGolf cg){
        switch(cg.state){
        case gCardState.target:
            break;
        case gCardState.drawpile:
            S.MoveToTarget(S.Draw());
            S.UpdateDrawPile();
            ScoreManager.TALLY(eScoreEvent.draw);
            break;
        case gCardState.mine:
            bool validMatch = true;

            if(!cg.faceUp) validMatch = false;
            if(!cg.AdjacentTo(S.target)) validMatch = false;
            if(validMatch){
                S.mine.Remove(cg);
                S.MoveToTarget(cg);
                S.SetMinefaceUps();
                ScoreManager.TALLY(eScoreEvent.mine);
            }
            break;
        }
        S.CheckForGameOver();
    }
}
