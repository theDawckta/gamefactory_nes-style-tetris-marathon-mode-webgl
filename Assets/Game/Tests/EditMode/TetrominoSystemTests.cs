using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TetrominoSystemTests
{
    // ── TetrominoData ─────────────────────────────────────────────────────────

    [Test]
    public void GetCells_ReturnsExactlyFourCells_ForAllTypesAndRotations()
    {
        foreach (TetrominoType type in System.Enum.GetValues(typeof(TetrominoType)))
        {
            for (int r = 0; r < 4; r++)
            {
                var cells = TetrominoData.GetCells(type, r);
                Assert.AreEqual(4, cells.Length,
                    $"{type} rotation {r} should have 4 cells");
            }
        }
    }

    [Test]
    public void GetCells_WrapsRotationModulo4()
    {
        var cells0 = TetrominoData.GetCells(TetrominoType.T, 0);
        var cells4 = TetrominoData.GetCells(TetrominoType.T, 4);
        CollectionAssert.AreEqual(cells0, cells4);
    }

    [Test]
    public void GetColor_IsPiece_ReturnsCyan()
    {
        var color = TetrominoData.GetColor(TetrominoType.I);
        Assert.AreEqual(new Color(0f, 1f, 1f), color);
    }

    [Test]
    public void GetColor_OPiece_ReturnsYellow()
    {
        var color = TetrominoData.GetColor(TetrominoType.O);
        Assert.AreEqual(new Color(1f, 1f, 0f), color);
    }

    [Test]
    public void GetColor_TPiece_ReturnsPurple()
    {
        var color = TetrominoData.GetColor(TetrominoType.T);
        Assert.AreEqual(new Color(0.5f, 0f, 0.5f), color);
    }

    [Test]
    public void GetColor_SPiece_ReturnsGreen()
    {
        var color = TetrominoData.GetColor(TetrominoType.S);
        Assert.AreEqual(new Color(0f, 0.5f, 0f), color);
    }

    [Test]
    public void GetColor_ZPiece_ReturnsRed()
    {
        var color = TetrominoData.GetColor(TetrominoType.Z);
        Assert.AreEqual(new Color(1f, 0f, 0f), color);
    }

    [Test]
    public void GetColor_JPiece_ReturnsBlue()
    {
        var color = TetrominoData.GetColor(TetrominoType.J);
        Assert.AreEqual(new Color(0f, 0f, 1f), color);
    }

    [Test]
    public void GetColor_LPiece_ReturnsOrange()
    {
        var color = TetrominoData.GetColor(TetrominoType.L);
        Assert.AreEqual(new Color(1f, 0.5f, 0f), color);
    }

    // ── TetrominoBag ──────────────────────────────────────────────────────────

    [Test]
    public void Next_Over70Calls_EachTypeAppearsExactly10Times()
    {
        var bag = new TetrominoBag(seed: 42);
        var counts = new Dictionary<TetrominoType, int>();
        foreach (TetrominoType t in System.Enum.GetValues(typeof(TetrominoType)))
            counts[t] = 0;

        for (int i = 0; i < 70; i++)
            counts[bag.Next()]++;

        foreach (TetrominoType t in System.Enum.GetValues(typeof(TetrominoType)))
            Assert.AreEqual(10, counts[t], $"{t} should appear exactly 10 times in 70 draws");
    }

    [Test]
    public void Next_DeterministicWithSameSeed()
    {
        var bag1 = new TetrominoBag(seed: 99);
        var bag2 = new TetrominoBag(seed: 99);
        for (int i = 0; i < 14; i++)
            Assert.AreEqual(bag1.Next(), bag2.Next());
    }

    [Test]
    public void Next_ReturnsAllSevenTypes_InFirstBag()
    {
        var bag = new TetrominoBag(seed: 1);
        var seen = new HashSet<TetrominoType>();
        for (int i = 0; i < 7; i++)
            seen.Add(bag.Next());
        Assert.AreEqual(7, seen.Count, "First bag must contain all 7 types");
    }

    [Test]
    public void Next_RefillsAutomatically_BeyondFirstBag()
    {
        var bag = new TetrominoBag(seed: 7);
        var counts = new Dictionary<TetrominoType, int>();
        foreach (TetrominoType t in System.Enum.GetValues(typeof(TetrominoType)))
            counts[t] = 0;
        for (int i = 0; i < 14; i++)
            counts[bag.Next()]++;
        foreach (TetrominoType t in System.Enum.GetValues(typeof(TetrominoType)))
            Assert.AreEqual(2, counts[t], $"{t} should appear exactly 2 times in 14 draws");
    }

    // ── NESGravityTable ───────────────────────────────────────────────────────

    [Test]
    public void GetSecondsPerRow_Level0_Returns0Point8167()
    {
        Assert.That(NESGravityTable.GetSecondsPerRow(0), Is.EqualTo(0.8167f).Within(0.0001f));
    }

    [Test]
    public void GetSecondsPerRow_Level9_Returns0Point1()
    {
        Assert.That(NESGravityTable.GetSecondsPerRow(9), Is.EqualTo(0.1f).Within(0.0001f));
    }

    [Test]
    public void GetSecondsPerRow_Level29_ReturnsCap()
    {
        float cap = NESGravityTable.GetSecondsPerRow(29);
        Assert.That(cap, Is.EqualTo(0.0167f).Within(0.0001f));
    }

    [Test]
    public void GetSecondsPerRow_Level100_ReturnsSameCapAsLevel29()
    {
        float level29 = NESGravityTable.GetSecondsPerRow(29);
        float level100 = NESGravityTable.GetSecondsPerRow(100);
        Assert.AreEqual(level29, level100);
    }

    [Test]
    public void GetSecondsPerRow_NegativeLevel_ClampsToLevel0()
    {
        Assert.AreEqual(NESGravityTable.GetSecondsPerRow(0), NESGravityTable.GetSecondsPerRow(-1));
    }

    [Test]
    public void GetSecondsPerRow_DecreasesWithLevel()
    {
        for (int i = 0; i < 19; i++)
        {
            Assert.LessOrEqual(NESGravityTable.GetSecondsPerRow(i + 1),
                NESGravityTable.GetSecondsPerRow(i),
                $"Level {i + 1} should be <= level {i}");
        }
    }
}
