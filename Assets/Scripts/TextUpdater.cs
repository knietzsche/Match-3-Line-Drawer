using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextUpdater : MonoBehaviour
{
    [SerializeField] private string dataName;

    private Text _text;

    private void OnEnable()
    {
        GameAction.UpdateData += OnUpdateData;
    }

    private void OnDisable()
    {
        GameAction.UpdateData -= OnUpdateData;
    }

    private void Awake()
    {
        _text = GetComponent<Text>();
    }

    private void OnUpdateData(string name, int value)
    {
        if (name == dataName)
        {
            _text.text = value.ToString();
        }
    }

}
