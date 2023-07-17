using UnityEngine;

public class CameraManager : MonoBehaviour {
    /// <summary>注視対象</summary>
    [SerializeField] private Transform _target;
    
    /// <summary>カメラと注視対象の相対的な座標</summary>
    private Vector3 _offsetPosition;

    /// <summary>注視対象を追いかける最大速度(m/s)</summary>
    [SerializeField] private float _maxSpeed = 10f;

    /// <summary>注視対象を追いかける速度が最大になる距離</summary>
    [SerializeField] private float _maxSpeedDistance = 2f;

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

        var currentPos = transform.position;
        // 移動目標座標
        var targetPos = _target.position + _offsetPosition;
        // カメラ座標と目標座標の距離
        var distance = Vector3.Distance(currentPos, targetPos);
        // 今回のフレームでのカメラの移動量
        var maxDistanceDelta = _maxSpeed * Mathf.InverseLerp(0f, _maxSpeedDistance, distance) * Time.deltaTime;
        // カメラを目標座標に向かって、計算した移動量分だけ移動させる
        transform.position = Vector3.MoveTowards(currentPos, targetPos, maxDistanceDelta);
    }
}
