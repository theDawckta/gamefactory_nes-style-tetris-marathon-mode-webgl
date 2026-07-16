using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class HelpButtonWidget : MonoBehaviour
{
    [SerializeField] private TutorialScreen _tutorialScreen;
    [SerializeField] private FontAsset _font;

    public Button HelpButton { get; private set; }
    private TutorialScreen _activeScreen;

    public void Initialize(VisualElement gameScreenRoot, TutorialScreen tutorialScreen)
    {
        if (gameScreenRoot == null) return;
        _activeScreen = tutorialScreen ?? _tutorialScreen;

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

        gameScreenRoot.Add(HelpButton);

        if (_activeScreen != null)
            _activeScreen.OnHide += OnTutorialHidden;
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
