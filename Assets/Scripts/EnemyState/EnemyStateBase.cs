/// <summary>敵の状態遷移の1状態の規定クラス</summary>
public class EnemyStateBase : StateBase {
    /// <summary>この状態を動作させている敵</summary>
    protected Enemy _enemy;

    protected EnemyStateBase(Enemy enemy) {
        _enemy = enemy;
    }
}
