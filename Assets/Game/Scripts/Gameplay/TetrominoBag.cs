using System;
using System.Collections.Generic;

public class TetrominoBag
{
    private readonly Random _random;
    private readonly List<TetrominoType> _bag = new List<TetrominoType>();

    public TetrominoBag(int seed = 0)
    {
        _random = seed != 0 ? new Random(seed) : new Random();
    }

    public TetrominoType Next()
    {
        if (_bag.Count == 0)
            Refill();
        int index = _random.Next(_bag.Count);
        var piece = _bag[index];
        _bag.RemoveAt(index);
        return piece;
    }

    private void Refill()
    {
        _bag.Add(TetrominoType.I);
        _bag.Add(TetrominoType.O);
        _bag.Add(TetrominoType.T);
        _bag.Add(TetrominoType.S);
        _bag.Add(TetrominoType.Z);
        _bag.Add(TetrominoType.J);
        _bag.Add(TetrominoType.L);
    }
}
