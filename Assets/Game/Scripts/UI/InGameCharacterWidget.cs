using UnityEngine;
using UnityEngine.UIElements;

public class InGameCharacterWidget : MonoBehaviour
{
    [SerializeField] private FactoryAuthController _authController;
    [SerializeField] private CharacterIdleDisplay _charIdleDisplay;

    public void Initialize(VisualElement characterIdleRegion)
    {
        if (_charIdleDisplay != null)
            _charIdleDisplay.Root = characterIdleRegion;
        if (characterIdleRegion != null)
            characterIdleRegion.style.display = DisplayStyle.None;
    }

    private void Awake()
    {
        if (_authController != null)
            _authController.OnIdentityResolved += HandleIdentityResolved;
    }

    private void OnDestroy()
    {
        if (_authController != null)
            _authController.OnIdentityResolved -= HandleIdentityResolved;
    }

    private void HandleIdentityResolved(FactoryAuthController controller)
    {
        if (string.IsNullOrEmpty(controller.CharacterName))
            return;

        string baseUrl = ConfigService.Instance != null
            ? ConfigService.Instance.Get("charactersBaseUrl")
            : "";
        _charIdleDisplay.Load(controller.CharacterName, baseUrl);
    }
}
