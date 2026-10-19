using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardState
{
    drawpile,
    tableau, // 矿井中初始的28张牌之一
    target, // 弃牌堆顶部的有效牌
    discard
}

public class CardProspector : Card
{
    public CardState state = CardState.drawpile;
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    public int layoutID;
    public SlotDef slotDef;

    public override void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
