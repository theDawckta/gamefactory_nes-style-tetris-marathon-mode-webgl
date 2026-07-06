using System;
using System.Collections;
using UnityEngine;
using OneTimeGames.CoreSystems;

public class FactoryAuthController : MonoBehaviour
{
    [SerializeField] private FactoryAuth _factoryAuth;

    public string Username { get; private set; }
    public string CharacterName { get; private set; }
    public string Token { get; private set; }
    public bool IsGuest { get; private set; }
    public bool IsResolved { get; private set; }

    public event Action<FactoryAuthController> OnIdentityResolved;

    private IEnumerator Start()
    {
        if (_factoryAuth == null)
            _factoryAuth = GetComponent<FactoryAuth>() ?? gameObject.AddComponent<FactoryAuth>();

        if (ConfigService.Instance != null)
            yield return ConfigService.Instance.EnsureLoaded();

        var url = ConfigService.Instance != null ? ConfigService.Instance.Get("charactersBaseUrl") : "";
        _factoryAuth.charactersBaseUrl = url;
        _factoryAuth.OnResolved += HandleResolved;
        _factoryAuth.OnFailed += HandleFailed;
        _factoryAuth.Resolve();
    }

    private void HandleResolved(FactoryAuth auth)
    {
        Username = auth.Username;
        CharacterName = auth.CharacterName;
        Token = auth.Token;
        IsGuest = auth.IsGuest;
        IsResolved = true;
        OnIdentityResolved?.Invoke(this);
    }

    private void HandleFailed(string reason)
    {
        Debug.LogWarning("[FactoryAuthController] Identity resolution failed: " + reason + ". Treating as guest.");
        Username = "guest";
        CharacterName = null;
        Token = "";
        IsGuest = true;
        IsResolved = true;
        OnIdentityResolved?.Invoke(this);
    }
}
