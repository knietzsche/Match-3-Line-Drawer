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
        GameAction.SetGame += OnSetGame;
    }

    private void OnDisable()
    {
        GameAction.SetGame -= OnSetGame;
    }

    private void OnSetGame(bool value)
    {
        _button.interactable = !value;
    }

    private void Listener()
    {
        GameAction.SetGame?.Invoke(true);
    }
}
