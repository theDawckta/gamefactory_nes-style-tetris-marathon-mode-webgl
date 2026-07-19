using System;
using UnityEngine;

public enum TetrominoType { I, O, T, S, Z, J, L }

public static class TetrominoData
{
    private static readonly Vector2Int[][][] Cells = new Vector2Int[][][]
    {
        // I -- SRS-canonical: the two vertical states (rot1/rot3) sit in distinct
        // columns (rot1 at x=1, rot3 at x=0) so the SRS I wall-kick table applies.
        new[]
        {
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new[] { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(1, -2) },
            new[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1), new Vector2Int(2, -1) },
            new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(0, -2) },
        },
        // O
        new[]
        {
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(1, -1) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(1, -1) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(1, -1) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(1, -1) },
        },
        // T
        new[]
        {
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) },
            new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(1, 0) },
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, -1) },
            new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) },
        },
        // S
        new[]
        {
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1) },
            new[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(0, 0), new Vector2Int(1, 0) },
            new[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, -1) },
        },
        // Z
        new[]
        {
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1) },
            new[] { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(0, 0), new Vector2Int(0, -1) },
            new[] { new Vector2Int(0, -1), new Vector2Int(1, -1), new Vector2Int(-1, 0), new Vector2Int(0, 0) },
            new[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, -1) },
        },
        // J
        new[]
        {
            new[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) },
            new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 0), new Vector2Int(0, -1) },
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1) },
            new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(-1, -1) },
        },
        // L
        new[]
        {
            new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) },
            new[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1), new Vector2Int(1, -1) },
            new[] { new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) },
            new[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(0, -1) },
        },
    };

    private static readonly Color[] Colors = new Color[]
    {
        new Color(0f, 1f, 1f),         // I = cyan
        new Color(1f, 1f, 0f),         // O = yellow
        new Color(0.5f, 0f, 0.5f),     // T = purple
        new Color(0f, 0.5f, 0f),       // S = green
        new Color(1f, 0f, 0f),         // Z = red
        new Color(0f, 0f, 1f),         // J = blue
        new Color(1f, 0.5f, 0f),       // L = orange
    };

    public static Vector2Int[] GetCells(TetrominoType type, int rotation)
    {
        return Cells[(int)type][rotation & 3];
    }

    public static Color GetColor(TetrominoType type)
    {
        return Colors[(int)type];
    }

    // --- SRS wall kicks (https://tetris.wiki/Super_Rotation_System) ---------------
    // Offsets are in this project's Y-UP convention (positive Y = up), matching the
    // tetris.wiki tables directly. Each transition lists 5 candidate offsets tried in
    // order; the first is always (0,0) (rotate in place). Kicks are locked by tests
    // in TetrominoSystemTests / PlayfieldControllerTests -- if a Y sign is ever wrong,
    // the T-spin-into-a-notch test fails loudly.
    //
    // Transition index order (see KickIndex): 0:0->1 1:1->0 2:1->2 3:2->1
    //                                          4:2->3 5:3->2 6:3->0 7:0->3
    private static readonly Vector2Int[][] JlstzKicks = new[]
    {
        new[] { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, -2), new Vector2Int(-1, -2) }, // 0->1
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, 2), new Vector2Int(1, 2) },      // 1->0
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, 2), new Vector2Int(1, 2) },      // 1->2
        new[] { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, -2), new Vector2Int(-1, -2) }, // 2->1
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, -2), new Vector2Int(1, -2) },     // 2->3
        new[] { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },   // 3->2
        new[] { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },   // 3->0
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, -2), new Vector2Int(1, -2) },     // 0->3
    };

    private static readonly Vector2Int[][] IKicks = new[]
    {
        new[] { new Vector2Int(0, 0), new Vector2Int(-2, 0), new Vector2Int(1, 0), new Vector2Int(-2, -1), new Vector2Int(1, 2) },   // 0->1
        new[] { new Vector2Int(0, 0), new Vector2Int(2, 0), new Vector2Int(-1, 0), new Vector2Int(2, 1), new Vector2Int(-1, -2) },   // 1->0
        new[] { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(2, 0), new Vector2Int(-1, 2), new Vector2Int(2, -1) },   // 1->2
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-2, 0), new Vector2Int(1, -2), new Vector2Int(-2, 1) },   // 2->1
        new[] { new Vector2Int(0, 0), new Vector2Int(2, 0), new Vector2Int(-1, 0), new Vector2Int(2, 1), new Vector2Int(-1, -2) },   // 2->3
        new[] { new Vector2Int(0, 0), new Vector2Int(-2, 0), new Vector2Int(1, 0), new Vector2Int(-2, -1), new Vector2Int(1, 2) },   // 3->2
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-2, 0), new Vector2Int(1, -2), new Vector2Int(-2, 1) },   // 3->0
        new[] { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(2, 0), new Vector2Int(-1, 2), new Vector2Int(2, -1) },   // 0->3
    };

    private static readonly Vector2Int[] NoKicks = { new Vector2Int(0, 0) };

    // Maps an (fromRot, toRot) pair (adjacent CW or CCW states) to a kick-table index.
    private static int KickIndex(int fromRot, int toRot)
    {
        switch (fromRot * 10 + toRot)
        {
            case 1: return 0;   // 0->1
            case 10: return 1;  // 1->0
            case 12: return 2;  // 1->2
            case 21: return 3;  // 2->1
            case 23: return 4;  // 2->3
            case 32: return 5;  // 3->2
            case 30: return 6;  // 3->0
            case 3: return 7;   // 0->3
            default: return -1;
        }
    }

    // Ordered candidate wall-kick offsets for a rotation from fromRot to toRot.
    // The O piece never kicks; unknown transitions fall back to no kick.
    public static Vector2Int[] GetKicks(TetrominoType type, int fromRot, int toRot)
    {
        if (type == TetrominoType.O) return NoKicks;
        int idx = KickIndex(fromRot & 3, toRot & 3);
        if (idx < 0) return NoKicks;
        return (type == TetrominoType.I) ? IKicks[idx] : JlstzKicks[idx];
    }
}
