using System;

public static class GameAction
{
    public enum GameState { GameStart, TurnStart, UpdateData, TurnEnd, GameEnd }
    public static Action<GameState> SetGameState;

    public static Action<CellController, bool> LoadCell;

    public static Action<string, int> UpdateData;
}
