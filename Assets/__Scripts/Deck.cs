using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

// 用于解析xml的类，绑定至摄像机上
public class Deck : MonoBehaviour
{
    /* ------ 四个花色的Sprite ------ */
    public Sprite suitClub;
    public Sprite suitDiamand;
    public Sprite suitSpade;
    public Sprite suitHeart;

    /* ------ 脸牌和点数牌的Sprite ------ */
    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    /* ------ 纸牌背面、正面 以及 黄金色的纸牌背面、正面 ------ */
    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    /* ------ 卡牌、图案游戏对象 ------ */
    public GameObject prefabSprite;
    public GameObject prefabCard;

    public bool _____; // 分隔符，以上是在Inspector窗口设置的内容；以下是通过代码动态调整的内容

    public PT_XMLReader xmlr; // 读取XML文件的工具
    public List<string> cardNames; // 纸牌的名称，格式： “花色” + “点数”，例如“C1”表示 花色：Club  点数：1
    public List<Card> cards; // 52张卡牌（正牌），不包含大小王
    public List<Decorator> decorators; // 每张卡牌都会出现的 角码和角码下的花色图案的位置、是否翻转和缩放
    public List<CardDefinition> cardDefs; // 卡牌的配置信息，这些信息从XML文件读取
    public Transform deckAnchor; // 统一卡牌的父对象，方便设置和管理卡牌的相对位置
    public Dictionary<string, Sprite> dictSuits; // 

    #region 调试过程中使用的查看方法是否调用完成的变量
    public bool isConvertFinished = false; // 标记方法是否完成
    public bool isReadDeckFinished = false;
    public bool isMakeCardsFinished = false;
    public bool isShuffleFinished = false;
    #endregion


    #region 已删除的代码，应折叠起来
    //public void InitDeck(int ndx=0)
    //{
    //    StartCoroutine(InitDeck());
    //}
    //IEnumerator InitDeck()
    //{
    //    InitDeck("InitDeck");
    //    yield return null;
    //}
    #endregion


    /// <summary>
    /// 初始化卡牌
    /// </summary>
    /// <param name="deckXML">传入一个TextAsset文件</param>
    public void InitDeck(TextAsset deckXML)
    {
        /* ---------- 为卡牌的游戏对象创建一个锚点 ---------- */
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        /* ---------- 初始化字典，一个花色名称对应一个Sprite图像 ---------- */
        dictSuits = new Dictionary<string, Sprite>()
        {
            { "C",suitClub },
            {"D",suitDiamand },
            {"H",suitHeart },
            {           "S",suitSpade }
        };

        /* ---------- 先读取卡片信息，然后再制作卡片 ---------- */
        // 于2026-10-17更改为一开始的ReaadDeck()，同时修改了函数的参数
        //StartCoroutine(ReadDeck());
        ReadDeck(deckXML);
    }

    #region 已删除的代码，应折叠起来
    // 于2026-10-17注释掉
    //IEnumerator ReadDeck( )
    //{
    //    ResourceRequest rq = Resources.LoadAsync<TextAsset>("XML Versions of Deck & Layout/XML_Deck");
    //    yield return rq;

    //    TextAsset xmlAsset = rq.asset as TextAsset;
    //    ReadDeck(xmlAsset);
    //    isReadDeckFinished = true;

    //    //MakeCards();

    //    // 洗牌，新开协程；2026-10-15 20:49改为在Layout.cs的LayoutGame()前洗牌
    //    //Shuffle(ref cards); // 直接使用会卡死
    //    //StartCoroutine(ShuffleThread());
    //    // 洗牌
    //   // Shuffle(ref Prospector.S.deck.cards);

    //    // 在这里将card 转换为 CardProspector
    //    //StartCoroutine(ConvertListCardsToListCardProspectors());
    //    //ConvertListCardsToListCardProspectors();
    //}
    #endregion


    public void ConvertListCardsToListCardProspectors()
    {
        Prospector.S.drawPile = Prospector.S.ConvertListCardsToListCardProspectors(cards);
        isConvertFinished = true;
    }

