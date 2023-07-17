using UnityEngine;

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

    private int _hp;

    /// <summary>ダメージ中フラグ</summary>
    private bool _damageFlag;

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
            Damage(attacker.Power);
        }
    }
    
    /// <summary>ダメージ</summary>
    /// <param name="damage">ダメージ量</param>
    private void Damage(int damage) {
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
