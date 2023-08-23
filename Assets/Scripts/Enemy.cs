using System;
using DG.Tweening;
using UnityEngine;

public class Enemy : MonoBehaviour {
    /// <summary>最大HP</summary>
    private const int MAX_HP = 30;

    public Animator Animator => _animator;
    [SerializeField] private Animator _animator;

    /// <summary>現在の状態</summary>
    private StateBase _state;

    /// <summary>移動最高速度(m/s)</summary>
    public float MaxSpeed => _maxSpeed;
    [SerializeField] private float _maxSpeed;

    /// <summary>旋回最高速度(m/s)</summary>
    public float MaxTurnSpeed => _maxTurnSpeed;
    [SerializeField] private float _maxTurnSpeed;

    /// <summary>攻撃開始距離(m)</summary>
    public float AttackRange => _attackRange;
    [SerializeField] private float _attackRange;

    /// <summary>攻撃判定のコライダー</summary>
    [SerializeField] private Attacker _attacker;

    private int _hp;

    /// <summary>喰らい判定のコライダー</summary>
    public Collider HitCollider => _hitCollider;
    [SerializeField] private Collider _hitCollider;

    /// <summary>色の点滅演出のシーケンス</summary>
    private Sequence _blinkColorSeq;

    /// <summary>3Dモデルのレンダラー</summary>
    [SerializeField] private Renderer _bodyRenderer;

    /// <summary>振動演出のシーケンス</summary>
    private Sequence _shakeSeq;
    
    /// <summary>振動のオフセットベクトル</summary>
    private Vector3 _shakeOffset;

    /// <summary>振動演出のRoot</summary>
    [SerializeField] private Transform _shakeRoot;
    
    /// <summary>ノックバック演出のシーケンス</summary>
    private Sequence _knockBackSeq;

    /// <summary>死亡しているか否か</summary>
    public bool IsDead => _hp <= 0;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        // 待機状態から開始
        _state = new EnemyStatePatrol(this);
        _hp = MAX_HP;
    }

    /// <summary>状態遷移</summary>
    /// <param name="state">次に実行する状態</param>
    public void Transition(StateBase state) {
        _state.Transition(state);
    }

    /// <summary>更新処理</summary>
    private void Update() {
        if (GameManager.Instance.Player.IsDead) {
            // プレイヤー死亡時は何もしない
            return;
        }

        // 現在の状態の更新処理
        _state = _state.Process();
    }

    /// <summary>
    /// 攻撃のインパクトのタイミングの処理
    /// 攻撃モーションの設定したAnimationEventから呼び出される
    /// </summary>
    private void AttackImpactEvent() {
        // 攻撃の威力を設定する
        _attacker.SetData(20);
        // 攻撃判定を有効にする
        _attacker.Collider.enabled = true;
        // 0.1秒後に攻撃判定を無効にする処理を呼び出す
        Invoke(nameof(DisableAttackCollider), 0.1f);
    }

    /// <summary>攻撃判定を無効化</summary>
    private void DisableAttackCollider() {
        _attacker.Collider.enabled = false;
    }

    /// <summary>TriggerのColliderとの接触処理</summary>
    /// <param name="other">接触したコライダー</param>
    private void OnTriggerEnter(Collider other) {
        // 接触したコライダーがPlayerAttackタグならダメージ処理
        if (other.CompareTag("PlayerAttack")) {
            // 攻撃情報を持つAttackerコンポーネントを取得する
            var attacker = other.GetComponent<Attacker>();
            if (_hp <= 0) {
                // HPが既に0なら処理を抜ける
                return;
            }
            // ダメージ状態へ遷移
            Transition(new EnemyStateDamage(this, attacker.Power, other));
        }
    }

    /// <summary>HPを減らす</summary>
    /// <param name="amount">減少量</param>
    public void ReduceHP(int amount) {
        _hp = Mathf.Max(_hp - amount, 0);
    }

    /// <summary>色点滅の演出を再生</summary>
    /// <param name="color">点滅の色</param>
    public void BlinkColor(Color color) {
        // レンダラーからマテリアルを取得する
        // TIPS: Rendererのmaterialにアクセスすると、そのタイミングでアタッチされているmaterialが複製され
        //       Rendererに対してユニークなインスタンスとして扱えます
        var material = _bodyRenderer.material;

        // 前回の_blinkColorSeqがまだ再生中だった場合を考慮して、演出の強制終了メソッドを呼び出し
        _blinkColorSeq?.Kill();
        
        // 0.1秒で引数の色に変化させ、その後0.15秒で元の色に戻す演出を作成・再生
        _blinkColorSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(DOTween.To(() => Color.black, c => material.SetColor("_Color", c), color, 0.1f))
            .Append(DOTween.To(() => color, c => material.SetColor("_Color", c), Color.black, 0.15f));
    }

    /// <summary>振動演出を再生</summary>
    public void ShakeBody() {
        // 前回の_shakeSeqがまだ再生中だった場合を考慮して、演出の強制終了メソッドを呼び出し
        _shakeSeq?.Kill();

        // 0.5秒間、ランダムな方向に0.25mの幅で30回振動する演出を作成・再生
        // NOTE: 今回振動させるRootがAnimatorによって毎フレーム座標が上書きされるため、LateUpdateでオフセット値を加える形に対応している
        _shakeSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(DOTween.Shake(() => Vector3.zero, offset => _shakeOffset = offset, 0.5f, 0.25f, 30))
            .OnUpdate(() => _shakeRoot.localPosition += _shakeOffset)
            .SetUpdate(UpdateType.Late);
    }

    /// <summary>ノックバック演出</summary>
    /// <param name="onComplete">ノックバック終了時の処理</param>
    public void KnockBack(Action onComplete = null) {
        // 前回の_knockBackSeqがまだ再生中だった場合を考慮して、演出の強制終了メソッドを呼び出し
        _knockBackSeq?.Kill();

        // 後ろ方向0.8m位置をノックバック移動目標座標とする
        var endPos = transform.position - transform.forward * 1.2f;
        
        // 0.5秒でendPosに移動した後、onCompleteの処理を呼び出す演出を作成・再生
        _knockBackSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(transform.DOMove(endPos, 0.5f).SetEase(Ease.OutCubic))
            .OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>死亡終了</summary>
    public void EndDead() {
        // オブジェクトを削除
        EnemyManager.Instance.Remove(this);
    }

    /// <summary>オブジェクト削除時の処理</summary>
    private void OnDestroy() {
        // 登録したInvokeをすべてキャンセル
        CancelInvoke();
    }
}
