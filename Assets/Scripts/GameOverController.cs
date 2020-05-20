using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class GameOverController : MonoBehaviour
{
    private Text _text;

    private Color _original;

    private void OnEnable()
    {
        GameAction.SetGameState += OnSetGameStateChange;
    }

    private void OnDisable()
    {
        GameAction.SetGameState -= OnSetGameStateChange;
    }

    private void Awake()
    {
        _text = GetComponent<Text>();
        _original = _text.color;
    }

    private void Start()
    {
        _text.color = new Color(_original.r, _original.g, _original.b, 0f);
    }

    private void OnSetGameStateChange(GameAction.GameState gameState)
    {
        if (gameState == GameAction.GameState.GameStart)
        {
            _text.color = new Color(_original.r, _original.g, _original.b, 0f);
        }
        if (gameState == GameAction.GameState.GameEnd)
        {
            StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeIn()
    {
        var start = DateTime.Now;
        var progress = 0f;

        while (progress < 1f)
        {
            var duration = DateTime.Now - start;
            progress = (float)duration.TotalSeconds / (GameManager.durationAnimation * 3f);
            var value = Mathf.SmoothStep(0f, 1f, progress);

            _text.color = new Color(_original.r, _original.g, _original.b, _original.a * value);

            yield return new WaitForEndOfFrame();
        }

        _text.color = _original;
    }
}
