/// <summary>攻撃状態</summary>
public class EnemyStateAttack : EnemyStateBase {
    public EnemyStateAttack(Enemy enemy) : base(enemy) { }

    /// <summary>このStateに遷移したときに最初に呼び出す処理</summary>
    protected override void Enter() {
        base.Enter();
        // 攻撃アニメーションのトリガーを起動
        _enemy.Animator.SetTrigger("Attack");
    }
}