    /// <summary>
    /// 读取XML文件并存储卡牌配置信息
    /// </summary>
    /// <param name="deckXMLText"></param>
    private void ReadDeck(TextAsset tA)
    {
        #region 若使用PT_XMLReader读取XML文件
        // 先实例化一个PT_XMLReader，并把作为参数传入的XML文件中的字符串处理一下
        //xmlr = new PT_XMLReader();
        //xmlr.Parse(deckXMLText);

        #region （已注释）演示xmlr是如何读取xml文件的测试用例
        // 测试用例，演示xmlr怎么用
        //string s = "xml[0] decorator[0] ";
        //s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        //s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        //s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        //s += " scale" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        //print(s);
        #endregion

        #region （已注释）从XML文件中获取Sprite的位置信息
        //decorators = new List<Decorator>();
        //// xmlr.xml是一个PT_XMLHashtable类型的成员，xml["xml"]是一个PT_XMLHashList类型的成员，xml["xml"][0]是一个PT_XMLHashtable类型的成员，...[0]["decorator"]是一个PT_XMLHashList的成员
        //// 从文件中获取所有<decorator>标签，作为一个PT_XMLHashList列表
        //PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        //// Decorator是一个存储不同点数的Sprite的位置的对象，存的是位置信息与是否翻转
        //Decorator deco;
        //for (int i = 0; i < xDecos.Count; i++)
        //{
        //    // 创建一个新的Decorator对象，等会要把type、loc、flip和scale赋予它
        //    deco = new Decorator();

        //    deco.type = xDecos[i].att("type");

        //    // 如果flip属性是1就返回1，如果不是1就返回0
        //    deco.flip = (xDecos[i].att("flip") == "1");

        //    // 找到的是字符串，要转换成float类型
        //    deco.scale = float.Parse(xDecos[i].att("scale"));

        //    // 赋予位置信息
        //    deco.loc.x = float.Parse(xDecos[i].att("x"));
        //    deco.loc.y = float.Parse(xDecos[i].att("y"));
        //    deco.loc.z = float.Parse(xDecos[i].att("z"));

        //    // 记得记录起来，否则下一次就被丢掉了
        //    decorators.Add(deco);
        //}
        #endregion

        #region （已注释）从XML文件中获取角码和花色符号的位置信息
        //// 初始化这个列表，接下来将从XML文件获取角码（即1-K）和花色符号（黑桃、红桃、方块和梅花）的位置信息
        //cardDefs = new List<CardDefinition>();

        //// 获取所有的<card>标签
        //PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
        //for (int i = 0; i < xCardDefs.Count; i++)
        //{
        //    CardDefinition cDef = new CardDefinition();
        //    // 获取点数
        //    cDef.rank = int.Parse(xCardDefs[i].att("rank"));

        //    // 获取<card>中的<pip>标签
        //    PT_XMLHashList xPips = xCardDefs[i]["pip"];

        //    for (int j = 0; j < xPips.Count; j++)
        //    {
        //        deco = new Decorator();

        //        deco.type = "pip";
        //        deco.flip = (xPips[j].att("flip") == "1");
        //        deco.loc.x = float.Parse(xPips[j].att("x"));
        //        deco.loc.y = float.Parse(xPips[j].att("y"));
        //        deco.loc.z = float.Parse(xPips[j].att("z"));
        //        if (xPips[j].HasAtt("scale"))
        //        {
        //            deco.scale = float.Parse(xPips[j].att("scale"));
        //        }
        //        cDef.pips.Add(deco);
        //    }

        //    if (xCardDefs[i].HasAtt("face"))
        //    {
        //        cDef.face = xCardDefs[i].att("face");
        //    }
        //    cardDefs.Add(cDef);


        //}
        #endregion
        #endregion

        #region 使用C#的API读取XML文件——C#
        /* ---读取XML文件--- */

        // Resources文件夹比较特殊，不能使用相对路径访问
        // 注意，无需为文件添加".xml"后缀
        //TextAsset xmlAsset = Resources.Load<TextAsset>("XML Versions of Deck & Layout/XML_Deck");
        TextAsset xmlAsset = tA;

        // 检查是否获取到文件
        if (xmlAsset == null)
        {
            Debug.Log("XML_Deck文件获取失败！");
            return;
        }
        else
        {
            Debug.Log("XML_Deck文件获取成功！");
        }

        // 创建XML文档并加载已有的XML文档
        // 该步操作BELIKE： 打开文本编辑器，默认创建了一个暂未保存的空文档，然后用户点击文件-打开-某个文档
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlAsset.text);

        #region 读取XML文件中的 decorator 节点的信息
        /* ---读取XML文件中的 decorator节点--- */
        // 在 decorator前添加 // 表示是在文件全局中寻找
        XmlNodeList nodesDeco = doc.SelectNodes("//decorator");
        Debug.Log($"nodesDeco.Count:{nodesDeco.Count}"); // 检查找到的节点数量是否出错

