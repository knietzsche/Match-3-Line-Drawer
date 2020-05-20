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
        GameAction.SetGameState += OnSetGameState;
        GameAction.LoadCell += OnLoadCell;
    }

    private void OnDisable()
    {
        GameAction.SetGameState -= OnSetGameState;
        GameAction.LoadCell -= OnLoadCell;
    }

    private void OnSetGameState(GameAction.GameState gameState)
    {
        if (gameState == GameAction.GameState.TurnStart)
        {
            _lockInput = false;
        }
        else if (gameState == GameAction.GameState.TurnEnd)
        {
            _lockInput = true;
        }
        else if (gameState == GameAction.GameState.GameEnd)
        {
            _cellSet.Clear();
        }
    }

    private void OnLoadCell(CellController cell, bool value)
    {
        if (value)
        {
            _cellSet.Add(cell);
        }
        else
        {
            _cellSet.Remove(cell);
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (Application.isMobilePlatform)
            {
                Application.Unload();
            }
            else
            {
                Application.Quit();
            }
        }

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
                    GameAction.LoadCell?.Invoke(cell, false);
                }

                GameAction.SetGameState?.Invoke(GameAction.GameState.TurnEnd);
            }

            _selectedList.Clear();
            _mouseDown = false;
        }
    }
}
