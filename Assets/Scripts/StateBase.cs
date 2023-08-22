/// <summary>状態遷移の1状態の規定クラス</summary>
public abstract class StateBase {
    /// <summary>State内イベント</summary>
    private enum Event {
        Enter,
        Update,
        Exit
    }

    /// <summary>State内のイベントを格納</summary>
    private Event _stage;

    /// <summary>遷移先のState</summary>
    private StateBase _nextState;

    /// <summary>このStateに遷移したときに最初に呼び出す処理</summary>
    protected virtual void Enter() {
        _stage = Event.Update;
    }

    /// <summary>State中に毎フレーム実行する処理</summary>
    protected virtual void Update() { }

    /// <summary>Stateを抜けるときの処理</summary>
    protected virtual void Exit() { }

    /// <summary>
    /// Stateの更新処理
    /// 外部のUpdateから呼び出してStateが持つ機能を動作させる
    /// </summary>
    /// <returns>次に実行するState</returns>
    public StateBase Process() {
        switch (_stage) {
            case Event.Enter:
                Enter();
                break;
            case Event.Update:
                Update();
                break;
            case Event.Exit:
                Exit();
                // Exit時は、遷移先のStateを返す
                return _nextState;
        }

        // 現在のStateを返す
        return this;
    }

    /// <summary>状態遷移</summary>
    /// <param name="state">次に実行する状態</param>
    public void Transition(StateBase state) {
        _nextState = state;
        _stage = Event.Exit;
    }
}
