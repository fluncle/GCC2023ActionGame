/// <summary>死亡状態</summary>
public class EnemyStateDead : EnemyStateBase {
    public EnemyStateDead(Enemy enemy) : base(enemy) { }

    /// <summary>このStateに遷移したときに最初に呼び出す処理</summary>
    protected override void Enter() {
        base.Enter();
        // 死亡アニメーションのトリガーを起動
        _enemy.Animator.SetTrigger("Dead");
        // 2秒後に削除処理を呼び出す
        _enemy.Invoke(nameof(_enemy.EndDead), 2f);
    }
}
