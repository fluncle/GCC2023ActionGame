using UnityEngine;
using DG.Tweening;

public class CameraManager : MonoBehaviour {
    // staticメンバにインスタンスを代入して各所からアクセスできるようにする
    public static CameraManager Instance { get; private set; }

    /// <summary>注視対象</summary>
    [SerializeField] private Transform _target;
    
    /// <summary>カメラと注視対象の相対的な座標</summary>
    private Vector3 _offsetPosition;

    /// <summary>注視対象を追いかける最大速度(m/s)</summary>
    [SerializeField] private float _maxSpeed = 10f;

    /// <summary>注視対象を追いかける速度が最大になる距離</summary>
    [SerializeField] private float _maxSpeedDistance = 2f;

    private Vector3 _shakeOffset;

    private Sequence _shakeSeq;

    /// <summary>起動時の処理</summary>
    private void Awake() {
        Instance = this;

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

        // 振動によるオフセットを反映
        transform.position += _shakeOffset;
    }

    /// <summary>画面の振動演出</summary>
    public void Shake() {
        // 前回の_shakeSeqがまだ再生中だった場合を考慮して、演出の強制終了メソッドを呼び出し
        _shakeSeq?.Kill();
        
        // 0.3秒間、ランダムな方向に0.02mの幅で30回振動する演出を作成・再生
        _shakeSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(DOTween.Shake(() => Vector3.zero, offset => _shakeOffset = offset, 0.3f, 0.02f, 30));
    }
}
