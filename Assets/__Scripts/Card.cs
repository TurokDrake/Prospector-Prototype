using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 纸牌的信息
public class Card : MonoBehaviour
{
    public string suit; // 卡牌上的花色
    public int rank; // 牌的点数（1-13）
    public Color color = Color.black; // 花色符号的颜色
    public string colS = "Black"; // 颜色名称，纸牌上花色只需红与黑
    public List<GameObject> decoGOs = new List<GameObject>(); // decorator GameObjects
    public List<GameObject> pipGOs = new List<GameObject>(); // pip GameObjects

    public GameObject back; // 纸牌背面图像
    public CardDefinition def; // 卡牌的定义，从XML文件中读取
    public SpriteRenderer[] spriteRenderers; // 当前游戏对象的SpriteRenderer组件列表


    public bool FaceUp // true时正面朝上，false时背面朝上
    {
        get { return !back.activeSelf; }
        set { back.SetActive(!value); }
    }

    private void Start()
    {
        SetSortOrder(0); // 保证纸牌在一开始处于正确的深度排序
    }

    // 单击时，输出纸牌名
    virtual public void OnMouseUpAsButton()
    {
        print(name);
    }

    /// <summary>
    /// 如果未定义spriteRenderers，就使用该方法寻找
    /// </summary>
    public void PopulateSpriteRenderers()
    {
        // 如果spriteRenderers未存储对象的SpriteRenderer组件的引用，就初始化
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    /// <summary>
    /// 设置所有SpriteRenderer组件的sortingLayerName
    /// </summary>
    /// <param name="tSLN">temp sorting layer name</param>
    public void SetSortingLayerName(string tSLN)
    {
        // 先检查spriteRenderers是否有值
        PopulateSpriteRenderers();
        // 为每一个spriteRenderer组件设置sorting layer name
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }

    /// <summary>
    /// 设置排序顺序
    /// </summary>
    /// <param name="sOrd">sorting order</param>
    public void SetSortOrder(int sOrd)
    {
        // 检查spriteRenderers是否有值
        PopulateSpriteRenderers();
        
        // 白色背景的纸牌在底部
        // 在顶部的是花色、角码和脸
        // 背面应在顶部，因为当背面朝上时要覆盖其他图案

        // 遍历spriteRenderers
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            // 如果spriteRenderer的游戏对象是当前游戏对象（Prefab Card），说明这是卡牌背景
            if (tSR.gameObject == this.gameObject)
            {
                // 如果是，就设置当前的排序（0）
                tSR.sortingOrder = sOrd;
                continue;
            }
            // 如果不是，就检查spriteRenderer的游戏对象的名称
            switch (tSR.gameObject.name)
            {
                // 如果是back，表示背面，就显示在最高层
                case "Back":
                    tSR.sortingOrder = sOrd + 2;
                    break;

                // 如果不是back，就显示在中间层（空白卡的上方）
                case "face":
                default:
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

}

// 在每一张卡牌上都出现的内容，即角码和角码下的花色图案，序列化以在Inspector中显示
[System.Serializable]
public class Decorator
{
    public string type; // 对于花色符号，type = "pip"
    public Vector3 loc; // Sprite在纸片上的位置信息
    public bool flip = false; // 是否翻转Sprite
    public float scale = 1f; // 缩放
}

// 纸牌配置信息，序列化以在Inspector中显示
[System.Serializable]
public class CardDefinition
{
    public string face; // 使用的Sprite名称
    public int rank; // 点数
    public List<Decorator> pips = new List<Decorator>(); // 使用到的花色符号
}
