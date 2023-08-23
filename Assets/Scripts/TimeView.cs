using UnityEngine;
using UnityEngine.UI;

/// <summary>時間表示</summary>
public class TimeView : MonoBehaviour {
    [SerializeField] private Text _text;

    /// <summary>秒数を渡して分:秒に変換して表示</summary>
    public void SetTime(float time) {
        var minutes = Mathf.FloorToInt(time / 60f);
        var seconds = Mathf.FloorToInt(time % 60f);
         _text.text = $"{minutes:00}:{seconds:00}";
    }
}