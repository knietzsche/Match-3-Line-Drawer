using System;

public static class GameAction
{
    public static Action<CellController> Add;
    public static Action<CellController> Pop;

    public static Action TurnStart;
    public static Action TurnEnd;
    public static Action Upkeep;
}
