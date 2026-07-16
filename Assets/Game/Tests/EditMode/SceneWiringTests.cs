using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using OneTimeGames.CoreSystems;

public class SceneWiringTests
{
    private const string ScenePath = "Assets/Game/Scenes/Main.unity";

    // Opens Main.unity additively so we can inspect the scene hierarchy.
    // The caller is responsible for closing it.
    private static UnityEngine.SceneManagement.Scene OpenScene()
    {
        return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
    }

    // ── MobileInput GO ─────────────────────────────────────────────────────────

    [Test]
    public void MobileInput_GameObjectExistsInScene()
    {
        var scene = OpenScene();
        bool found = false;
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "MobileInput") { found = true; break; }
        EditorSceneManager.CloseScene(scene, true);
        Assert.IsTrue(found, "A root GameObject named 'MobileInput' must exist in Main.unity");
    }

    [Test]
    public void MobileInput_HasMobileControlsOverlay()
    {
        var scene = OpenScene();
        MobileControlsOverlay overlay = null;
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "MobileInput")
            {
                overlay = go.GetComponent<MobileControlsOverlay>();
                break;
            }
        EditorSceneManager.CloseScene(scene, true);
        Assert.IsNotNull(overlay, "MobileInput must have a MobileControlsOverlay component");
    }

    [Test]
    public void MobileInput_HasMobileTetrisInput()
    {
        var scene = OpenScene();
        MobileTetrisInput mti = null;
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "MobileInput")
            {
                mti = go.GetComponent<MobileTetrisInput>();
                break;
            }
        EditorSceneManager.CloseScene(scene, true);
        Assert.IsNotNull(mti, "MobileInput must have a MobileTetrisInput component");
    }

    [Test]
    public void MobileInput_UIDocumentHasPanelSettings()
    {
        var scene = OpenScene();
        UIDocument doc = null;
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "MobileInput")
            {
                doc = go.GetComponent<UIDocument>();
                break;
            }
        EditorSceneManager.CloseScene(scene, true);
        Assert.IsNotNull(doc, "MobileInput must have a UIDocument component");
        Assert.IsNotNull(doc.panelSettings, "MobileInput UIDocument must have PanelSettings assigned");
    }

    [Test]
    public void MobileInput_MobileTetrisInput_PlayfieldControllerWired()
    {
        var scene = OpenScene();
        MobileTetrisInput mti = null;
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "MobileInput")
            {
                mti = go.GetComponent<MobileTetrisInput>();
                break;
            }

        PlayfieldController pc = null;
        if (mti != null)
        {
            var field = typeof(MobileTetrisInput)
                .GetField("_playfieldController", BindingFlags.NonPublic | BindingFlags.Instance);
            pc = field?.GetValue(mti) as PlayfieldController;
        }
        EditorSceneManager.CloseScene(scene, true);

        Assert.IsNotNull(mti, "MobileInput must have MobileTetrisInput");
        Assert.IsNotNull(pc, "MobileTetrisInput._playfieldController must be wired");
    }

    // ── GameSessionController ───────────────────────────────────────────────────

    [Test]
    public void GameSessionController_TutorialScreenWired()
    {
        var scene = OpenScene();
        GameSessionController gsc = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            gsc = go.GetComponent<GameSessionController>();
            if (gsc != null) break;
        }

        TutorialScreen ts = null;
        if (gsc != null)
        {
            var field = typeof(GameSessionController)
                .GetField("tutorialScreen", BindingFlags.NonPublic | BindingFlags.Instance);
            ts = field?.GetValue(gsc) as TutorialScreen;
        }
        EditorSceneManager.CloseScene(scene, true);

        Assert.IsNotNull(gsc, "GameSessionController must exist in scene");
        Assert.IsNotNull(ts, "GameSessionController.tutorialScreen must be wired");
    }

    // ── TutorialScreen ─────────────────────────────────────────────────────────

    [Test]
    public void TutorialScreen_ExistsInScene()
    {
        var scene = OpenScene();
        TutorialScreen ts = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            ts = go.GetComponent<TutorialScreen>();
            if (ts != null) break;
        }
        EditorSceneManager.CloseScene(scene, true);
        Assert.IsNotNull(ts, "A TutorialScreen component must exist in the scene");
    }

    [Test]
    public void TutorialScreen_PlayfieldControllerWired()
    {
        var scene = OpenScene();
        TutorialScreen ts = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            ts = go.GetComponent<TutorialScreen>();
            if (ts != null) break;
        }

        PlayfieldController pc = null;
        if (ts != null)
        {
            var field = typeof(TutorialScreen)
                .GetField("_playfieldController", BindingFlags.NonPublic | BindingFlags.Instance);
            pc = field?.GetValue(ts) as PlayfieldController;
        }
        EditorSceneManager.CloseScene(scene, true);

        Assert.IsNotNull(ts, "TutorialScreen must exist");
        Assert.IsNotNull(pc, "TutorialScreen._playfieldController must be wired");
    }

    // ── HelpButtonWidget ────────────────────────────────────────────────────────

    [Test]
    public void HelpButtonWidget_ExistsInScene()
    {
        var scene = OpenScene();
        HelpButtonWidget hw = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            hw = go.GetComponent<HelpButtonWidget>();
            if (hw == null)
                hw = go.GetComponentInChildren<HelpButtonWidget>(true);
            if (hw != null) break;
        }
        EditorSceneManager.CloseScene(scene, true);
        Assert.IsNotNull(hw, "A HelpButtonWidget component must exist in the scene");
    }

    [Test]
    public void HelpButtonWidget_TutorialScreenWired()
    {
        var scene = OpenScene();
        HelpButtonWidget hw = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            hw = go.GetComponent<HelpButtonWidget>();
            if (hw == null)
                hw = go.GetComponentInChildren<HelpButtonWidget>(true);
            if (hw != null) break;
        }

        TutorialScreen ts = null;
        if (hw != null)
        {
            var field = typeof(HelpButtonWidget)
                .GetField("_tutorialScreen", BindingFlags.NonPublic | BindingFlags.Instance);
            ts = field?.GetValue(hw) as TutorialScreen;
        }
        EditorSceneManager.CloseScene(scene, true);

        Assert.IsNotNull(hw, "HelpButtonWidget must exist");
        Assert.IsNotNull(ts, "HelpButtonWidget._tutorialScreen must be wired");
    }

    // ── SceneBootstrapper ───────────────────────────────────────────────────────

    [Test]
    public void SceneBootstrapper_TutorialScreenWired()
    {
        var scene = OpenScene();
        SceneBootstrapper sb = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            sb = go.GetComponent<SceneBootstrapper>();
            if (sb != null) break;
        }

        TutorialScreen ts = null;
        if (sb != null)
        {
            var field = typeof(SceneBootstrapper)
                .GetField("_tutorialScreen", BindingFlags.NonPublic | BindingFlags.Instance);
            ts = field?.GetValue(sb) as TutorialScreen;
        }
        EditorSceneManager.CloseScene(scene, true);

        Assert.IsNotNull(sb, "SceneBootstrapper must exist in scene");
        Assert.IsNotNull(ts, "SceneBootstrapper._tutorialScreen must be wired");
    }
}