        decorators = new List<Decorator>();
        Decorator deco;
        if (nodesDeco.Count > 0)
        {
            // 读取并设置decorator
            foreach (XmlNode n in nodesDeco)
            {
                deco = new Decorator();
                // 添加?作为可空类型，可以避免XML中没写这个属性，但是却使用了其Value，导致空引用报错
                deco.type = n.Attributes["type"]?.Value;
                deco.loc.x = float.Parse(n.Attributes["x"]?.Value);
                deco.loc.y = float.Parse(n.Attributes["y"]?.Value);
                deco.loc.z = float.Parse(n.Attributes["z"]?.Value);
                deco.flip = n.Attributes["flip"]?.Value == "1";
                deco.scale = float.Parse(n.Attributes["scale"]?.Value);

                decorators.Add(deco);
            }
        }
        #endregion

        #region 读取XML文件中的 card 节点的信息
        /* ---读取XML文件中的 card 节点--- */
        XmlNodeList nodesCard = doc.SelectNodes("//card");
        Debug.Log($"nodesDeco.Count:{nodesCard.Count}"); // 检查找到的节点数量是否出错

        cardDefs = new List<CardDefinition>();
        CardDefinition cDef;
        if (nodesCard.Count > 0)
        {
            foreach (XmlNode n in nodesCard)
            {
                cDef = new CardDefinition();

                cDef.rank = int.Parse(n.Attributes["rank"]?.Value);

                XmlNodeList childNodesCard = n.ChildNodes;

                foreach (XmlNode childN in childNodesCard)
                {
                    deco = new Decorator();

                    deco.type = "pip";

                    deco.flip = childN.Attributes["flip"]?.Value == "1";
                    deco.loc.x = float.Parse(childN.Attributes["x"]?.Value);
                    deco.loc.y = float.Parse(childN.Attributes["y"]?.Value);
                    deco.loc.z = float.Parse(childN.Attributes["z"]?.Value);
                    deco.scale = float.Parse(childN.Attributes["scale"]?.Value);

                    cDef.pips.Add(deco);
                }

                // J、Q、K卡面是人脸
                cDef.face = n.Attributes["face"]?.Value;
                cardDefs.Add(cDef);

            }
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// 传入纸牌点数，获取该点数纸牌的定义
    /// </summary>
    /// <param name="rnk">点数</param>
    /// <returns></returns>
    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        // 遍历从XML文件中读取的卡牌配置信息，如果有符合条件的卡牌就返回其配置信息；没有则返回空
        foreach (CardDefinition cd in cardDefs)
        {
            if (cd.rank == rnk)
            {
                return cd;
            }
        }
        return null;
    }

    /// <summary>
    /// 制作卡牌
    /// </summary>
    public void MakeCards()
    {
        /* <------------ 设置卡牌名称，共 4 X 13 = 52 张 ------------> */
        cardNames = new List<string>();
        // C表示Club、D表示Diamand、H表示Heart，S表示Spade
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            // 卡牌的名称格式：花色+数字
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }

        /* <------------ 添加卡牌 ------------> */
        cards = new List<Card>();
        // tS、 tGO、 tSR 缩写分别为 tempSprite、 tempGameObject、 tempSpriteRenderer，作为临时变量使用
        Sprite tS = null;
        GameObject tGO = null;
        SpriteRenderer tSR = null;

