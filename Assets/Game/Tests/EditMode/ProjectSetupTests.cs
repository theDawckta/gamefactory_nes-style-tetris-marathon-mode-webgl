using System.IO;
using NUnit.Framework;
using UnityEngine;

public class ProjectSetupTests
{
    [Test]
    public void ConfigJson_Exists()
    {
        var path = Path.Combine(Application.dataPath, "StreamingAssets", "config.json");
        Assert.IsTrue(File.Exists(path), "Assets/StreamingAssets/config.json must exist");
    }

    [Test]
    public void ConfigJson_HasLeaderboardUrlKey()
    {
        var path = Path.Combine(Application.dataPath, "StreamingAssets", "config.json");
        var text = File.ReadAllText(path);
        Assert.IsTrue(text.Contains("leaderboardUrl"), "config.json must contain leaderboardUrl key");
    }

    [Test]
    public void ConfigJson_HasCharactersBaseUrlKey()
    {
        var path = Path.Combine(Application.dataPath, "StreamingAssets", "config.json");
        var text = File.ReadAllText(path);
        Assert.IsTrue(text.Contains("charactersBaseUrl"), "config.json must contain charactersBaseUrl key");
    }

    [Test]
    public void GameScenesFolder_Exists()
    {
        var path = Path.Combine(Application.dataPath, "Game", "Scenes");
        Assert.IsTrue(Directory.Exists(path), "Assets/Game/Scenes/ folder must exist");
    }

    [Test]
    public void GameScriptsFolder_Exists()
    {
        var path = Path.Combine(Application.dataPath, "Game", "Scripts");
        Assert.IsTrue(Directory.Exists(path), "Assets/Game/Scripts/ folder must exist");
    }

    [Test]
    public void GameAudioFolder_Exists()
    {
        var path = Path.Combine(Application.dataPath, "Game", "Audio");
        Assert.IsTrue(Directory.Exists(path), "Assets/Game/Audio/ folder must exist");
    }

    [Test]
    public void GameResourcesFolder_Exists()
    {
        var path = Path.Combine(Application.dataPath, "Game", "Resources");
        Assert.IsTrue(Directory.Exists(path), "Assets/Game/Resources/ folder must exist");
    }

    [Test]
    public void MainScene_Exists()
    {
        var path = Path.Combine(Application.dataPath, "Game", "Scenes", "Main.unity");
        Assert.IsTrue(File.Exists(path), "Assets/Game/Scenes/Main.unity must exist");
    }
}
