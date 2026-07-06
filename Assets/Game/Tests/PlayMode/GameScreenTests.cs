using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class GameScreenTests
{
    private GameScreen CreateScreen()
    {
        var go = new GameObject();
        go.SetActive(false);
        var uiDoc = go.AddComponent<UIDocument>();
        uiDoc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var screen = go.AddComponent<GameScreen>();
        return screen;
    }

    [UnityTest]
    public IEnumerator PlayfieldRegion_NonNull_AfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.PlayfieldRegion);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator PlayfieldRegion_HasName_playfieldRegion()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual("playfieldRegion", screen.PlayfieldRegion.name);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator PlayfieldRegion_Width_Is200()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual(200f, screen.PlayfieldRegion.style.width.value.value);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator PlayfieldRegion_Height_Is400()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual(400f, screen.PlayfieldRegion.style.height.value.value);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator ScoreRegion_NonNull_AfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.ScoreRegion);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator ScoreRegion_HasName_scoreRegion()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual("scoreRegion", screen.ScoreRegion.name);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator LinesRegion_NonNull_AfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.LinesRegion);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator LinesRegion_HasName_linesRegion()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual("linesRegion", screen.LinesRegion.name);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator LevelRegion_NonNull_AfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.LevelRegion);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator LevelRegion_HasName_levelRegion()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual("levelRegion", screen.LevelRegion.name);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator NextPieceRegion_NonNull_AfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.NextPieceRegion);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator NextPieceRegion_HasName_nextPieceRegion()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual("nextPieceRegion", screen.NextPieceRegion.name);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator CharacterIdleRegion_NonNull_AfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.CharacterIdleRegion);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator CharacterIdleRegion_HasName_characterIdleRegion()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual("characterIdleRegion", screen.CharacterIdleRegion.name);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator CharacterIdleRegion_HiddenByDefault()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual(DisplayStyle.None, screen.CharacterIdleRegion.style.display.value);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Show_SetsDisplayFlex()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        Assert.AreEqual(DisplayStyle.Flex, screen.Root.style.display.value);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_SetsDisplayNone()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        screen.Hide();
        Assert.AreEqual(DisplayStyle.None, screen.Root.style.display.value);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Show_IsVisibleTrue()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        Assert.IsTrue(screen.IsVisible);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_IsVisibleFalse()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        screen.Hide();
        Assert.IsFalse(screen.IsVisible);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator RequiresUIDocument_ComponentPresent()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.GetComponent<UIDocument>());
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Root_FlexDirectionIsRow()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual(FlexDirection.Row, screen.Root.style.flexDirection.value);
        Object.Destroy(screen.gameObject);
    }
}
