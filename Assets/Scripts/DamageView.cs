using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DamageView : MonoBehaviour {
    /// <summary>ダメージ量のテキスト</summary>
    [SerializeField] private Text _text;

    /// <summary>表示するワールド座標</summary>
    private Vector3 _playPosition;

    /// <summary>移動アニメーションのRootオブジェクト</summary>
    [SerializeField] private Transform _moveRoot; 

    /// <summary>再生</summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="worldPosition">表示位置（ワールド座標）</param>
    public void Play(int damage, Vector3 worldPosition) {
        _text.text = damage.ToString();

        // 表示位置を更新
        _playPosition = worldPosition;
        Update();

        // 上方向に対して±30°のランダムな角度のベクトルを取得
        var moveVector = Quaternion.Euler(0f, 0f, Random.Range(-30, 30)) * Vector3.up;
        // 表示を0.3秒で200px動かすアニメーションを作成・再生
        DOTween.Sequence()
            .SetLink(gameObject)
            .Append(_moveRoot.DOLocalMove(moveVector * 200f, 0.3f).SetEase(Ease.OutCubic));
        
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
