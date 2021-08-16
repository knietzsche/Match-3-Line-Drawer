using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static GameSettings;

public class GameManager : MonoBehaviour
{
    private Vector2 _originOffset;

    [SerializeField] private Transform _gameBoard;
    [SerializeField] private GameObject _cellPrefab;

    private List<CellController>[] _columns = new List<CellController>[CellCountX];
    private List<CellController> _toPopList = new List<CellController>();

    private int _turnRemaining;
    private int _scoreTotal;

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

    private void OnSetGameState(GameAction.GameState gameState)
    {
        if (gameState == GameAction.GameState.GameStart)
        {
            _scoreTotal = 0;
            _turnRemaining = turnMax;

            GameAction.SetGameState?.Invoke(GameAction.GameState.UpdateData);
        }
        else if (gameState == GameAction.GameState.TurnStart)
        {
            foreach (var column in _columns)
            {
                for (var indexY = 0; indexY < column.Count; indexY++)
                {
                    column[indexY].PlaceAt(GetLocalPosition(column[indexY], indexY));
                }
            }
        }
        else if (gameState == GameAction.GameState.UpdateData)
        {
            GameAction.UpdateData(dataKeyScore, _scoreTotal);
            GameAction.UpdateData(dataKeyTurn, _turnRemaining);

            Populate();

            if (_turnRemaining > 0)
            {
                GameAction.SetGameState?.Invoke(GameAction.GameState.TurnStart);
            }
            else
            {
                GameAction.SetGameState?.Invoke(GameAction.GameState.GameEnd);
            }
        }
        else if (gameState == GameAction.GameState.TurnEnd)
        {
            var scoreTurn = 0;

            foreach (var cell in _toPopList)
            {
                _columns[cell.X].Remove(cell);

                scoreTurn += scoreCell;
                if (cell.Bonus)
                {
                    scoreTurn *= scoreBonusMultiplier;
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

            StartCoroutine(SetGameStateWait(GameAction.GameState.UpdateData));
        }
        else if (gameState == GameAction.GameState.GameEnd)
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

    private void OnLoadCell(CellController cell, bool value)
    {
        if (value)
        {
            _columns[cell.X].Add(cell);
        }
        else
        {
            _toPopList.Add(cell);
        }
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

                // I want to have controll from the game manager over the cell type? what for?
                // also setting up the X here ??
                // what if none of this had to be set up?

                // what if some kind of dealer set up these things??
                // why would then be the reason to sepparate setting of these properties and their initialization

                var type = Random.Range(0, CellTypeCount);
                var bonus = Random.Range(0, 4) == 0;

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

    private static IEnumerator SetGameStateWait(GameAction.GameState gameState)
    {
        yield return new WaitForSeconds(durationAnimation);

        GameAction.SetGameState?.Invoke(gameState);
    }

    private void OnValidate()
    {
        Debug.Assert(_gameBoard != null, $"{this} is missing reference");
        Debug.Assert(_cellPrefab != null, $"{this} is missing reference");
    }
}
