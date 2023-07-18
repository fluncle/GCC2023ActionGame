using UnityEngine;
using UnityEngine.UI;

public class DamageView : MonoBehaviour {
    /// <summary>ダメージ量のテキスト</summary>
    [SerializeField] private Text _text;

    /// <summary>表示するワールド座標</summary>
    private Vector3 _playPosition;

    /// <summary>再生</summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="worldPosition">表示位置（ワールド座標）</param>
    public void Play(int damage, Vector3 worldPosition) {
        _text.text = damage.ToString();

        // 表示位置を更新
        _playPosition = worldPosition;
        Update();
        
        // 0.5秒後に削除処理を呼ぶ
        Invoke(nameof(Destroy), 0.5f);
    }

    /// <summary>更新処理</summary>
    private void Update() {
        // 表示位置のワールド座標をスクリーン空間に変換して代入
        transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, _playPosition);
    }

    /// <summary>削除処理</summary>
    private void Destroy() {
        Destroy(gameObject);
    }
}
