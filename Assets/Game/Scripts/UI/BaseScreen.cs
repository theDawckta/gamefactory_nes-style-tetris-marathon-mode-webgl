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

    public virtual void Show()
    {
        // Only activate if truly inactive. In the game scene all screen GOs are always
        // active, so this path is a no-op there. Skipping the redundant SetActive(true)
        // prevents OnEnable from firing mid-frame and triggering tree modifications
        // during a UIElements generateVisualContent callback.
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        Root.style.display = DisplayStyle.Flex;
    }

    public virtual void Hide()
    {
        Root.style.display = DisplayStyle.None;
    }

    public bool IsVisible => Root.style.display.value != DisplayStyle.None;
}
