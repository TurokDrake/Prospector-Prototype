using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Floating Score State
public enum FSState
{
    idle, // 闲置
    pre, // 准备
    active, // 正处于激活状态
    post // 已完成
}

public class FloatingScore : MonoBehaviour
{
    public FSState state = FSState.idle; // 默认状态是闲置，即不浮动
    [SerializeField]
    private int _score = 0; // 分数
    public string scoreString; // 字符串，传给Text组件的文本

    // 分数，设置时顺便设置字符串文本，并将Text组件的文本设置为字符串文本
    public int score
    {
        get { return _score; }
        set
        {
            _score = value;
            scoreString = Utils.AddCommasToNumber(_score);
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector3> bezierPts; // 需要进行贝塞尔插值的点，用于实现字体位置的抖动效果
    public List<float> fontSizes; // 需要进行贝塞尔插值的浮点数，表示字体大小
    public float timeStart = -1f; // 开始进行插值的时间点
    public float timeDuration = 1f; // 插值的持续时间
    public string easingCurve = Easing.InOut; // Utils.cs是个工具类，其中定义了Easing类型，即缓动类型；这里选择缓入缓出

    // 移动完成时游戏对象将接收SendMessage
    public GameObject reportFinishTo = null;
    public Text fsText;

    public void Init(List<Vector3> ePts,float eTimeS=0,float eTimeD = 1)
    {
        // 设置要进行贝塞尔插值的点，从外部传入
        bezierPts = new List<Vector3>(ePts);
        // 如果只有一个点，那么这个点就是终点
        if (ePts.Count == 1)
        {
            // 直接将字体运行到这里
            //transform.position = ePts[0];
            //fsText.rectTransform.position = ePts[0];
            fsText.rectTransform.anchoredPosition3D = ePts[0];
            return;
        }
         // 如果没有给开始运动的时间（easing time start），就从当前时间开始运动
        if (eTimeS == 0)
        {
            eTimeS = Time.time;
        }
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = FSState.pre; // 设置为准备状态
    }

    // 回调函数
    public void FSCallback(FloatingScore fs)
    {
        // 在回调函数中加上分数
        score += fs.score;
    }

    private void Update()
    {
        // 如果字体处于闲置状态，就不继续执行后面的逻辑
        if (state == FSState.idle)
        {
            return;
        }
        // 否则开始插值

        // 获取插值的u
        float u = (Time.time - timeStart) / timeDuration;
        // 获取经过插值的u
        float uC = Easing.Ease(u, easingCurve);
        // 如果u为负数，就设置当前状态为预备状态，并将起点设置为第0个点
        if (u < 0)
        {
            state = FSState.pre;
            // transform.position = bezierPts[0];
            //fsText.rectTransform.position = bezierPts[0];
            fsText.rectTransform.anchoredPosition = bezierPts[0];
        }
        else
        {
            // 说明已经完成移动
            if (u >= 1)
            {
                uC = 1;
                state = FSState.post; // 已经完成插值
                // 使用回调函数计分
                if (reportFinishTo != null)
                {
                    // 每个绑定到这个对象上的脚本中如果有名为"FSCallback"都会调用
                    reportFinishTo.SendMessage("FSCallback", this);

                    Destroy(gameObject);
                }
                else
                {
                    state = FSState.idle;
                }
                
            }
            // 0<=u<=1，表明当前对象正在移动
            else
            {
                state = FSState.active;
            }

            // 使用贝塞尔插值移动点
            Vector3 pos = Utils.Bezier(uC, bezierPts);
            //transform.position = pos;
            //fsText.rectTransform.position = pos;
            fsText.rectTransform.anchoredPosition = pos;

            // 如果有用于贝塞尔插值的数，就进行贝塞尔插值
            if (fontSizes != null && fontSizes.Count > 0)
            {
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}
