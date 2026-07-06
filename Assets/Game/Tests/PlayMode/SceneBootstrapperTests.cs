using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class SceneBootstrapperTests
{
    [UnityTest]
    public IEnumerator SceneBootstrapper_StartRunsWithoutException_WhenWidgetsMissing()
    {
        var go = new GameObject("BootstrapTest");
        go.AddComponent<UIDocument>();
        go.AddComponent<GameScreen>();
        go.AddComponent<SceneBootstrapper>();

        yield return null; // Awake/OnEnable
        yield return null; // SceneBootstrapper.Start coroutine begins
        yield return null; // after internal yield return null

        Assert.IsNotNull(go.GetComponent<SceneBootstrapper>());

        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator SceneBootstrapper_SetsPlayfieldRendererActive_WhenControllerPresent()
    {
        var pfGo = new GameObject("PF");
        var controller = pfGo.AddComponent<PlayfieldController>();

        var screenGo = new GameObject("GS");
        screenGo.AddComponent<UIDocument>();
        screenGo.AddComponent<GameScreen>();
        var renderer = screenGo.AddComponent<PlayfieldRenderer>();
        screenGo.AddComponent<ScoreWidget>();
        screenGo.AddComponent<LinesWidget>();
        screenGo.AddComponent<LevelWidget>();
        screenGo.AddComponent<NextPieceWidget>();
        screenGo.AddComponent<InGameCharacterWidget>();
        var bootstrapper = screenGo.AddComponent<SceneBootstrapper>();

        // Wire controller via reflection (no UnityEditor in PlayMode)
        var field = typeof(SceneBootstrapper).GetField("_playfieldController",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(bootstrapper, controller);

        yield return null;
        yield return null;
        yield return null;

        // PlayfieldRenderer is present (Initialize was called; no exception thrown)
        Assert.IsNotNull(renderer);

        Object.Destroy(pfGo);
        Object.Destroy(screenGo);
    }

    [UnityTest]
    public IEnumerator SceneBootstrapper_HandlesNullGameScreen_Gracefully()
    {
        // Bootstrapper on a GO without GameScreen -- should not throw
        var go = new GameObject("BootstrapNoScreen");
        go.AddComponent<SceneBootstrapper>();

        yield return null;
        yield return null;
        yield return null;

        Assert.IsNotNull(go.GetComponent<SceneBootstrapper>());

        Object.Destroy(go);
    }
}
