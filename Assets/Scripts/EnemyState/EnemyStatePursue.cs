using UnityEngine;

/// <summary>プレイヤーを追いかける状態</summary>
public class EnemyStatePursue : EnemyStateBase {
    /// <summary>追跡を諦める距離</summary>
    private const float CANCEL_DISTANCE = 5f;
    
    public EnemyStatePursue(Enemy enemy) : base(enemy) { }

    /// <summary>このStateに遷移したときに最初に呼び出す処理</summary>
    protected override void Enter() {
        base.Enter();
        // 移動アニメーションのフラグを立てる
        _enemy.Animator.SetBool("IsMove", true);
    }

    /// <summary>State中に毎フレーム実行する処理</summary>
    protected override void Update() {
        base.Update();
        var player = GameManager.Instance.Player;

        // プレイヤーの方向を向く
        var playerPos = player.transform.position;
        _enemy.transform.LookAt(playerPos);

        // プレイヤーが攻撃範囲にいたら攻撃を実行
        var currentPos = _enemy.transform.position;
        var distance = Vector3.Distance(currentPos, playerPos);
        if (distance <= _enemy.AttackRange) {
            // 攻撃時は移動処理をせず処理を抜ける
            _enemy.Transition(new EnemyStateAttack(_enemy));
            return;
        }

        // 一定距離以上プレイヤーから離れたら追跡をやめる
        if (distance > CANCEL_DISTANCE) {
            // パトロール状態へ遷移
            _enemy.Transition(new EnemyStatePatrol(_enemy));
            return;
        }

        // プレイヤーに向かって移動
        var maxDistanceDelta = _enemy.MaxSpeed * Time.deltaTime;
        _enemy.transform.position = Vector3.MoveTowards(currentPos, playerPos, maxDistanceDelta);
    }

    /// <summary>Stateを抜けるときの処理</summary>
    protected override void Exit() {
        base.Exit();
        // 移動アニメーションのフラグを降ろす
        _enemy.Animator.SetBool("IsMove", false);
    }
}
