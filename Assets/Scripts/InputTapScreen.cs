using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>タップ入力管理クラス</summary>
public class InputTapScreen : MonoBehaviour {
    [SerializeField] private InputAction _inputAction;

    /// <summary>タップ入力成立時のイベント</summary>
    public Action OnPerformEvent;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        // コールバックにメソッドを登録する
        _inputAction.performed += OnPerform;
    }

    /// <summary>タップ入力成立時の処理</summary>
    private void OnPerform(InputAction.CallbackContext c) {
        OnPerformEvent?.Invoke();
    }

    // 各種コールバックのタイミングで必要なinputActionの処理を呼び出し
    private void OnEnable() => _inputAction.Enable();
    private void OnDisable() => _inputAction.Disable();
    private void OnDestroy() => _inputAction.Dispose();
}
