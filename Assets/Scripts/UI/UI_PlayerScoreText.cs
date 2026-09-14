using TMPro;
using UnityEngine;

public class UI_PlayerScoreText : MonoBehaviour
{
    private PlayerController _pc;
    private TMP_Text _scoreText;
    public int Score { get; private set; }
       
    public void Init(PlayerController pc)
    {
        Debug.Assert(pc != null);
        _pc = pc;
        _pc.AddScoreEventHandler.AddListener(OnPlayerGetScored);
    }

    private void OnPlayerGetScored()
    {
        EnsureComponent();
        ++Score;
        _scoreText.text = $"{Score}";
    }
    private void Awake()
    {
        EnsureComponent();
    }

    private void OnDisable()
    {
        if (_pc != null)
        {
            _pc.AddScoreEventHandler.RemoveListener(OnPlayerGetScored);
        }
    }

    private void EnsureComponent()
    {
        if (_scoreText == null)
        {
            _scoreText = GetComponent<TMP_Text>();
        }
    }
}
