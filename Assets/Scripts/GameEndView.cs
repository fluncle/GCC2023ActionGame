using UnityEngine;
using UnityEngine.UI;

/// <summary>ゲーム終了時の演出</summary>
public class GameEndView : MonoBehaviour {
    [SerializeField] private CanvasGroup _canvasGroup;

    /// <summary>画面全体に半透明の色を付けるImage</summary>
    [SerializeField] private Image _layer;

    /// <summary>ゲームクリア/オーバーを表示するテキスト</summary>
    [SerializeField] private Text _text;

    /// <summary>テキストの影</summary>
    [SerializeField] private Shadow _textShadow;

    /// <summary>演出再生</summary>
    /// <param name="isGameClear">ゲームクリアによる終了か否か</param>
    public void Play(bool isGameClear) {
        // ゲームクリアかゲームオーバーかで各種色とテキストを設定
        if (isGameClear) {
            _layer.color = new Color(1f, 1f, 1f, 0.1f);
            _text.text = "GAME CLEAR";
            _text.color = new Color(1f, 0.8f, 0f);
            _textShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        }
        else {
            _layer.color = new Color(0f, 0f, 0f, 0.75f);
            _text.text = "GAME OVER";
            _text.color = new Color(0f, 0.5f, 1f);
            _textShadow.effectColor = new Color(0.75f, 0.75f, 0.75f, 0.2f);
        }

        // CanvasGroupで表示全体のアルファ、タッチ判定などを有効化
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
}
