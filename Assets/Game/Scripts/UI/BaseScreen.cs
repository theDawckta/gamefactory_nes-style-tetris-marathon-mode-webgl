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
        gameObject.SetActive(true);
        Root.style.display = DisplayStyle.Flex;
    }

    public virtual void Hide()
    {
        Root.style.display = DisplayStyle.None;
    }

    public bool IsVisible => Root.style.display.value != DisplayStyle.None;
}
