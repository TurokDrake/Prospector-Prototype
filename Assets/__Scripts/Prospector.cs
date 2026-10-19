using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 得分事件
public enum ScoreEvent
{
    draw, 
    mine, 
    mineGold,
    gameWin,
    gameLoss
}

// 控制游戏流程
public class Prospector : MonoBehaviour
{
    public static Prospector S; // 单例
    public static int SCORE_FROM_PREV_ROUND = 0; // 目前为止所有回合的得分
    public static int HIGH_SCORE = 0; // 最高分

    public float reloadDelay = 5f; // 延时1s

    public Vector3 fsPosMid = new Vector3(8f, 270f, 0);
    public Vector3 fsPosRun = new Vector3(8f, 365f, 0);
    public Vector3 fsPosMid2 = new Vector3(-13f, 365f, 0);
    public Vector3 fsPosEnd = new Vector3(8f, 270f, 0);

    public Deck deck; // 对Deck脚本的引用，方便解析DeckXml.xml中的信息
    public TextAsset deckXML; // 待解析的xml文本组件

    public Layout layout;
    public TextAsset layoutXML;
    public Vector3 layoutCenter; // 布局中心
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;

    public CardProspector target; // 有效牌只有一张
    public List<CardProspector> tableau; // 28张初始牌
    public List<CardProspector> discardPile; // 弃牌堆

    public List<CardProspector> drawPile; // 抽牌堆

    // 记录得分信息
    public int chain = 0; // 当前回合的分数
    public int scoreRun = 0;
    public int score = 0;

    public FloatingScore fsRun;

    public Text text_GameOver;
    public Text text_RoundResult;
    public Text text_HighScore;

    private void Awake()
    {
        S = this; // 绑定单例
        // 确认记录的得分的最高值
        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }
        // 将分数添加到上一轮，如果赢的话分数>0
        score += SCORE_FROM_PREV_ROUND;
        // 重置SCORE_FROM_PREV_ROUND
        SCORE_FROM_PREV_ROUND = 0;

        // 设置最后一轮显示的Text
        if (text_GameOver != null)
        {
            Debug.Log("text_GameOver获取成功！");
        }
        if (text_RoundResult != null)
        {
            Debug.Log("text_RoundResult获取成功！");
        }

        ShowResultText(false);

