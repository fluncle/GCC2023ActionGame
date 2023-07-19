using System;
using UnityEngine;

/// <summary>ダメージ状態</summary>
public class EnemyStateDamage : EnemyStateBase {
    /// <summary>ダメージ量</summary>
    private int _damage;
    
    /// <summary>攻撃コライダー</summary>
    private Collider _attackCollider;

    /// <param name="damage">ダメージ量</param>
    /// <param name="attackCollider">攻撃コライダー</param>
    public EnemyStateDamage(Enemy enemy, int damage, Collider attackCollider) : base(enemy) {
        _damage = damage;
        _attackCollider = attackCollider;
    }

    /// <summary>このStateに遷移したときに最初に呼び出す処理</summary>
    protected override void Enter() {
        base.Enter();
        
        // 攻撃アニメーションのトリガーをリセットする
        _enemy.Animator.ResetTrigger("Attack");

        // 攻撃コライダーの方を向く
        _enemy.transform.LookAt(_attackCollider.transform.position);

        // ダメージ量分、HPを減らす
        _enemy.ReduceHP(_damage);

        // ダメージ表示を再生
        var hitPos = _enemy.HitCollider.ClosestPointOnBounds(_attackCollider.transform.position);
        DamageViewManager.Instance.Play(_damage, hitPos);
        // ヒットエフェクトを再生
        EffectManager.Instance.PlayAttackHit(hitPos);

        // ダメージによる点滅表現
        _enemy.BlinkColor(new Color(1f, 0.4f, 0.4f));

        // ダメージによる振動表現
        _enemy.ShakeBody();

        // HPが0になった場合、死亡処理に分岐
        if (_enemy.IsDead) {
            _enemy.KnockBack();
            _enemy.Transition(new EnemyStateDead(_enemy));
            return;
        }

        // ダメージアニメーションのトリガーを起動
        _enemy.Animator.SetTrigger("Damage");

        // ノックバック後に待機状態へ遷移
        Action onComplete = () => _enemy.Transition(new EnemyStatePatrol(_enemy));
        _enemy.KnockBack(onComplete);
    }
}
