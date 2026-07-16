using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

// '?' help button that opens the TutorialScreen during gameplay.
//
// The button lives in its OWN UIDocument at sortingOrder 300 (factory layering ladder:
// game screens 0, mobile gesture overlay 100, modal overlays 200, HUD buttons 300).
// It originally lived inside the GameScreen document (sortingOrder 0), where the
// full-screen right-half GestureZone in the mobile overlay (100) sat ABOVE it and
// swallowed every tap -- tapping '?' rotated the piece instead of opening the tutorial.
// The document root's pickingMode is Ignore so ONLY the small button itself picks;
// taps anywhere else fall through to the tutorial/zones/screens beneath.
public class HelpButtonWidget : MonoBehaviour
{
    [SerializeField] private TutorialScreen _tutorialScreen;
    [SerializeField] private FontAsset _font;

    public Button HelpButton { get; private set; }
    private TutorialScreen _activeScreen;
    private UIDocument _buttonDoc;
    private bool _gameScreenVisible;

    // Signature kept from the original in-GameScreen version so SceneBootstrapper's
    // call site is unchanged; gameScreenRoot is no longer the button's parent.
    public void Initialize(VisualElement gameScreenRoot, TutorialScreen tutorialScreen)
    {
        _activeScreen = tutorialScreen ?? _tutorialScreen;
        StartCoroutine(BuildOwnDocument());
    }

    private IEnumerator BuildOwnDocument()
    {
        var hostDoc = GetComponent<UIDocument>();
        if (hostDoc == null || hostDoc.panelSettings == null) yield break;

        var go = new GameObject("HelpButtonLayer");
        go.transform.SetParent(transform, false);
        _buttonDoc = go.AddComponent<UIDocument>();
        _buttonDoc.panelSettings = hostDoc.panelSettings;
        _buttonDoc.sortingOrder = 300;

        yield return null; // let the UIDocument build rootVisualElement

        var root = _buttonDoc.rootVisualElement;
        if (root == null) yield break;
        root.pickingMode = PickingMode.Ignore;

        HelpButton = new Button(OnHelpPressed);
        HelpButton.name = "helpButton";
        HelpButton.text = "?";
        HelpButton.style.position = Position.Absolute;
        HelpButton.style.top = 8;
        HelpButton.style.right = 8;
        HelpButton.style.width = 40;
        HelpButton.style.height = 40;
        HelpButton.style.fontSize = 20;
        HelpButton.style.color = new StyleColor(Color.white);
        HelpButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1f));
        HelpButton.style.borderTopLeftRadius = 4;
        HelpButton.style.borderTopRightRadius = 4;
        HelpButton.style.borderBottomLeftRadius = 4;
        HelpButton.style.borderBottomRightRadius = 4;
        HelpButton.style.borderTopWidth = 0;
        HelpButton.style.borderBottomWidth = 0;
        HelpButton.style.borderLeftWidth = 0;
        HelpButton.style.borderRightWidth = 0;
        if (_font != null)
            HelpButton.style.unityFontDefinition = new StyleFontDefinition(_font);

        root.Add(HelpButton);

        // The button is only relevant while the GameScreen is up; apply whatever
        // visibility was requested before this document finished building.
        ApplyVisibility();

        if (_activeScreen != null)
            _activeScreen.OnHide += OnTutorialHidden;
    }

    /// <summary>Called by GameScreen.Show()/Hide() -- the '?' only exists during gameplay.</summary>
    public void SetGameScreenVisible(bool visible)
    {
        _gameScreenVisible = visible;
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (HelpButton == null) return;
        HelpButton.style.display = _gameScreenVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnHelpPressed()
    {
        if (HelpButton != null)
            HelpButton.style.visibility = Visibility.Hidden;
        _activeScreen?.Show();
    }

    private void OnTutorialHidden()
    {
        if (HelpButton != null)
            HelpButton.style.visibility = Visibility.Visible;
    }

    private void OnDestroy()
    {
        if (_activeScreen != null)
            _activeScreen.OnHide -= OnTutorialHidden;
    }
}
