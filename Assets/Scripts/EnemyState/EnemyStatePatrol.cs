using UnityEngine;

/// <summary>プレイヤーを探している状態</summary>
public class EnemyStatePatrol : EnemyStateBase {
    /// <summary>探査範囲</summary>
    private const float SEARCH_DISTANCE = 5f;

    public EnemyStatePatrol(Enemy enemy) : base(enemy) { }

    /// <summary>State中に毎フレーム実行する処理</summary>
    protected override void Update() {
        base.Update();
        var player = GameManager.Instance.Player;
        var distance = Vector3.Distance(_enemy.transform.position, player.transform.position);
        // プレイヤーとの距離が一定以内になったら追跡を開始
        if (distance < SEARCH_DISTANCE) {
            // 追跡状態へ遷移
            _enemy.Transition(new EnemyStatePursue(_enemy));
        }
    }
}
