using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// バーチャルスティックの入力管理クラス
/// 入力ベクトルや入力中か否かを取得したり、
/// 入力開始時や終了時のイベントを登録したりする
/// </summary>
public class InputVirtualJoyStick : MonoBehaviour {
    [SerializeField] private InputAction _inputAction;
    
    /// <summary>入力開始時のイベント</summary>
    public Action OnStartEvent;

    /// <summary>入力値変化時のイベント</summary>
    public Action<Vector2> OnPerformEvent;
    
    /// <summary>入力終了時のイベント</summary>
    public Action OnCancelEvent;

    /// <summary>スティック操作中か否か</summary>
    public bool IsInput { get; private set; }
    
    /// <summary>入力ベクトル</summary>
    public Vector2 Vector { get; private set; }

    /// <summary>起動時の処理</summary>
    private void Awake() {
        // 各種コールバックにメソッドを登録する
        _inputAction.started += OnStart;
        _inputAction.performed += OnPerform;
        _inputAction.canceled += OnCancel;
    }

    /// <summary>入力開始時の処理</summary>
    private void OnStart(InputAction.CallbackContext c) {
        // 入力中フラグを立てる
        IsInput = true;
        OnStartEvent?.Invoke();
    }

    /// <summary>入力値変化時の処理</summary>
    private void OnPerform(InputAction.CallbackContext c) {
        Vector = _inputAction.ReadValue<Vector2>();
        OnPerformEvent?.Invoke(Vector);
    }

    /// <summary>入力終了時の処理</summary>
    private void OnCancel(InputAction.CallbackContext c) {
        // 入力中フラグを降ろす
        IsInput = false;
        OnCancelEvent?.Invoke();
    }

    // 各種コールバックのタイミングで必要なinputActionの処理を呼び出し
    private void OnEnable() => _inputAction.Enable();
    private void OnDisable() => _inputAction.Disable();
    private void OnDestroy() => _inputAction.Dispose();
}