        string highScore = "High Score: " + Utils.AddCommasToNumber(HIGH_SCORE);
        text_HighScore.text = highScore;
    }

    void ShowResultText(bool show)
    {
        text_GameOver.gameObject.SetActive(show);
        text_RoundResult.gameObject.SetActive(show);
    }

    private void Start()
    {
        Scoreboard.S.score = score;
        // 初始化卡片，该过程包括 读取XML文件中关于卡牌的配置信息 和 初始化卡牌
        deck = GetComponent<Deck>();
        layout = GetComponent<Layout>();

        deck.InitDeck(deckXML);

        deck.MakeCards();
        
        deck.Shuffle(ref deck.cards);

        drawPile =ConvertListCardsToListCardProspectors(deck.cards);

        layout.ReadLayout(layoutXML);

        LayoutGame();

    }

    #region 删除的代码，请折叠起来
    // 封装了所有初始化过程
    // 使用协程存在一些问题——异步加载资源时不会阻塞其他方法
    //IEnumerator Init()
    //{
    //    deck.InitDeck("0");

    //    yield return new WaitUntil(() => { return deck.isReadDeckFinished; });

    //    deck.MakeCards();

    //    yield return new WaitUntil(() => { return deck.isMakeCardsFinished; });

    //    deck.Shuffle(ref Prospector.S.deck.cards);

    //    yield return new WaitUntil(() => { return deck.isShuffleFinished; });

    //    deck.ConvertListCardsToListCardProspectors();

    //    yield return new WaitUntil(() => { return deck.isConvertFinished; });

    //    layout.ReadLayout();

    //    yield return new WaitUntil(() => { return layout.isReadLayoutFinished; });

    //    LayoutGame();
    //}
    #endregion

    // 将其修改为公共方法，以确保在deck.cards正确读取后再转换
    public List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        // 初始化一个CardProspector列表，将作为最终的返回值
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        // 遍历传入的List<Card>，逐一转换成CardProspector
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }

        return lCP;
    }

    /// <summary>
    /// 从抽牌堆 (drawPile) 中移除并返回索引 0 处的 CardProspector。
    /// </summary>
    /// <remarks>调用前应确保 drawPile 非空，否则会引发异常。</remarks>
    /// <returns>已移除并返回的 CardProspector 实例。</returns>
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);
        return cd;
    }

    /// <summary>
    /// 布局并初始化游戏界面元素，计算并设置各控件的位置与大小。
    /// </summary>
    /// <remarks>应在 UI 线程上调用。保持幂等并避免长时间阻塞；将耗时资源加载或复杂计算移到异步任务或后台线程，布局操作仅负责控件排列和视图更新。</remarks>
    public void LayoutGame()
    {
        // 创建场景锚点
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            // layoutCenter 是布局中心
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;
        // 矿井中有28张牌，从牌堆抽28张牌作为矿井中的初始牌
        foreach (SlotDef tSD in layout.slotDefs)
        {
            cp = Draw(); // 抽牌
            cp.FaceUp = tSD.faceUp; // 设置哪一面向上
            cp.transform.parent = layoutAnchor; // 设置其父对象

            // 修改其位置
            cp.transform.localPosition = new Vector3(layout.multiplier.x*tSD.x, 
                layout.multiplier.y*tSD.y, 
                -tSD.layerID);
            // 设置其在矿井中的id
            cp.layoutID = tSD.id;
            // 设置其slot定义
            cp.slotDef = tSD;
            // 设置其状态
            cp.state = CardState.tableau;

            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);
        }

        // 遍历矿井中的牌
        foreach (CardProspector tCP in tableau)
        {
            // 看这张牌哪些牌覆盖了
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        // 抽一张有效牌
        MoveToTarget(Draw());

        // 更新抽牌堆
        UpdateDrawPile();

    }

    public void CardClicked(CardProspector cd)
    {
        switch (cd.state)
        {
            // 如果是抽牌堆的牌，就把“当前回合”的目标牌扔到弃牌堆，将这张牌作为新的目标牌，再更新抽牌堆
            case CardState.drawpile:
                MoveToDiscard(target); // 移动目标牌到弃牌堆
                MoveToTarget(Draw()); // 从抽牌堆抽出的牌作为目标牌
                UpdateDrawPile(); // 重洗储备牌
                ScoreManager(ScoreEvent.draw); // 是通过抽抽牌堆的牌获得的一分
                break;
            // 如果是矿井中的牌
            case CardState.tableau:
                // 单击画面中的纸牌将检查是否有效
                bool validMatch = true;
                // 朝下的牌是无法被点击的！！！
                if (!cd.FaceUp)
                {
                    validMatch = false;
                }
                // 如果这张牌和“当前回合”的目标牌点数不相邻，也是无法被点击的
                if (!AdjacentRank(cd, target))
                {
                    validMatch = false;
                }
                // 点击行为无效，就不执行任何动作
                if (!validMatch)
                {
                    return;
                }
                // 若有效，则将它从矿井中移除，并让它从矿井中的牌变为新的目标牌
                tableau.Remove(cd);
                MoveToTarget(cd);
                // 移除卡牌后，检查其下的卡牌是否能够露面
                SetTableauFaces();
                ScoreManager(ScoreEvent.mine); // 在矿井中的牌进入有效牌
                break;
            // 如果是“当前回合”的目标牌
            case CardState.target:
                // 目标纸牌不会受鼠标点击影响
                break;
        }
        // 每次点击完后判断游戏是否结束
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        // 如果矿井中没有卡牌，游戏结束,且为胜利
        if (tableau.Count == 0)
        {
            GameOver(true);
            return;
        }
        // 如果储备堆中还有牌，那么游戏未结束
        if (drawPile.Count > 0)
        {
            return;
        }
        // 检查剩余的可以玩的纸牌
        foreach (CardProspector cd in tableau)
        {
            // 如果这张牌和有效牌是点数相邻的，那么就能继续玩
            if (AdjacentRank(cd, target))
            {
                return;
            }
        }
        // 经过判断后，没有牌和有效牌相邻，就结束，而且判输
        GameOver(false);
    }

    void GameOver(bool won)
    {
        if (won)
        {
            ScoreManager(ScoreEvent.gameWin);
            //print("Game Over. You won!  :)");
        }
        else
        {
            ScoreManager(ScoreEvent.gameLoss);
            //print("Game Over. You Lost.  :(");
        }
        Invoke("ReloadLevel", reloadDelay);
    }

    void ReloadLevel()
    {
        // 重新加载此场景
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// 确定两张牌的点数是否相邻。（A&K也算相邻）
    /// </summary>
    /// <param name="c0">第一张待比较的牌。</param>
    /// <param name="c1">第二张待比较的牌。</param>
    /// <returns>若点数相邻则返回 true；否则返回 false。</returns>
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        // 若其中一张纸牌朝下，则无需判断，直接退出
        if (!c0.FaceUp || !c1.FaceUp)
        {
            return false;
        }

        // 相邻点数返回true
        if (Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return true;
        }

        if (c0.rank == 1 && c1.rank == 13)
        {
            return true;
        }
        if (c0.rank == 13 && c1.rank == 1)
        {
            return true;
        }

        return false;
    }

    void SetTableauFaces()
    {
        foreach (CardProspector cd in tableau)
        {
            bool fup = true; // 假设纸牌朝上
            // 遍历盖在这张牌上面的牌
            foreach (CardProspector cover in cd.hiddenBy)
            {
                // 如果其上面的牌状态还是“在矿井中”，说明其没有被移走，那么这张牌就背面朝上
                if (cover.state == CardState.tableau)
                {
                    fup = false;
                }
            }
            // 否则，这张牌就正面朝上
            cd.FaceUp = fup;
        }
    }

    CardProspector FindCardByLayoutID(int layoutID)
    {
        // 遍历矿井中的牌
        foreach (CardProspector tCP in tableau)
        {
            // 如果矿井中有牌的ID符合要求，就返回这张牌的ID
            if (tCP.layoutID == layoutID)
            {
                return tCP;
            }
        }
        // 没找到就返回空，并报告一下
        Debug.Log("FindCardByLayoutID()未找到id为layoutID的CardPprospector对象！");
        return null;
    }

    /// <summary>
    /// 更新抽牌堆的游戏状态并刷新其界面表示。
    /// </summary>
    /// <remarks>在牌组变更或回合开始时调用。会重新计算剩余牌数、处理洗牌与空堆逻辑，并触发界面或数据绑定刷新以同步状态。</remarks>
    private void UpdateDrawPile()
    {
        CardProspector cd;
        // 遍历储备牌
        for(int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            // 使用layout.drawPile.stagger定位
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x), layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);
            // 设置牌朝下，以及状态
            cd.FaceUp = false;
            cd.state = CardState.drawpile;
            // 深度排序
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    /// <summary>
    /// 抽一张牌作为有效牌
    /// </summary>
    /// <param name="cardProspector"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void MoveToTarget(CardProspector cd)
    {
        // 如果当前已有目标牌，则将其移到弃牌堆
        if(target!=null)
        {
            MoveToDiscard(target);
        }
        target = cd; // 更改目标牌
        // 修改牌状态和父对象
        cd.state = CardState.target;
        cd.transform.parent = layoutAnchor;
        // 移动到目标位置
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
        cd.FaceUp = true;
        // 深度排序
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    /// <summary>
    /// 把当前有效牌移动到弃牌堆
    /// </summary>
    /// <param name="target"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void MoveToDiscard(CardProspector cd)
    {
        // 修改卡牌状态为丢弃，并添加到弃牌堆
        cd.state = CardState.discard;
        discardPile.Add(cd);
        // 更新transform元素，将这张卡牌的父对象从抽牌堆的DeckAnchor改为layoutAnchor
        cd.transform.parent =layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f);
        // 定位到弃牌堆的牌是明牌，正面向上
        cd.FaceUp = true;
        // 放到牌堆顶部用于深度排序
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    /// <summary>
    /// 处理分数事件并触发相应的分数更新或通知。
    /// </summary>
    /// <param name="sEvt">包含分数变更的详细信息和上下文信息。</param>
    void ScoreManager(ScoreEvent sEvt)
    {
        List<Vector3> fsPts; // floating score points
        switch (sEvt)
        {
            // 无论是抽牌、赢或者输，都有对应的动作
            case ScoreEvent.draw:
            case ScoreEvent.gameWin:
            case ScoreEvent.gameLoss:
                chain = 0; // 重置当前得分
                score += scoreRun; // 加分
                scoreRun = 0;

                if (fsRun != null)
                {
                    fsPts = new List<Vector3>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);

                    fsRun.fontSizes = new List<float>(new float[] { 100, 120, 140 });
                    fsRun = null;
                }
                break;
            case ScoreEvent.mine:
                chain++; // 将要加的分数
                scoreRun += chain; // 当前回合的分数加上得分

                FloatingScore fs;
                //Vector3 p0 = Input.mousePosition;
                //p0.x = p0.x / Screen.width;
                //p0.y = p0.y / Screen.height;
                fsPts = new List<Vector3>();
                // fsPts.Add(p0);
                fsPts.Add(fsPosRun);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(chain, fsPts);
                fs.fontSizes = new List<float>(new float[] { 100, 140, 120 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }

        switch (sEvt)
        {
            case ScoreEvent.gameWin:
                text_GameOver.text = "Round Over";
                // 如果赢了，就加上一轮的分数并重置上一轮的分数
                Prospector.SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                text_RoundResult.text = "You won this round!\nRound Score: " + score;
                ShowResultText(true);
                break;
            case ScoreEvent.gameLoss:
                text_GameOver.text = "Game Over";
                // 如果输了，与最高分进行比较
                if (Prospector.HIGH_SCORE <= score)
                {
                    print("You got the high score! High score: " + score);
                    string sRR = "You got the high score!\nHigh score: " + score;
                    text_RoundResult.text = sRR;
                    Prospector.HIGH_SCORE = score; // 新记录
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    print("Your final score for the game was: " + score);
                    text_RoundResult.text = "Your final score was: " + score;
                }
                ShowResultText(true);
                break;
            default:
                print("score: " + score + "scoreRun :" + scoreRun + "chain: " + chain);
                break;
        }
    }

}
