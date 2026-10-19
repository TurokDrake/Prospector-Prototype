using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S; // 计分板单例

    public GameObject prefabFloatingScore;
    public Canvas canvas;

    public bool ______;

    [SerializeField]
    private int _score = 0;
    public string _scoreString;

    public int score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            scoreString = Utils.AddCommasToNumber(_score);
        }
    }

    public string scoreString
    {
        get
        {
            return _scoreString;
        }
        set
        {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }

    private void Awake()
    {
        S = this;
    }

    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }

    public FloatingScore CreateFloatingScore(int amt,List<Vector3> pts)
    {
        // 创建一个浮动分数并将其放在Canvas下
        // GameObject go = Instantiate(prefabFloatingScore);
        GameObject go = Instantiate(prefabFloatingScore, canvas.transform, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        //go.transform.SetParent(canvas.gameObject.transform, true);
        //go.GetComponent<Text>().rectTransform.position = new Vector3(8, 270, 0);
        // 获取其floatingScore组件并设置分数以及回调对象
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;
        fs.Init(pts);
        return fs;
    }
}
