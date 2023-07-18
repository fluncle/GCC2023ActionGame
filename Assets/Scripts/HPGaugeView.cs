using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HPGaugeView : MonoBehaviour {
    /// <summary>ゲージのバー表示のImage</summary>
    [SerializeField] private Image _bar;
    
    /// <summary>HP量を表示するテキスト</summary>
    [SerializeField] private Text _valueText;
    
    /// <summary>差分ゲージのバー表示のImage</summary>
    [SerializeField] private Image _diffBar;

    private Sequence _barSeq;

    /// <summary>最大HP</summary>
    private int _maxHP;
    
    /// <summary>初期化</summary>
    /// <param name="maxHP">最大HP</param>
    public void Initialize(int maxHP) {
        _maxHP = maxHP;
        SetHP(maxHP);
    }

    /// <summary>HPを指定して表示を更新</summary>
    public void SetHP(int hp) {
        var hpRate = (float)hp / _maxHP;
        // 残りHPの割合でバーの表示幅を更新
        _bar.fillAmount = hpRate;

        _barSeq?.Kill();
        _barSeq = DOTween.Sequence()
            .SetLink(gameObject)
            .SetDelay(0.75f)
            .Append(_diffBar.DOFillAmount(hpRate, 0.5f));

        // HP量のテキストを更新
        _valueText.text = string.Format("HP {0}/{1}", hp, _maxHP);
    }
}
