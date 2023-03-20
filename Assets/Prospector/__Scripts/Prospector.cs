using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Deck))]
[RequireComponent(typeof(JsonParseLayout))]
public class Prospector : MonoBehaviour
{
    private static Prospector S;

    [Header("Dynamic")]
    public List<CardProspector> drawPile;
    public List<CardProspector> discardPile;
    public List<CardProspector> mine;
    public CardProspector target;

    private Transform layoutAnchor;
    private Deck deck;
    private JsonLayout jsonLayout;

    private Dictionary<int, CardProspector> mineIDToCardDict;

    // Start is called before the first frame update
    void Start()
    {
        if(S != null) Debug.LogError("Attempted to set S more than once!");
        S = this;

        jsonLayout = GetComponent<JsonParseLayout>().layout;

        deck = GetComponent<Deck>();

        deck.InitDeck();
        Deck.Shuffle(ref deck.cards);
        drawPile = ConvertCardsToCardProspectors(deck.cards);

        LayoutMine();

        MoveToTarget(Draw());

        UpdateDrawPile();
    }

    List<CardProspector> ConvertCardsToCardProspectors(List<Card> listCard){
        List<CardProspector> listCP = new List<CardProspector>();
        CardProspector cp;
        foreach(Card card in listCard){
            cp = card as CardProspector;
            listCP.Add(cp);
        }
        return(listCP);
    }

    CardProspector Draw(){
        CardProspector cp = drawPile[0];
        drawPile.RemoveAt(0);
        return(cp);
    }

    void LayoutMine(){
        if(layoutAnchor == null){
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
        }

        CardProspector cp;

        mineIDToCardDict = new Dictionary<int, CardProspector>();

        foreach(JsonLayoutSlot slot in jsonLayout.slots){
            cp = Draw();
            cp.faceUp = slot.faceUp;
            cp.transform.SetParent(layoutAnchor);

            int z = int.Parse(slot.layer[slot.layer.Length - 1].ToString());

            cp.SetLocalPos(new Vector3(jsonLayout.multiplier.x * slot.x, jsonLayout.multiplier.y * slot.y, -z));
            cp.layoutID = slot.id;
            cp.layoutSlot = slot;
            
            cp.state = eCardState.mine;

            cp.SetSpriteSortingLayer(slot.layer);

            mine.Add(cp);

            mineIDToCardDict.Add(slot.id, cp);
        }
    }

    void MoveToDiscard(CardProspector cp){
        cp.state = eCardState.discard;
        discardPile.Add(cp);
        cp.transform.SetParent(layoutAnchor);

        cp.SetLocalPos(new Vector3(jsonLayout.multiplier.x * jsonLayout.discardPile.x, jsonLayout.multiplier.y * jsonLayout.discardPile.y, 0));
        cp.faceUp = true;

        cp.SetSpriteSortingLayer(jsonLayout.discardPile.layer);
        cp.SetSortingOrder(-200 + (discardPile.Count * 3));
    }

    void MoveToTarget(CardProspector cp){
        if(target != null) MoveToDiscard(target);

        MoveToDiscard(cp);

        target = cp;
        cp.state = eCardState.target;

        cp.SetSpriteSortingLayer("Target");
        cp.SetSortingOrder(0);
    }

    void UpdateDrawPile(){
        CardProspector cp;

        for(int i = 0; i < drawPile.Count; i++){
            cp = drawPile[i];
            cp.transform.SetParent(layoutAnchor);

            Vector3 cpPos = new Vector3();
            cpPos.x = jsonLayout.multiplier.x * jsonLayout.drawPile.x;
            cpPos.x += jsonLayout.drawPile.xStagger * i;
            cpPos.y = jsonLayout.multiplier.y * jsonLayout.drawPile.y;
            cpPos.z = 0.1f * i;
            cp.SetLocalPos(cpPos);

            cp.faceUp = false;
            cp.state = eCardState.drawpile;
            cp.SetSpriteSortingLayer(jsonLayout.drawPile.layer);
            cp.SetSortingOrder(-10 * i);
        }
    }

    public void SetMineFaceUps(){
        CardProspector coverCP;
        foreach(CardProspector cp in mine){
            bool faceUp = true;

            foreach(int coverID in cp.layoutSlot.hiddenBy){
                coverCP = mineIDToCardDict[coverID];
                if(coverCP == null || coverCP.state == eCardState.mine){
                    faceUp = false;
                }
            }
            cp.faceUp = faceUp;
        }
    }

    void CheckForGameOver(){
        if(mine.Count == 0){
            GameOver(true);
            return;
        }
        if(drawPile.Count > 0) return;

        foreach(CardProspector cp in mine){
            if(target.AdjacentTo(cp)) return;
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

        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    static public void CARD_CLICKED(CardProspector cp){
        switch(cp.state){
        case eCardState.target:
            break;
        case eCardState.drawpile:
            S.MoveToTarget(S.Draw());
            S.UpdateDrawPile();
            ScoreManager.TALLY(eScoreEvent.draw);
            break;
        case eCardState.mine:
            bool validMatch = true;

            if(!cp.faceUp) validMatch = false;
            if(!cp.AdjacentTo(S.target)) validMatch = false;
            if(validMatch){
                S.mine.Remove(cp);
                S.MoveToTarget(cp);
                S.SetMineFaceUps();
                ScoreManager.TALLY(eScoreEvent.mine);
            }
            break;
        }
        S.CheckForGameOver();
    }

}
