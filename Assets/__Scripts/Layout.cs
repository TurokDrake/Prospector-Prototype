using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Layout : MonoBehaviour
{
    public PT_XMLHashtable xml;
    public PT_XMLReader xmlr;
    public Vector2 multiplier; // 场景中牌的距离
    public List<SlotDef> slotDefs;
    public SlotDef drawPile;
    public SlotDef discardPile;
    // 所有图层的名称
    public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    public bool isReadLayoutFinished = false;

    #region 已删除的代码，请折叠
    // 于2026-10-17注释
    //public void ReadLayout()
    //{
    //    StartCoroutine(ReadLayoutXML());
    //}
    #endregion


    public void ReadLayout(TextAsset tA)
    {
        #region 使用C#的API读取Deck_Layout
        // 加载文件
        //TextAsset layoutAsset = Resources.Load<TextAsset>("XML Versions of Deck & Layout/XML_Layout");
        TextAsset layoutAsset = tA;

        // 检查是否获取到文件
        if (layoutAsset == null)
        {
            Debug.Log("layoutAsset获取失败！");
            return;
        }
        else
        {
            Debug.Log("layoutAsset获取成功！");
        }

        // 创建并加载XML文件
        XmlDocument layoutXml = new XmlDocument();
        layoutXml.LoadXml(layoutAsset.text);

        // 获取<multiplier>节点并读取其属性
        XmlNode multiplierNode = layoutXml.SelectSingleNode("//multiplier");
        multiplier.x = float.Parse(multiplierNode.Attributes["x"]?.Value);
        multiplier.y = float.Parse(multiplierNode.Attributes["y"]?.Value);

        // 获取<slot>节点并读取其属性
        XmlNodeList slotsX = layoutXml.SelectNodes("//slot");
        SlotDef tSD;
        if (slotsX.Count > 0)
        {
            for (int i = 0; i < slotsX.Count; i++)
            {
                tSD = new SlotDef();
                if (slotsX[i].Attributes["type"] != null)
                {
                    tSD.type = slotsX[i].Attributes["type"]?.Value;
                }
                else
                {
                    tSD.type = "slot";
                }

                tSD.x = float.Parse(slotsX[i].Attributes["x"]?.Value);
                tSD.y = float.Parse(slotsX[i].Attributes["y"]?.Value);
                tSD.layerID = int.Parse(slotsX[i].Attributes["layer"]?.Value);
                tSD.layerName = sortingLayerNames[tSD.layerID];

                switch (tSD.type)
                {
                    case "slot":
                        tSD.faceUp = (slotsX[i].Attributes["faceup"]?.Value=="1");
                        tSD.id = int.Parse(slotsX[i].Attributes["id"]?.Value);
                        if (slotsX[i].Attributes["hiddenby"]!=null)
                        {
                            string[] hiding = slotsX[i].Attributes["hiddenby"]?.Value.Split(',');
                            foreach (string s in hiding)
                            {
                                tSD.hiddenBy.Add(int.Parse(s));
                            }
                        }
                        slotDefs.Add(tSD);
                        break;
                    case "drawpile":
                        tSD.stagger.x = float.Parse(slotsX[i].Attributes["xstagger"]?.Value);
                        drawPile = tSD;
                        break;
                    case "discardpile":
                        discardPile = tSD;
                        break;
                }
            }
        }

        #endregion

        #region PT_XMLReader读取Deck_Layout
        //xmlr = new PT_XMLReader();
        //xmlr.Parse(xmlText);
        //xml = xmlr.xml["xml"][0];

        //// 读取设置牌间距的属性
        //multiplier.x = float.Parse(xmlr.xml["xml"][0]["multipiler"][0].att("x"));
        //multiplier.y = float.Parse(xmlr.xml["xml"][0]["multipiler"][0].att("y"));

        //SlotDef tSD; // tempSlotDef，临时的牌的位置

        //PT_XMLHashList slotsX = xml["slot"];

        //for (int i = 0; i < slotsX.Count; i++)
        //{
        //    tSD = new SlotDef();
        //    if (slotsX[i].HasAtt("type"))
        //    {
        //        tSD.type = slotsX[i].att("type");
        //    }
        //    else
        //    {
        //        tSD.type = "slot";
        //    }

        //    tSD.x = float.Parse(slotsX[i].att("x"));
        //    tSD.y = float.Parse(slotsX[i].att("y"));
        //    tSD.layerID = int.Parse(slotsX[i].att("layer"));
        //    tSD.layerName = sortingLayerNames[tSD.layerID];

        //    switch (tSD.type)
        //    {
        //        case "slot":
        //            tSD.faceUp = (slotsX[i].att("faceUp") == "1");
        //            tSD.id = int.Parse(slotsX[i].att("id"));
        //            if (slotsX[i].HasAtt("hiddenby"))
        //            {
        //                string[] hiding = slotsX[i].att("hiddenby").Split(',');
        //                foreach (string s in hiding)
        //                {
        //                    tSD.hiddenBy.Add(int.Parse(s));
        //                }
        //            }
        //            slotDefs.Add(tSD);
        //            break;
        //        case "drawpile":
        //            tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
        //            drawPile = tSD;
        //            break;
        //        case "discardpile":
        //            discardPile = tSD;
        //            break;
        //    }
        //}
        #endregion

    }

    #region 已删除的代码，应折叠
    // 动态加载XML_Layout文件
    // 于2026-10-27注释
    //public IEnumerator ReadLayoutXML()
    //{
    //    ResourceRequest rq = Resources.LoadAsync<TextAsset>("XML Versions of Deck & Layout/XML_Layout");
    //    yield return rq;

    //    TextAsset layoutAsset = rq.asset as TextAsset;

    //    ReadLayout(layoutAsset);

    //    isReadLayoutFinished = true;

    //    // 从Prospector中替换到此处
    //   // Prospector.S.LayoutGame();
    //}
    #endregion

}

[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}