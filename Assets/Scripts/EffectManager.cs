using UnityEngine;

public class EffectManager : MonoBehaviour {
    // staticメンバにインスタンスを代入して各所からアクセスできるようにする
    public static EffectManager Instance { get; private set; }

    /// <summary>ダメージ表示の親Transform</summary>
    [SerializeField] private Transform _effectsParent;

    /// <summary>攻撃ヒットエフェクトのプレハブ</summary>
    [SerializeField] private ParticleSystem _attackHitPrefab;

    /// <summary>致死ダメージの攻撃ヒットエフェクトのプレハブ</summary>
    [SerializeField] private ParticleSystem _attackHitDeadPrefab;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        Instance = this;
    }

    /// <summary>攻撃ヒットエフェクトを再生</summary>
    public void PlayAttackHit(Vector3 position) {
        Instantiate(_attackHitPrefab, position, Quaternion.identity, _effectsParent);
    }

    /// <summary>攻撃ヒットエフェクトを再生</summary>
    public void PlayAttackHitDead(Vector3 position) {
        Instantiate(_attackHitDeadPrefab, position, Quaternion.identity, _effectsParent);
    }
}
