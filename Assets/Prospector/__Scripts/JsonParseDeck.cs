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
    [Header("Inscribed")]
    public TextAsset jsonDeckFile;

    [Header("Dynamic")]
    public JsonDeck deck;

    void Awake(){
        deck = JsonUtility.FromJson<JsonDeck>(jsonDeckFile.text);
    }
}

