using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class InputManager : MonoBehaviour
{
    private HashSet<CellController> _cellSet = new HashSet<CellController>();

    private List<CellController> _selectedList = new List<CellController>();

    private bool _mouseDown;
    private bool _lockInput;

    private void OnEnable()
    {
        GameAction.Add += OnAdd;
        GameAction.Pop += OnPop;
        GameAction.TurnStart += OnTurnStart;
        GameAction.TurnEnd += OnTurnEnd;
    }

    private void OnDisable()
    {
        GameAction.Add -= OnAdd;
        GameAction.Pop -= OnPop;
        GameAction.TurnStart -= OnTurnStart;
        GameAction.TurnEnd -= OnTurnEnd;
    }

    private void OnAdd(CellController cell)
    {
        _cellSet.Add(cell);
    }

    private void OnPop(CellController cell)
    {
        _cellSet.Remove(cell);
    }

    private void OnTurnStart()
    {
        _lockInput = false;
    }

    private void OnTurnEnd()
    {
        _lockInput = true;
    }

    private void Update()
    {
        if (_lockInput)
        {
            return;
        }

        if (_mouseDown && _selectedList.Count > 0)
        {
            var last = _selectedList[_selectedList.Count - 1];

            if (!last.IsOverlapping(Input.mousePosition))
            {
                var cell = _cellSet.Where(x => x.IsOverlapping(Input.mousePosition)).FirstOrDefault();

                if (cell != null && last.IsNear(cell))
                {
                    var index = _selectedList.IndexOf(cell);
                    if (index == -1)
                    {
                        if (cell.Type == last.Type)
                        {
                            _selectedList.Add(cell);
                            last.SetArrowTo(cell);
                        }
                    }
                    else if (index == _selectedList.Count - 2)
                    {
                        _selectedList[index].SetArrowTo(null);

                        _selectedList.Remove(last);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var cells = _cellSet.ToArray();
            foreach(var cell in cells)
            {
                if (cell.IsOverlapping(Input.mousePosition))
                {
                    _selectedList.Add(cell);
                    Debug.Log("Cell added count == " + _selectedList.Count );
                    break;
                }
            }

            _mouseDown = true;
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            foreach (var cell in _selectedList)
            {
                cell.SetArrowTo(null);
            }

            if (_selectedList.Count > 2)
            {
                foreach (var cell in _selectedList)
                {
                    GameAction.Pop?.Invoke(cell);
                }

                GameAction.TurnEnd?.Invoke();
            }

            _selectedList.Clear();
            _mouseDown = false;
        }
    }
}
