using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using Sequence = DG.Tweening.Sequence;

public class Enemy : MonoBehaviour {
    /// <summary>最大HP</summary>
    private const int MAX_HP = 30;

    /// <summary>ヒットストップ時間</summary>
    private const float HIT_STOP_DURATION = 0.3f;

    [SerializeField] private Animator _animator;

    /// <summary>移動最高速度(m/s)</summary>
    [SerializeField] private float _maxSpeed;

    /// <summary>移動最高速度(m/s)</summary>
    [SerializeField] private float _attackRange;

    /// <summary>攻撃準備フラグ</summary>
    private bool _attackReadyFlag;

    /// <summary>攻撃中フラグ</summary>
    private bool _attackFlag;
    
    /// <summary>攻撃判定のコライダー</summary>
    [SerializeField] private Attacker _attacker;

    private int _hp;

    /// <summary>ダメージ中フラグ</summary>
    private bool _damageFlag;

    /// <summary>喰らい判定のコライダー</summary>
    [SerializeField] private Collider _hitCollider;

    /// <summary>色の点滅演出のシーケンス</summary>
    private Sequence _blinkColorSeq;

    /// <summary>3Dモデルのレンダラー</summary>
    [SerializeField] private Renderer _bodyRenderer;

    /// <summary>3Dモデルのマテリアル</summary>
    [SerializeField] private Material _bodyMaterial;

    /// <summary>振動演出のシーケンス</summary>
    private Sequence _shakeSeq;
    
    /// <summary>振動のオフセットベクトル</summary>
    private Vector3 _shakeOffset;

    /// <summary>振動演出のRoot</summary>
    [SerializeField] private Transform _shakeRoot;
    
    /// <summary>ノックバック演出のシーケンス</summary>
    private Sequence _knockBackSeq;

    /// <summary> 死亡時に再生するエフェクト </summary>
    [SerializeField] private ParticleSystem _deadEffect;
    
    /// <summary>死亡しているか否か</summary>
    public bool IsDead => _hp <= 0;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        _animator.SetBool("IsMove", true);
        _hp = MAX_HP;
        
