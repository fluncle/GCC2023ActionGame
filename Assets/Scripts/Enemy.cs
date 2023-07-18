using UnityEngine;

public class Enemy : MonoBehaviour {
    /// <summary>最大HP</summary>
    private const int MAX_HP = 30;

    [SerializeField] private Animator _animator;

    /// <summary>移動最高速度(m/s)</summary>
    [SerializeField] private float _maxSpeed;

    /// <summary>移動最高速度(m/s)</summary>
    [SerializeField] private float _attackRange;

    /// <summary>攻撃中フラグ</summary>
    private bool _attackFlag;
    
    /// <summary>攻撃判定のコライダー</summary>
    [SerializeField] private Attacker _attacker;

    private int _hp;

    /// <summary>ダメージ中フラグ</summary>
    private bool _damageFlag;

    /// <summary>死亡しているか否か</summary>
    public bool IsDead => _hp <= 0;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        _animator.SetBool("IsMove", true);
        _hp = MAX_HP;
    }

    /// <summary>更新処理</summary>
    private void Update() {
        var player = GameManager.Instance.Player;
        if (_attackFlag || _damageFlag || _hp <= 0 ||  player.IsDead) {
            // 攻撃中、ダメージ中、死亡時、またはプレイヤー死亡時は何もしない
            return;
        }

        // プレイヤーの方向を向く
        var playerPos = player.transform.position;
        transform.LookAt(playerPos);

        // プレイヤーが攻撃範囲にいたら攻撃を実行
        var currentPos = transform.position;
        var distance = Vector3.Distance(currentPos, playerPos);
        if (distance <= _attackRange) {
            Attack();
            // 攻撃時は移動処理をせず処理を抜ける
            return;
        }

        // プレイヤーに向かって移動
        var maxDistanceDelta = _maxSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(currentPos, playerPos, maxDistanceDelta);
    }

    /// <summary>攻撃</summary>
    private void Attack() {
        // 攻撃中フラグを立てる
        _attackFlag = true;
        // 攻撃アニメーションのトリガーを起動
        _animator.SetTrigger("Attack");
        // 移動アニメーションのフラグを降ろす
        _animator.SetBool("IsMove", false);
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
        // 1秒後に攻撃終了処理を呼び出す
        Invoke(nameof(EndAttack), 1f);
    }

    /// <summary>攻撃終了</summary>
    private void EndAttack() {
        // 攻撃中フラグを降ろす
        _attackFlag = false;
        // 移動アニメーションのフラグを立てる
        _animator.SetBool("IsMove", true);
    }

    /// <summary>TriggerのColliderとの接触処理</summary>
    /// <param name="other">接触したコライダー</param>
    private void OnTriggerEnter(Collider other) {
        // 接触したコライダーがPlayerAttackタグならダメージ処理
        if (other.CompareTag("PlayerAttack")) {
            // 攻撃情報を持つAttackerコンポーネントを取得する
            var attacker = other.GetComponent<Attacker>();
            Damage(attacker.Power, other);
        }
    }

    /// <summary>ダメージ</summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="attackCollider">攻撃コライダー</param>
    private void Damage(int damage, Collider attackCollider) {
        if (_hp <= 0) {
            // HPが既に0なら処理を抜ける
            return;
        }

        // 攻撃中フラグを降ろす
        _attackFlag = false;
        // 攻撃アニメーションのトリガーをリセットする
        _animator.ResetTrigger("Attack");

        // ダメージ中フラグを立てる
        _damageFlag = true;
        // 攻撃コライダーの方を向く
        transform.LookAt(attackCollider.transform.position);

        // ダメージ量分、HPを減らす
        _hp = Mathf.Max(_hp - damage, 0);

        // HPが0になったの場合、死亡処理に分岐
        if (_hp <= 0) {
            Dead();
            return;
        }

        // ダメージアニメーションのトリガーを起動
        _animator.SetTrigger("Damage");

        // 1秒後にダメージ終了処理を呼び出す
        Invoke(nameof(EndDamage), 1f);
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
        // オブジェクトを削除
        EnemyManager.Instance.Remove(this);
    }

    /// <summary>オブジェクト削除時の処理</summary>
    private void OnDestroy() {
        // 登録したInvokeをすべてキャンセル
        CancelInvoke();
    }
}
