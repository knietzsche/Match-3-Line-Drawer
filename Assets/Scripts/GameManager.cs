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

    private Vector2 _originOffset;

    [SerializeField] private Transform _gameBoard;
    [SerializeField] private GameObject _cellPrefab;

    private List<CellController> _toPopList = new List<CellController>();

    private List<CellController>[] _columns = new List<CellController>[CellCountX];

    private int _scoreRound;
    private int _scoreTotal;

    private void OnEnable()
    {
        GameAction.Add += OnAdd;
        GameAction.Pop += OnPop;
        GameAction.TurnStart += OnTurnStart;
        GameAction.TurnEnd += OnTurnEnd;
        GameAction.Upkeep += OnUpkeep;
    }

    private void OnDisable()
    {
        GameAction.Add -= OnAdd;
        GameAction.Pop -= OnPop;
        GameAction.TurnStart -= OnTurnStart;
        GameAction.TurnEnd -= OnTurnEnd;
        GameAction.Upkeep -= OnUpkeep;
    }

    void Start()
    {
        _originOffset = new Vector2(
            (-1) * (CellCountX - 1) * CellSizeX / 2f,
            ((-1) * (CellCountY - 1) * CellSizeY / 2f) - (CellSizeY / 4f));

        for (var i = 0; i < _columns.Length; i++)
        {
            _columns[i] = new List<CellController>();
        }

        GameAction.Upkeep?.Invoke();
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

        StartCoroutine(InvokeWait(GameAction.TurnStart));
    }

    private void OnTurnEnd()
    {
        foreach (var cell in _toPopList)
        {
            _columns[cell.X].Remove(cell);
            
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
