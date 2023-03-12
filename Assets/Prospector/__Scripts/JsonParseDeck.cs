using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JsonPip{
    public string type = "pip";
    public Vector3 loc;
    public bool flip = false;
    public float scale = 1;
}

[System.Serializable]
public class JsonCard{
    public int rrank;
    public string face;
    public List<JsonPip> pips = new List<JsonPip>();
}

[System.Serializable]
    public class JsonDeck{
    public List<JsonPip> decorators = new List<JsonPip>();
    public List<JsonCard> cards = new List<JsonCard>();
}

public class JsonParseDeck : MonoBehaviour{
    private static JsonParseDeck S {get; set;}

    [Header("Inscribed")]
    public TextAsset jsonDeckFile;

    [Header("Dynamic")]
    public JsonDeck deck;

    void Awake(){
        if(S != null){
            Debug.LogError("JsonParseDeck.S can't be set a 2nd time!");
            return;
        }
        S = this;

        deck = JsonUtility.FromJson<JsonDeck>(jsonDeckFile.text);
    }

    static public List<JsonPip> DECORATORS {
        get{return S.deck.decorators;}
    }

    static public JsonCard GET_CARD_DEF(int rank){
        if((rank < 1) || (rank > S.deck.cards.Count)){
            Debug.LogWarning("Illegal rank argument: " + rank);
            return null;
        }
        return S.deck.cards[rank - 1];
    }
}





