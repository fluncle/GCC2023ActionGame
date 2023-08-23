using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    // staticメンバにインスタンスを代入して各所からアクセスできるようにする
    public static GameManager Instance { get; private set; }

    /// <summary>プレイヤー</summary>
    public Player Player => _player;
    [SerializeField] private Player _player;

    /// <summary>ゲーム時間</summary>
    [SerializeField] private int _maxTime;
    
    /// <summary>ゲーム時間のカウント</summary>
    private float _time;
    
    /// <summary>時間切れか否か</summary>
    public bool IsTimeOver => _time <= 0f;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        Instance = this;
        _time = _maxTime;
    }

    /// <summary>実行開始前の処理</summary>
    private void Start() {
        // GameUIManager.InstanceはAwakeで設定されるので、Startでアクセスする
        GameUIManager.Instance.TimeView.SetTime(_time);
    }

    /// <summary>ゲームリセット</summary>
    public void ResetScene() {
        // ゲームシーンを読み込むことでリセットする
        SceneManager.LoadScene(0);
    }

    /// <summary>更新処理</summary>
    private void Update() {
        if (IsTimeOver || !EnemyManager.Instance.IsExistEnemy) {
            // 残り時間が既に0か、ゲームクリア済みなら処理を抜ける
            return;
        }
        
        // 経過時間を減算。ただし、0未満にならないようにする
        _time = Mathf.Max(_time - Time.deltaTime, 0f);
        GameUIManager.Instance.TimeView.SetTime(_time);
        
        if (IsTimeOver) {
            // 残り時間が0になったらゲームオーバー
            GameUIManager.Instance.GameEndView.Play(false);
        }
    }
}
