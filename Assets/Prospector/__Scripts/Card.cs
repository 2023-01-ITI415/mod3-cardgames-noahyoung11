using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Dynamic")]
    public char suit;
    public int rank;
    public Color color = Color.black;
    public string colS = "Black";
    public GameObject back;
    public JsonCard def;

    public List<GameObject> decoGOs = new List<GameObject>();

    public List<GameObject> pipGOs = new List<GameObject>();

    public void Init(char eSuit, int eRank, bool startFaceUp = true){
        gameObject.name = name = eSuit.ToString() + eRank;
        suit = eSuit;
        rank = eRank;

        if(suit == 'D' || suit == 'H'){
            colS = "Red";
            color = Color.red;
        }

        def = JsonParseDeck.GET_CARD_DEF(rank);

        AddDecorators();
        AddPips();
        AddFace();
        AddBack();
        faceUp = startFaceUp;
    }

    public virtual void SetLocalPos(Vector3 v){
        transform.localPosition = v;
    }

    private Sprite _tSprite = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSRend = null;
    private Quaternion _flipRot = Quaternion.Euler(0, 0, 180);

    private void AddDecorators(){
        foreach(JsonPip pip in JsonParseDeck.DECORATORS){
            if(pip.type == "suit"){
                _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
                _tSRend = _tGO.GetComponent<SpriteRenderer>();
                _tSRend.sprite = CardSpritesSO.SUITS[suit];
            }else{
                _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
                _tSRend = _tGO.GetComponent<SpriteRenderer>();
                _tSRend.sprite = CardSpritesSO.RANKS[rank];
                _tSRend.color = color;
            }

            _tSRend.sortingOrder = 1;
            _tGO.transform.localPosition = pip.loc;
            if(pip.flip) _tGO.transform.rotation = _flipRot;
            if(pip.scale != 1){
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            _tGO.name = pip.type;
            decoGOs.Add(_tGO);
        }
    }

    private void AddPips(){
        int pipNum = 0;

        foreach(JsonPip pip in def.pips){
            _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
            _tGO.transform.localPosition = pip.loc;
            if(pip.flip) _tGO.transform.rotation = _flipRot;
            if(pip.scale != 1){
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }

            _tGO.name = "pip_" + pipNum++;
            _tSRend = _tGO.GetComponent<SpriteRenderer>();
            _tSRend.sprite = CardSpritesSO.SUITS[suit];
            _tSRend.sortingOrder = 1;
            pipGOs.Add(_tGO);
        }
    }

    private void AddFace(){
        if(def.face == "") return;
        string faceName = def.face + suit;
        _tSprite = CardSpritesSO.GET_FACE(faceName);
        if(_tSprite == null){
            Debug.LogError("Face sprite " + faceName + " not found.");
            return;
        }

        _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
        _tSRend = _tGO.GetComponent<SpriteRenderer>();
        _tSRend.sprite = _tSprite;
        _tSRend.sortingOrder = 1;
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = faceName;
    }

    public bool faceUp {
        get{return(!back.activeSelf);}
        set{back.SetActive(!value);}
    }

    private void AddBack(){
        _tGO = Instantiate<GameObject>(Deck.SPRITE_PREFAB, transform);
        _tSRend = _tGO.GetComponent<SpriteRenderer>();
        _tSRend.sprite = CardSpritesSO.BACK;
        _tGO.transform.localPosition = Vector3.zero;
        _tSRend.sortingOrder = 2;
        _tGO.name = "back";
        back = _tGO;
    }

    private SpriteRenderer[] spriteRenderers;

    void PopulateSpriteRenderers(){
        if(spriteRenderers != null) return;
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void SetSpriteSortingLayer(string layerName){
        PopulateSpriteRenderers();

        foreach(SpriteRenderer srend in spriteRenderers){
            srend.sortingLayerName = layerName;
        }
    }

    public void SetSortingOrder(int sOrd){
        PopulateSpriteRenderers();

        foreach(SpriteRenderer srend in spriteRenderers){
            if(srend.gameObject == this.gameObject){
                srend.sortingOrder = sOrd;
            }else if(srend.gameObject.name == "back"){
                srend.sortingOrder = sOrd + 2;
            }else{
                srend.sortingOrder = sOrd + 1;
            }
        }
    }

    virtual public void OnMouseUpAsButton(){
        print(name);
    }

    public bool AdjacentTo(Card otherCard, bool wrap = true){
        if(!faceUp || !otherCard.faceUp) return (false);
        if(Mathf.Abs(rank - otherCard.rank) == 1) return (true);
        if(wrap){
            if(rank == 1 && otherCard.rank == 13) return (true);
            if(rank == 13 && otherCard.rank == 1) return (true);

        }
        return (false);
    }
}
