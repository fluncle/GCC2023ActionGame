using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    // staticメンバにインスタンスを代入して各所からアクセスできるようにする
    public static EnemyManager Instance { get; private set; }
    
    /// <summary>ゲーム内で生きている敵リスト</summary>
    private List<Enemy> _enemies;

    private void Awake() {
        Instance = this;
        // 子オブジェクト内から全てのEnemyコンポーネントを検索して取得
        var enemyArray = transform.GetComponentsInChildren<Enemy>();
        // List化してメンバに代入
        _enemies = new List<Enemy>(enemyArray);
    }

    /// <summary>敵の削除処理</summary>
    /// <param name="enemy">削除する敵</param>
    public void Remove(Enemy enemy) {
        if (!_enemies.Contains(enemy)) {
            // リストに含まれていない敵の場合は処理をスキップ
            return;
        }

        // リストから敵を除外し、オブジェクトを削除
        _enemies.Remove(enemy);
        Destroy(enemy.gameObject);

        if (_enemies.Count <= 0 && !GameManager.Instance.Player.IsDead) {
            // 敵リスト内の敵の数が0になっていたらゲームクリア演出を呼び出し
            // ただし、既にプレイヤーが死亡していたらクリア演出は呼ばない
            GameUIManager.Instance.GameEndView.Play(true);
        }
    }
}
