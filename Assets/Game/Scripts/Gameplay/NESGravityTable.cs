public static class NESGravityTable
{
    private static readonly float[] Table = new float[]
    {
        0.8167f,  // Level 0
        0.7167f,  // Level 1
        0.6333f,  // Level 2
        0.55f,    // Level 3
        0.4667f,  // Level 4
        0.3833f,  // Level 5
        0.3f,     // Level 6
        0.2167f,  // Level 7
        0.1333f,  // Level 8
        0.1f,     // Level 9
        0.0833f,  // Level 10
        0.0833f,  // Level 11
        0.0833f,  // Level 12
        0.0667f,  // Level 13
        0.0667f,  // Level 14
        0.0667f,  // Level 15
        0.05f,    // Level 16
        0.05f,    // Level 17
        0.05f,    // Level 18
        0.0333f,  // Level 19
        0.0167f,  // Level 20
        0.0167f,  // Level 21
        0.0167f,  // Level 22
        0.0167f,  // Level 23
        0.0167f,  // Level 24
        0.0167f,  // Level 25
        0.0167f,  // Level 26
        0.0167f,  // Level 27
        0.0167f,  // Level 28
        0.0167f,  // Level 29 (cap)
    };

    public static float GetSecondsPerRow(int level)
    {
        if (level < 0) level = 0;
        if (level >= Table.Length) level = Table.Length - 1;
        return Table[level];
    }
}
