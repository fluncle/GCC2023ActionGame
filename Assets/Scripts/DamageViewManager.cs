using UnityEngine;

public class DamageViewManager : MonoBehaviour {
    // staticメンバにインスタンスを代入して各所からアクセスできるようにする
    public static DamageViewManager Instance { get; private set; }

    /// <summary>ダメージ表示のプレハブ</summary>
    [SerializeField] private DamageView _viewPrefab;

    /// <summary>ダメージ表示の親Transform</summary>
    [SerializeField] private Transform _viewsParent;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        Instance = this;
    }

    /// <summary>ダメージ表示を再生</summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="worldPosition">表示位置（ワールド座標）</param>
    public void Play(int damage, Vector3 worldPosition) {
        // ダメージ表示のインスタンス生成
        var view = Instantiate(_viewPrefab, _viewsParent);
        view.Play(damage, worldPosition);
    }
}