        // 开始创建 52 张卡牌
        for (int i = 0; i < cardNames.Count; i++)
        {
            // 创建一张卡牌，并设置其父对象，便于管理卡牌间的相对位置；获取卡牌的 Card 组件引用
            // Card 组件中存放着关于卡牌的配置信息
            GameObject cgo = Instantiate<GameObject>(prefabCard);
            cgo.transform.parent = deckAnchor;
            Card card = cgo.GetComponent<Card>();

            // 调整每一张卡牌的位置
            cgo.transform.localPosition = new Vector3((i % 13) * 3, i / 13 * 4, 0);

            // 设置卡牌的基本属性值
            card.name = cardNames[i]; // 卡牌游戏对象的名称
            card.suit = card.name[0].ToString(); // 卡牌花色的首字母，例如C1表示Club 1（花色为梅花，点数为1），其首字母即C
            card.rank = int.Parse(card.name.Substring(1)); // 卡牌的点数，由于我们命名很规范，直接从名字里截取即可获得

            // 若花色为方块或者红桃，我们需要修改花色的颜色为红色；花色的颜色默认为黑色
            if (card.suit == "D" || card.suit == "H")
            {
                card.colS = "Red";
                card.color = Color.red;
            }

            // 获取指定点数的卡牌的配置信息
            card.def = GetCardDefinitionByRank(card.rank);

            /* <------------ 获取每一张卡牌都有的内容——卡牌的角码和角码下的花色的信息 ------------> */
            foreach (Decorator deco in decorators)
            {
                // 如果是四种花色的一种
                if (deco.type == "suit")
                {
                    // 创建一个Sprite，获取其SpriteRenderer，根据卡片的花色修改图片
                    tGO = Instantiate(prefabSprite);
                    tSR = tGO.GetComponent<SpriteRenderer>();
                    tSR.sprite = dictSuits[card.suit];
                }
                // 如果不是花色，就是点数
                else
                {
                    // 创建Sprite并获取其SpriteRenderer
                    tGO = Instantiate(prefabSprite);
                    tSR = tGO.GetComponent<SpriteRenderer>();
                    // 获取卡牌的点数的Sprite图像
                    tS = rankSprites[card.rank];
                    tSR.sprite = tS;
                    tSR.color = card.color;
                }
                // 设置图层排序顺序，让角码的Sprite显示在卡牌上层
                tSR.sortingOrder = 1;
                // 设置角码的父对象为卡牌
                tGO.transform.parent = cgo.transform;
                // 设置角码在卡牌上的位置
                tGO.transform.localPosition = deco.loc;
                // 是否翻转
                if (deco.flip)
                {
                    tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                // 是否缩放
                if (deco.scale != 1)
                {
                    tGO.transform.localScale = Vector3.one * deco.scale;
                }
                // 修改名字
                tGO.name = deco.type;
                // 将对象存储起来
                card.decoGOs.Add(tGO);
            }

            /* <------------ 绘制特定点数的卡牌上的花色图案 ------------> */
            foreach (Decorator pip in card.def.pips)
            {
                // 创建一个Sprite游戏对象并设置其位置
                tGO = Instantiate(prefabSprite);
                tGO.transform.parent = cgo.transform;
                tGO.transform.localPosition = pip.loc;

                // 是否翻转
                if (pip.flip)
                {
                    tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                // 是否缩放
                if (pip.scale != 1)
                {
                    // 在XML文件中，有的<pip>标签的scale属性没有定义，因此读到的是0，这样一来，就算那些默认为1的花色Sprite的scale也变成了0
                    // 我的做法是修改XML文件，为其显示赋予scale属性
                    // 添加scale属性时，我贪快，后面一部分属性间没有用空格隔开，然后就报错了，在属性前补上空格即可
                    tGO.transform.localScale = Vector3.one * pip.scale;
                }

                // 设置该对象的各项属性
                tGO.name = "pip";
                tSR = tGO.GetComponent<SpriteRenderer>();
                tSR.sprite = dictSuits[card.suit];
                tSR.sortingOrder = 1;
                card.pipGOs.Add(tGO);
            }

            /* <------------ 当卡牌是花牌时，即JQK一张 ------------> */
            if (card.def.face != "")
            {
                tGO = Instantiate(prefabSprite);
                tSR = tGO.GetComponent<SpriteRenderer>();
                // 名称为脸+点数
                tS = GetFace(card.def.face + card.suit);
                tSR.sprite = tS;
                tSR.sortingOrder = 1;
                tGO.transform.parent = card.transform;
                tGO.transform.localPosition = Vector3.zero;
                tGO.name = "face";
            }

            /* <------------ 添加纸牌背景 ------------> */
            tGO = Instantiate(prefabSprite);
            tSR = tGO.GetComponent<SpriteRenderer>();
            tSR.sprite = cardBack;
            tGO.transform.parent = card.transform;
            tGO.transform.localPosition = Vector3.zero;

            // 为了覆盖角码、花色等Sprite，需要设置其渲染顺序
            tSR.sortingOrder = 2;
            tGO.name = "Back";
            card.back = tGO;

            // 卡牌默认背面朝上
            card.FaceUp = false;

            cards.Add(card);
        }
        isMakeCardsFinished = true;
    }

    /// <summary>
    /// 根据传入的花脸名称返回对应的花脸Sprite图像
    /// </summary>
    /// <param name="faceS"></param>
    /// <returns></returns>
    public Sprite GetFace(string faceS)
    {
        foreach (Sprite tS in faceSprites)
        {
            if (tS.name == faceS)
            {
                return tS;
            }
        }
        // 没找到就返回null
        return null;
    }

    /// <summary>
    /// 洗牌
    /// </summary>
    /// <param name="oCards">original cards，原始的卡牌</param>
    public void Shuffle(ref List<Card> oCards)
    {
        // tempCards，临时列表，存储洗牌后的纸牌的新顺序
        List<Card> tCards = new List<Card>();
        int ndx;

        while (oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
            // 原来这行代码在这个位置oCards = tCards;
        }

        oCards = tCards;

        isShuffleFinished = true;
    }
}
