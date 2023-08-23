using System;
using UnityEngine;

/// <summary>攻撃情報の受け渡しをするコンポーネント</summary>
public class Attacker : MonoBehaviour {
    /// <summary>攻撃判定のコライダー</summary>
    public Collider Collider => _collider;
    [SerializeField] private Collider _collider;
    
    /// <summary>接触イベント</summary>
    public Action<Collider> OnTriggerEnterEvent;

    /// <summary>威力</summary>
    public int Power { get; private set; }

    /// <summary>攻撃情報を設定</summary>
    /// <param name="power">威力</param>
    public void SetData(int power) {
        Power = power;
    }

    /// <summary>TriggerのColliderとの接触処理</summary>
    /// <param name="other">接触したコライダー</param>
    private void OnTriggerEnter(Collider other) {
        OnTriggerEnterEvent?.Invoke(other);
    }
}
