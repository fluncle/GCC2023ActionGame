using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour {
    /// <summary>最大HP</summary>
    private const int MAX_HP = 100;

    [SerializeField] private Animator _animator;

    /// <summary>画面タップ入力管理</summary>
    [SerializeField] private InputTapScreen _inputTapScreen;
    
    /// <summary>バーチャルパッド入力管理</summary>
    [SerializeField] private InputVirtualJoyStick _inputVirtualJoyStick;

    /// <summary>移動最高速度(m/s)</summary>
    [SerializeField] private float _maxSpeed;

    /// <summary>攻撃中フラグ</summary>
    private bool _attackFlag;

    /// <summary>攻撃判定のコライダー</summary>
    [SerializeField] private Attacker _attacker;

    /// <summary>攻撃目標の敵</summary>
    private Enemy _attackTarget;

    /// <summary>攻撃目標に向かって回転する速度(angle/s)</summary>
    [SerializeField] private float _attackRotateSpeed;

    /// <summary>攻撃によって前進する距離(m)</summary>
    [SerializeField] private float _attackAdvanceDistance;

    /// <summary>攻撃前進の最高速度(m/s)</summary>
    [SerializeField] private float _attackAdvanceMaxSpeed;
    
    /// <summary>攻撃中に前進した距離カウント</summary>
    private float _attackAdvanceCount;

    private int _hp;

    /// <summary>ダメージ中フラグ</summary>
    private bool _damageFlag;

    /// <summary>喰らい判定のコライダー</summary>
    [SerializeField] private Collider _hitCollider;

    /// <summary>色の点滅演出のシーケンス</summary>
    private Sequence _blinkColorSeq;

    /// <summary>3Dモデルのレンダラー</summary>
    [SerializeField] private Renderer _bodyRenderer;

    /// <summary>死亡しているか否か</summary>
    public bool IsDead => _hp <= 0;

    // 攻撃中、またはダメージ中でなければ各種入力を受け付ける
    private bool CanInput => !_attackFlag && !_damageFlag;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        // 画面タップ入力時のイベント登録
        _inputTapScreen.OnPerformEvent = Attack;
        
        // バーチャルパッド入力開始・終了時のイベント登録
        _inputVirtualJoyStick.OnStartEvent = ()=> _animator.SetBool("IsMove", true);
        _inputVirtualJoyStick.OnCancelEvent = ()=> _animator.SetBool("IsMove", false);
    }

    /// <summary>実行開始前の処理</summary>
    private void Start() {
        _hp = MAX_HP;
        // GameUIManager.InstanceはAwakeで設定されるので、Startでアクセスする
        GameUIManager.Instance.PlayerHPGauge.Initialize(MAX_HP);
    }

    /// <summary>更新処理</summary>
    private void Update() {
        if (_attackFlag) {
            UpdateAttackRotate();
            UpdateAttackAdvance();
        }
        
        if (!CanInput) {
            // 入力禁止時は処理を抜ける
            return;
        }

        if (_inputVirtualJoyStick.IsInput) {
            // バーチャルパット操作で移動
            Move(_inputVirtualJoyStick.Vector);
        }
    }

    /// <summary>移動</summary>
    /// <param name="vector">スティックの入力</param>
    private void Move(Vector2 vector) {
        // 移動アニメーションにスティックの入力量(0〜1)を設定
        var speedRate = vector.magnitude;
        // スティックの入力量から移動速度を計算
        var speed = Mathf.Lerp(0f, _maxSpeed, speedRate);
        // 今回のフレームでの移動量を計算
        var moveVector = new Vector3(vector.x, 0f, vector.y) * speed * Time.deltaTime;
        // 移動先の座標を計算
        var position = transform.position + moveVector;

        // 移動先に向きを変更
        transform.LookAt(position);

        // 座標を更新
        transform.position = position;
        
        // 移動モーションのブレンド値を設定
        _animator.SetFloat("Speed", speedRate);
    }
    
    /// <summary>攻撃</summary>
    private void Attack() {
        if (!CanInput) {
            // 入力禁止時は処理を抜ける
            return;
        }

        // 攻撃中フラグを立てる
        _attackFlag = true;
        // 攻撃アニメーションのトリガーを起動
        _animator.SetTrigger("Attack");

        // 半径4m以内にいる最寄りの敵を攻撃目標として設定する
        _attackTarget = EnemyManager.Instance.GetNearestEnemy(transform.position, 4f);

        _attackAdvanceCount = 0f;
    }

    /// <summary>攻撃時に敵に向かって回転させる更新処理</summary>
    private void UpdateAttackRotate() {
        if (_attackTarget == null) {
            // 攻撃目標がいなければ何もしない
            return;
        }

        // 攻撃目標への向き(Y角度)を計算
        var targetVector = _attackTarget.transform.position - transform.position;
        var targetAngleY = Quaternion.LookRotation(targetVector).eulerAngles.y;
        
        var eulerAngles = transform.eulerAngles;
        // 回転速度を加味した今回のフレームでの回転量
        var maxDelta = _attackRotateSpeed * Time.deltaTime;
        // 敵に向かって、計算した回転量分だけ回転させる
        eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, targetAngleY, maxDelta);
        transform.eulerAngles = eulerAngles;
    }

    /// <summary>攻撃時の前進の更新処理</summary>
    private void UpdateAttackAdvance() {
        // 残りの移動距離を計算
        var remainAdvanceDistance = _attackAdvanceDistance - _attackAdvanceCount;

        // 攻撃目標の敵がいる場合
        if (_attackTarget != null) {
            // 攻撃の前進によって、敵を通り過ぎてしまわないように
            // 残りの移動距離より敵との距離の方が近ければ、敵との距離を残りの移動距離として扱う
            var distance = Vector3.Distance(transform.position, _attackTarget.transform.position);
            // 敵との距離が1m未満なら移動を止めるために、マイナス1する
            distance -= 1f;
            remainAdvanceDistance = Mathf.Min(remainAdvanceDistance, distance);
        }

        // 前進速度の計算
        // 残りの移動距離が多いほど速度が速くなるよう計算している
        // その結果、前進始めほど早く、後半が遅くなるイージングの動きになる
        var advanceSpeed = _attackAdvanceMaxSpeed * Mathf.InverseLerp(0f, _attackAdvanceDistance, remainAdvanceDistance);
        // 今回のフレームでの前進距離
        var advanceDistance = advanceSpeed * Time.deltaTime;
        // 前方に向かって前進距離分、座標を更新
        transform.position += transform.forward * advanceDistance;
        
        // 攻撃中に前進した距離をカウント
        _attackAdvanceCount += advanceDistance;
    }

    /// <summary>
    /// 攻撃のインパクトのタイミングの処理
    /// 攻撃モーションの設定したAnimationEventから呼び出される
    /// </summary>
    private void AttackImpactEvent() {
        // 攻撃の威力を設定する
        _attacker.SetData(10);
        // 攻撃判定を有効にする
        _attacker.Collider.enabled = true;
        // 0.1秒後に攻撃判定を無効にする処理を呼び出す
        Invoke(nameof(DisableAttackCollider), 0.1f);
    }

    /// <summary>攻撃判定を無効化</summary>
    private void DisableAttackCollider() {
        _attacker.Collider.enabled = false;
        // 攻撃中フラグを降ろす
        _attackFlag = false;
    }

    /// <summary>TriggerのColliderとの接触処理</summary>
    /// <param name="other">接触したコライダー</param>
    private void OnTriggerEnter(Collider other) {
        // 接触したコライダーがEnemyAttackタグならダメージ処理
        if (other.CompareTag("EnemyAttack")) {
            // 攻撃情報を持つAttackerコンポーネントを取得する
            var attacker = other.GetComponent<Attacker>();
            Damage(attacker.Power, other);
        }
    }
    
    /// <summary>ダメージ</summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="attackCollider">攻撃コライダー</param>
    private void Damage(int damage, Collider attackCollider) {
        if (IsDead) {
            // HPが既に0なら処理を抜ける
            return;
        }

        // 攻撃中フラグを降ろす
        _attackFlag = false;

        // ダメージ中フラグを立てる
        _damageFlag = true;

        // ダメージ量分、HPを減らす
        _hp = Mathf.Max(_hp - damage, 0);
        // HPゲージにHP量を反映
        GameUIManager.Instance.PlayerHPGauge.SetHP(_hp);

        // ダメージ表示を再生
        var hitPos = _hitCollider.ClosestPointOnBounds(attackCollider.transform.position);
        DamageViewManager.Instance.Play(damage, hitPos);
        // ヒットエフェクトを再生
        EffectManager.Instance.PlayAttackHit(hitPos + Vector3.up / 2f);

        // ダメージによる点滅表現
        BlinkColor(new Color(1f, 0.4f, 0.4f));

        // HPが0になったの場合、死亡処理に分岐
        if (_hp <= 0) {
            Dead();
            return;
        }

        // ダメージアニメーションのトリガーを起動
        _animator.SetTrigger("Damage");

        // 0.5秒後にダメージ終了処理を呼び出す
        Invoke(nameof(EndDamage), 0.5f);
    }

    /// <summary>色点滅の演出を再生</summary>
    /// <param name="color">点滅の色</param>
    private void BlinkColor(Color color) {
        // レンダラーからマテリアルを取得する
        // TIPS: Rendererのmaterialにアクセスすると、そのタイミングでアタッチされているmaterialが複製され
        //       Rendererに対してユニークなインスタンスとして扱えます
        var material = _bodyRenderer.material;

        // 前回の_blinkColorSeqがまだ再生中だった場合を考慮して、演出の強制終了メソッドを呼び出し
        _blinkColorSeq?.Kill();
        
        // 0.1秒で引数の色に変化させ、その後0.15秒で元の色に戻す演出を作成・再生
        _blinkColorSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(DOTween.To(() => Color.white, c => material.SetColor("_Color", c), color, 0.1f))
            .Append(DOTween.To(() => color, c => material.SetColor("_Color", c), Color.white, 0.15f));
    }

    /// <summary>ダメージ終了</summary>
    private void EndDamage() {
        // ダメージ中フラグを降ろす
        _damageFlag = false;
    }

    /// <summary>死亡</summary>
    private void Dead() {
        // 死亡アニメーションのトリガーを起動
        _animator.SetTrigger("Dead");
        // 2秒後に死亡終了処理を呼び出す
        Invoke(nameof(EndDead), 2f);
    }

    /// <summary>死亡終了</summary>
    private void EndDead() {
        GameUIManager.Instance.GameEndView.Play(false);
    }
}
