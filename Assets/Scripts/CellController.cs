using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CircleCollider2D))]

public class CellController : MonoBehaviour
{
    [SerializeField] private Transform _arrowContainer;
    [SerializeField] private Image _icon;

    [SerializeField] private List<Sprite> _commonList = new List<Sprite>();
    [SerializeField] private List<Sprite> _premiumList = new List<Sprite>();

    public int X { get; set; }
    public int Type { get; set; }
    public bool Bonus { get; set; }

    private CircleCollider2D _collider;
    private Coroutine _zoom;
    private bool _spawned;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
    }

    public void Initialize(int x, int type, bool bonus)
    {
        _arrowContainer.gameObject.SetActive(false);
        
        _icon.gameObject.SetActive(false);
        _icon.sprite = bonus ? _premiumList[type] : _commonList[type];

        X = x;
        Type = type;
        Bonus = bonus;

        GameAction.LoadCell?.Invoke(this, true);
    }

    public void PlaceAt(Vector3 localPosition)
    {
        if (!_spawned)
        {
            transform.localPosition = localPosition;

            _zoom = StartCoroutine(ZoomIn());
        }
        else if (localPosition != transform.localPosition)
        {
            StartCoroutine(SlideTo(localPosition));
        }
    }

    public void Pop()
    {
        if (_zoom != null)
        {
            StopCoroutine(_zoom);
        }

        _zoom = StartCoroutine(ZoomOut());
    }

    private IEnumerator ZoomIn()
    {
        _icon.transform.localScale = Vector3.zero;
        _icon.gameObject.SetActive(true);

        var start = DateTime.Now;
        var progress = 0f;

        while (progress < 1f)
        {
            var duration = DateTime.Now - start;
            progress = (float)duration.TotalSeconds / GameManager.durationAnimation;
            var value = Mathf.SmoothStep(0f, 1f, progress);

            _icon.transform.localScale = new Vector3(value, value, value);

            yield return new WaitForEndOfFrame();
        }

        _icon.transform.localScale = Vector3.one;

        _spawned = true;
        _zoom = null;
    }

    private IEnumerator ZoomOut()
    {
        _icon.transform.localScale = Vector3.one ;
        _icon.gameObject.SetActive(true);

        var start = DateTime.Now;
        var progress = 0f;

        while (progress < 1f)
        {
            var duration = DateTime.Now - start;
            progress = (float) duration.TotalSeconds / (GameManager.durationAnimation * .5f);
            var value = Mathf.SmoothStep(1f, 0f, progress);

            _icon.transform.localScale = new Vector3(value, value, value);

            yield return new WaitForEndOfFrame();
        }

        _icon.transform.localScale = Vector3.zero;

        Destroy(this.gameObject);
    }

    private IEnumerator SlideTo(Vector3 localPositionEnd)
    {
        var localPositionStart = transform.localPosition;
        var localPositionDifference = localPositionEnd - localPositionStart;

        var start = DateTime.Now;
        var progress = 0f;

        while (progress < 1f)
        {
            var duration = DateTime.Now - start;
            progress = (float)duration.TotalSeconds / (GameManager.durationAnimation * .5f);
            var value = Mathf.SmoothStep(0f, 1f, progress);

            transform.localPosition = localPositionStart + (localPositionDifference * value);

            yield return new WaitForEndOfFrame();
        }

        transform.localPosition = localPositionEnd;
    }

    public bool IsOverlapping(Vector2 point)
    {
        return _collider.OverlapPoint(point);
    }

    public bool IsNear(CellController other)
    {
        return (Vector2.Distance(transform.position, other.transform.position) < _collider.radius * 3f);
    }

    public void SetArrowTo(CellController other)
    {
        if (other == null)
        {
            _arrowContainer.gameObject.SetActive(false);
            return;
        }

        other.transform.SetAsFirstSibling();

        _arrowContainer.gameObject.SetActive(true);

        var angle = Vector3.SignedAngle(other.transform.position - this.transform.position, Vector3.up, Vector3.back);
        _arrowContainer.rotation = Quaternion.Euler(0f,0f,angle);
    }

    private void OnValidate()
    {
        Debug.Assert(_arrowContainer != null);
        Debug.Assert(_icon != null);
    }
}
