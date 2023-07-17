using UnityEngine;

/// <summary>
/// UI管理クラス
/// 各種UIコンポーネントに各所からアクセスできるようにすることが主な目的
/// </summary>
public class GameUIManager : MonoBehaviour {
    // staticメンバにインスタンスを代入して各所からアクセスできるようにする
    public static GameUIManager Instance { get; private set; }

    /// <summary>プレイヤーのHPゲージ</summary>
    public HPGaugeView PlayerHPGauge => _playerHPGauge;
    [SerializeField] private HPGaugeView _playerHPGauge;

    /// <summary>ゲーム終了演出</summary>
    public GameEndView GameEndView => _gameEndView;
    [SerializeField] private GameEndView _gameEndView;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        Instance = this;
    }
}
