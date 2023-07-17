using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    // staticメンバにインスタンスを代入して各所からアクセスできるようにする
    public static GameManager Instance { get; private set; }

    /// <summary>プレイヤー</summary>
    public Player Player => _player;
    [SerializeField] private Player _player;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        Instance = this;
    }

    /// <summary>ゲームリセット</summary>
    public void ResetScene() {
        // ゲームシーンを読み込むことでリセットする
        SceneManager.LoadScene(0);
    }
}
