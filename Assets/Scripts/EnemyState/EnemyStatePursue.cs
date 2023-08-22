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

        var playerPos = player.transform.position;
        var enemyrT = _enemy.transform;
        var enemyrRot = _enemy.transform.rotation;

        // プレイヤーの方向へ回転
        var targetVector = playerPos - enemyrT.position;
        var lookRotation = Quaternion.LookRotation(playerPos - enemyrT.position);
        var turnDegrees = _enemy.Param.MaxTurnSpeed * Time.deltaTime;
        _enemy.transform.rotation = Quaternion.RotateTowards(enemyrRot, lookRotation, turnDegrees);

        // 攻撃範囲内かつプレイヤーの方向を向いていたら攻撃を実行
        var distance = Vector3.Distance(enemyrT.position, playerPos);
        var degrees = Vector3.Angle(_enemy.transform.forward, targetVector);
        if (distance <= _enemy.Param.AttackRange && degrees <= 10f) {
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

        // 前方へ移動
        var moveDistance = _enemy.Param.MaxSpeed * Time.deltaTime;
        _enemy.transform.position += _enemy.transform.forward * moveDistance;
    }

    /// <summary>Stateを抜けるときの処理</summary>
    protected override void Exit() {
        base.Exit();
        // 移動アニメーションのフラグを降ろす
        _enemy.Animator.SetBool("IsMove", false);
    }
}
