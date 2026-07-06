using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConfigService : MonoBehaviour
{
    public static ConfigService Instance { get; private set; }

    private readonly Dictionary<string, string> _values = new();
    private bool _loaded;

    public bool IsReady => _loaded;
    public event Action OnReady;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public IEnumerator EnsureLoaded()
    {
        if (_loaded) yield break;
        var pageUrl = Application.absoluteURL;
        var baseUrl = string.IsNullOrEmpty(pageUrl)
            ? ""
            : pageUrl.Substring(0, pageUrl.LastIndexOf('/') + 1);
        using var req = UnityWebRequest.Get(baseUrl + "config.json");
        req.timeout = 3;
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            var text = req.downloadHandler.text.Trim('{', '}', ' ', '\n');
            foreach (var pair in text.Split(','))
            {
                // Split on first ':' only -- URLs contain multiple colons.
                var kv = pair.Split(new char[] { ':' }, 2);
                if (kv.Length == 2)
                    _values[kv[0].Trim().Trim('"')] = kv[1].Trim().Trim('"');
            }
        }
        _loaded = true;
        OnReady?.Invoke();
    }

    public string Get(string key, string fallback = "") =>
        _values.TryGetValue(key, out var v) ? v : fallback;
}
