using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum gCardState{drawpile, mine, target, discard}

public class CardGolf : Card
{
    [Header("Dynamic: CardGolf")]
    public gCardState state = gCardState.drawpile;
    public List<CardGolf> hiddenBy = new List<CardGolf>();
    public int layoutID;
    public JsonLayoutSlot layoutSlot;

    override public void OnMouseUpAsButton(){
        Golf.CARD_CLICKED(this);
    }
}
