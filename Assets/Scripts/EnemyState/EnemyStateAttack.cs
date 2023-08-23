using UnityEngine;

/// <summary>攻撃状態</summary>
public class EnemyStateAttack : EnemyStateBase {
    /// <summary>進捗タイプ</summary>
    private enum Progress {
        /// <summary>攻撃前の準備</summary>
        Ready,
        /// <summary>攻撃後の硬直</summary>
        Rigid,
    }

    private float _countTime;

    /// <summary>攻撃進行状態</summary>
    private Progress _progress;

    public EnemyStateAttack(Enemy enemy) : base(enemy) { }

    /// <summary>このStateに遷移したときに最初に呼び出す処理</summary>
    protected override void Enter() {
        base.Enter();
        // 進捗を攻撃前の準備へ進める
        _progress = Progress.Ready;
    }

    /// <summary>State中に毎フレーム実行する処理</summary>
    protected override void Update() {
        base.Update();

        _countTime += Time.deltaTime;

        switch (_progress) {
            case Progress.Ready:
                if (_countTime >= _enemy.Param.AttackReadyTime) {
                    // 攻撃アニメーションのトリガーを起動
                    _enemy.Animator.SetTrigger("Attack");
                    // 進捗を攻撃後の硬直へ進める
                    _progress = Progress.Rigid;
                }
                break;
            case Progress.Rigid:
                if (_countTime >= _enemy.Param.AttackRigidTime) {
                    // 待機状態へ遷移
                    Transition(new EnemyStatePatrol(_enemy));
                }
                break;
        }
    }
}
