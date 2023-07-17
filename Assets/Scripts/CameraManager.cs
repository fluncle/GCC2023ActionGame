using UnityEngine;

public class CameraManager : MonoBehaviour {
    /// <summary>注視対象</summary>
    [SerializeField] private Transform _target;
    
    /// <summary>カメラと注視対象の相対的な座標</summary>
    private Vector3 _offsetPosition;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        if (_target == null) {
            return;
        }

        // ゲーム開始時のカメラと注視対象の相対的な座標を計算
        _offsetPosition = transform.position - _target.position;
    }

    /// <summary>更新処理</summary>
    private void Update() {
        if (_target == null) {
            return;
        }

        // ゲーム開始時の位置関係を維持してカメラを追従させる
        transform.position = _target.position + _offsetPosition;
    }
}
