using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public abstract class BaseScreen : MonoBehaviour
{
    private UIDocument _document;

    protected UIDocument Document
    {
        get
        {
            if (_document == null)
                _document = GetComponent<UIDocument>();
            return _document;
        }
    }

    public virtual VisualElement Root => Document.rootVisualElement;

    private bool _showRequested;

    public virtual void Show()
    {
        // Only activate if truly inactive. In the game scene all screen GOs are always
        // active, so this path is a no-op there. Skipping the redundant SetActive(true)
        // prevents OnEnable from firing mid-frame and triggering tree modifications
        // during a UIElements generateVisualContent callback.
        _showRequested = true;
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        Root.style.display = DisplayStyle.Flex;
    }

    // Call at the END of a screen's BuildUI(): starts the screen hidden unless Show()
    // was already requested before this screen's Start() ran (Start-order race).
    // Without this, every screen renders VISIBLE for the frames between scene load and
    // the state machine's first transition (GameSessionController.Start yields on
    // config/auth fetches), flashing the GameScreen before the StartScreen on boot.
    protected void ApplyInitialHidden()
    {
        if (!_showRequested)
            Root.style.display = DisplayStyle.None;
    }

    public virtual void Hide()
    {
        Root.style.display = DisplayStyle.None;
    }

    public bool IsVisible => Root.style.display.value != DisplayStyle.None;
}
