using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StartButtonController : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();

        _button.onClick.AddListener(Listener);
    }

    private void OnEnable()
    {
        GameAction.SetGameState += OnSetGameState;
    }

    private void OnDisable()
    {
        GameAction.SetGameState -= OnSetGameState;
    }

    private void OnSetGameState(GameAction.GameState gameState)
    {
        if (gameState == GameAction.GameState.GameStart)
        {
            _button.interactable = false;
        }
        else if (gameState == GameAction.GameState.GameEnd)
        {
            _button.interactable = true;
        }
    }

    private void Listener()
    {
        GameAction.SetGameState?.Invoke(GameAction.GameState.GameStart);
    }
}
