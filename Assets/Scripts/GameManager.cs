using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const int CellCountX = 7;
    public const int CellCountY = 6;
    public const float CellSizeX = 162f;
    public const float CellSizeY = 186f;
    public const int CellTypeCount = 4;

    public const float durationAnimation = .8f;

    public const string dataKeyScore = "score";
    public const string dataKeyTurn = "round";

    public const int turnMax = 20;
    public const int scoreCell = 100;
    public const int scoreBonusMultiplier = 2;

    private Vector2 _originOffset;

    [SerializeField] private Transform _gameBoard;
    [SerializeField] private GameObject _cellPrefab;

    private List<CellController>[] _columns = new List<CellController>[CellCountX];
    private List<CellController> _toPopList = new List<CellController>();

    private int _turnRemaining;
    private int _scoreTotal;

    private void OnEnable()
    {
        GameAction.SetGame += OnSetGame;
        GameAction.Add += OnAdd;
        GameAction.Pop += OnPop;
        GameAction.TurnStart += OnTurnStart;
        GameAction.TurnEnd += OnTurnEnd;
        GameAction.Upkeep += OnUpkeep;
    }

    private void OnDisable()
    {
        GameAction.SetGame -= OnSetGame;
        GameAction.Add -= OnAdd;
        GameAction.Pop -= OnPop;
        GameAction.TurnStart -= OnTurnStart;
        GameAction.TurnEnd -= OnTurnEnd;
        GameAction.Upkeep -= OnUpkeep;
    }

    void Awake()
    {
        _originOffset = new Vector2(
            (-1) * (CellCountX - 1) * CellSizeX / 2f,
            ((-1) * (CellCountY - 1) * CellSizeY / 2f) - (CellSizeY / 4f));

        for (var i = 0; i < _columns.Length; i++)
        {
            _columns[i] = new List<CellController>();
        }
    }

    private void OnSetGame(bool value)
    {
        if (value)
        {
            _scoreTotal = 0;
            _turnRemaining = turnMax;

            GameAction.Upkeep?.Invoke();
        }
        else
        {
            foreach (var column in _columns)
            {
                foreach (var cell in column)
                {
                    cell.Pop();
                }

                column.Clear();
            }
        }
    }

    private void OnAdd(CellController cell)
    {
        _columns[cell.X].Add(cell);
    }

    private void OnPop(CellController cell)
    {
        _toPopList.Add(cell);
    }

    private void OnTurnStart()
    {
        foreach (var column in _columns)
        {
            for (var indexY = 0; indexY < column.Count; indexY++)
            {
                column[indexY].PlaceAt(GetLocalPosition(column[indexY], indexY));
            }
        }
    }

    private void OnUpkeep()
    {
        Populate();

        GameAction.UpdateData(dataKeyScore, _scoreTotal);
        GameAction.UpdateData(dataKeyTurn, _turnRemaining);

        if (_turnRemaining > 0)
        {
            GameAction.TurnStart?.Invoke();
        }
        else
        {
            GameAction.SetGame?.Invoke(false);
        }
    }

    private void OnTurnEnd()
    {
        var scoreTurn = 0;

        foreach (var cell in _toPopList)
        {
            _columns[cell.X].Remove(cell);

            scoreTurn += scoreCell;
            if (cell.Bonus)
            {
                scoreTurn *=  scoreBonusMultiplier;
            }

            cell.Pop();
        }

        foreach (var column in _columns)
        {
            for (var indexY = 0; indexY < column.Count; indexY++)
            {
                column[indexY].PlaceAt(GetLocalPosition(column[indexY], indexY));
            }
        }

        _toPopList.Clear();

        _scoreTotal += scoreTurn;
        _turnRemaining -= 1;

        StartCoroutine(InvokeWait(GameAction.Upkeep));
    }

    private void Populate()
    {
        for (var iX = 0; iX < CellCountX; iX++)
        {
            for (var iY = 0; iY < CellCountY; iY++)
            {
                if (_columns[iX].Count > iY)
                {
                    continue;
                }

                var instance = Instantiate(_cellPrefab, _gameBoard);
                
                var controller = instance.GetComponent<CellController>();

                var type = UnityEngine.Random.Range(0, GameManager.CellTypeCount);
                var bonus = UnityEngine.Random.Range(0, 4) == 0;

                controller.Initialize(iX, type, bonus);
            }
        }
    }

    private Vector2 GetLocalPosition(CellController cell, int indexY)
    {
        var localPosition = new Vector2(
                    cell.X * CellSizeX,
                    indexY * CellSizeY + cell.X % 2f * (CellSizeY / 2f));
        localPosition += _originOffset;

        return localPosition;
    }

    private static IEnumerator InvokeWait(Action action)
    {
        yield return new WaitForSeconds(GameManager.durationAnimation);

        action?.Invoke();
    }

    private void OnValidate()
    {
        Debug.Assert(_gameBoard != null);
        Debug.Assert(_cellPrefab != null);
    }
}