        // レンダラーからマテリアルを取得する
        // TIPS: Rendererのmaterialにアクセスすると、そのタイミングでアタッチされているmaterialが複製され
        //       Rendererに対してユニークなインスタンスとして扱えます
        _bodyMaterial = _bodyRenderer.material;
    }

    /// <summary>更新処理</summary>
    private void Update() {
        var player = GameManager.Instance.Player;
        if (_attackReadyFlag || _attackFlag || _damageFlag || _hp <= 0 ||  player.IsDead) {
            // 攻撃準備中、攻撃中、ダメージ中、死亡時、またはプレイヤー死亡時は何もしない
            return;
        }

        // プレイヤーの方向を向く
        var playerPos = player.transform.position;
        transform.LookAt(playerPos);

        // プレイヤーが攻撃範囲にいたら攻撃を実行
        var currentPos = transform.position;
        var distance = Vector3.Distance(currentPos, playerPos);
        if (distance <= _attackRange) {
            AttackReady();
            // 攻撃時は移動処理をせず処理を抜ける
            return;
        }

        // プレイヤーに向かって移動
        var maxDistanceDelta = _maxSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(currentPos, playerPos, maxDistanceDelta);
    }

    /// <summary>攻撃準備</summary>
    private void AttackReady() {
        // 攻撃準備中フラグを立てる
        _attackReadyFlag = true;
        // 攻撃準備アニメーションのトリガーを起動
        _animator.SetTrigger("AttackReady");
        // 移動アニメーションのフラグを降ろす
        _animator.SetBool("IsMove", false);
        // 攻撃準備表現として、体を3回点滅させる
        FadeColor(Color.gray, 3);
        // 攻撃準備表現として、体を振動させる
        ShakeBody(2);
        // 0.1秒後に攻撃判定を無効にする処理を呼び出す
        Invoke(nameof(Attack), 0.3f);
    }

    /// <summary>攻撃</summary>
    private void Attack() {
        // 攻撃準備中フラグを降ろす
        _attackReadyFlag = false;
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
        _attacker.SetData(20);
        // 攻撃判定を有効にする
        _attacker.Collider.enabled = true;
        // 0.1秒後に攻撃判定を無効にする処理を呼び出す
        Invoke(nameof(DisableAttackCollider), 0.1f);
    }

    /// <summary>攻撃判定を無効化</summary>
    private void DisableAttackCollider() {
        _attacker.Collider.enabled = false;
        // 攻撃準備表現の点滅を停止する
        _blinkColorSeq?.Complete();
        // 攻撃準備表現の振動を停止する
        _shakeSeq?.Complete();
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

        if (_attackReadyFlag) {
            // 攻撃準備中フラグを降ろす
            _attackReadyFlag = false;
            // 攻撃準備アニメーションのトリガーをリセットする
            _animator.ResetTrigger("AttackReady");
            // 登録したAttackのInvokeをキャンセル
            CancelInvoke(nameof(Attack));
        }

        if (_attackFlag) {
            // 攻撃中フラグを降ろす
            _attackFlag = false;
            // 攻撃アニメーションのトリガーをリセットする
            _animator.ResetTrigger("Attack");
        }

        // ダメージ中フラグを立てる
        _damageFlag = true;
        // 攻撃コライダーの方を向く
        transform.LookAt(attackCollider.transform.position);

        // ダメージ量分、HPを減らす
        _hp = Mathf.Max(_hp - damage, 0);

        // ダメージ表示を再生
        var hitPos = _hitCollider.ClosestPointOnBounds(attackCollider.transform.position);
        DamageViewManager.Instance.Play(damage, hitPos);

        // ダメージによる点滅表現
        BlinkColor(new Color(1f, 0.4f, 0.4f));

        // ダメージによる振動表現
        ShakeBody();

        // HPが0になったの場合、死亡処理に分岐
        if (_hp <= 0) {
            BeginDead();
            // 致死ダメージヒットエフェクトを再生
            EffectManager.Instance.PlayAttackHitDead(hitPos);
            // 致死ダメージ時、プレイヤーのヒットストップ処理を呼ぶ
            GameManager.Instance.Player.PlayHitStop(HIT_STOP_DURATION);
            return;
        }

        // ヒットエフェクトを再生
        EffectManager.Instance.PlayAttackHit(hitPos);

        // ダメージアニメーションのトリガーを起動
        _animator.SetTrigger("Damage");

        // ノックバック後にダメージ終了処理を呼び出す
        KnockBack(EndDamage);
    }

    /// <summary>色点滅の演出を再生</summary>
    /// <param name="color">点滅の色</param>
    private void BlinkColor(Color color) {
        // 前回の_blinkColorSeqがまだ再生中だった場合を考慮して、演出の強制終了メソッドを呼び出し
        _blinkColorSeq?.Kill();
        
        // 0.1秒で引数の色に変化させ、その後0.15秒で元の色に戻す演出を作成・再生
        _blinkColorSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(DOTween.To(() => Color.black, SetColor, color, 0.1f))
            .Append(DOTween.To(() => color, SetColor, Color.black, 0.15f));
    }

    /// <summary>フェードによる色点滅の演出を再生</summary>
    /// <param name="color">点滅の色</param>
    /// <param name="loop">ループ回数</param>
    private void FadeColor(Color color, int loop) {
        // 前回の_blinkColorSeqがまだ再生中だった場合を考慮して、演出の強制終了メソッドを呼び出し
        _blinkColorSeq?.Kill();

        _blinkColorSeq = DOTween.Sequence()
            .SetLink(gameObject);

        for (int i = 0; i < loop; i++) {
            _blinkColorSeq
                .Append(DOTween.To(() => Color.black, SetColor, color, 0.15f))
                .Append(DOTween.To(() => color, SetColor, Color.black, 0.15f));
        }
    }

    /// <summary>色を設定</summary>
    private void SetColor(Color color) {
        _bodyMaterial.SetColor("_Color", color);
    }

    /// <summary>振動演出を再生</summary>
    private void ShakeBody() {
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

    /// <summary>ループする振動演出を再生</summary>
    private void ShakeBody(int loop) {
        // 前回の_shakeSeqがまだ再生中だった場合を考慮して、演出の強制終了メソッドを呼び出し
        _shakeSeq?.Kill();

        _shakeSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(DOTween.Shake(() => Vector3.zero, offset => _shakeOffset = offset, 0.5f, 0.15f, 20, fadeOut: false))
            .OnUpdate(() => _shakeRoot.localPosition += _shakeOffset)
            .SetUpdate(UpdateType.Late)
            .SetLoops(loop);
    }

    /// <summary>ノックバック演出</summary>
    /// <param name="onComplete">ノックバック終了時の処理</param>
    private void KnockBack(Action onComplete = null) {
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

    /// <summary>ダメージ終了</summary>
    private void EndDamage() {
        // ダメージ中フラグを降ろす
        _damageFlag = false;
    }

    /// <summary>死亡演出開始</summary>
    private void BeginDead() {
        // 死亡アニメーションのトリガーを起動
        _animator.SetTrigger("Dead");
        _animator.speed = 0f;

        // _blinkColorSeqがまだ再生中だった場合を考慮して、演出の強制終了メソッドを呼び出し
        _blinkColorSeq?.Kill();
        // 致死ダメージ時の色を設定
        SetColor(new Color(1f, 0.4f, 0.4f));

        // ヒットストップ時間経過後に死亡処理を呼び出す
        Invoke(nameof(Dead), HIT_STOP_DURATION);
    }

    /// <summary>死亡</summary>
    private void Dead() {
        // アニメーションを再開
        _animator.speed = 1f;
        // 死亡エフェクトを再生
        _deadEffect.Play();

        // _blinkColorSeqがまだ再生中だった場合を考慮して、演出の強制終了メソッドを呼び出し
        _blinkColorSeq?.Kill();
        // 死亡時の色を設定
        SetColor(new Color(0.3f, 0.1f, 1f));

        // フェードアウト演出のために影を落とさないようにする
        _bodyRenderer.shadowCastingMode = ShadowCastingMode.Off;

        // 1.5秒かけてディゾルブ表現によるフェードアウトを再生し、その後に死亡終了処理を呼び出す
        DOTween.Sequence()
            .SetLink(gameObject)
            .Append(DOTween.To(() => 0f, SetDisolveFade, 1f, 1.5f))
            .OnComplete(EndDead);
    }

    /// <summary>ディゾルブ表現によるフェードを設定</summary>
    private void SetDisolveFade(float threshold) {
        _bodyMaterial.SetFloat("_Threshold", threshold);
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
