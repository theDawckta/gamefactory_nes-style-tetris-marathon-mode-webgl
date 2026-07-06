using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class LeaderboardWidget : MonoBehaviour
{
    [SerializeField] private LeaderboardClient _leaderboardClient;
    [SerializeField] private string _charactersBaseUrl;

    private VisualElement _leaderboardRegion;
    private CharacterIdleDisplay[] _charDisplays;
    private const int RowCount = 5;

    private void Awake()
    {
        _charDisplays = new CharacterIdleDisplay[RowCount];
        for (int i = 0; i < RowCount; i++)
        {
            var helperGo = new GameObject($"LeaderboardCharRow{i}");
            helperGo.transform.SetParent(transform);
            _charDisplays[i] = helperGo.AddComponent<CharacterIdleDisplay>();
        }
    }

    private IEnumerator Start()
    {
        if (string.IsNullOrEmpty(_charactersBaseUrl) && ConfigService.Instance != null)
        {
            yield return ConfigService.Instance.EnsureLoaded();
            var url = ConfigService.Instance.Get("charactersBaseUrl");
            if (!string.IsNullOrEmpty(url))
                _charactersBaseUrl = url;
        }
    }

    public void SetLeaderboardRegion(VisualElement region)
    {
        _leaderboardRegion = region;
        WireDisplayRoots();
    }

    private void WireDisplayRoots()
    {
        if (_leaderboardRegion == null || _charDisplays == null) return;
        for (int i = 0; i < RowCount && i < _leaderboardRegion.childCount; i++)
        {
            var row = _leaderboardRegion[i];
            if (row.childCount >= 4)
            {
                _charDisplays[i].Root = row[3];
                _charDisplays[i].Hide();
            }
        }
    }

    public void Refresh()
    {
        if (_leaderboardClient == null || _leaderboardRegion == null) return;
        StartCoroutine(RefreshCoroutine());
    }

    private IEnumerator RefreshCoroutine()
    {
        List<LeaderboardEntry> entries = null;
        yield return _leaderboardClient.FetchTopFive(
            e => entries = e,
            _ => { }
        );

        if (entries == null)
        {
            ClearAllRows();
            yield break;
        }

        for (int i = 0; i < RowCount; i++)
        {
            if (i < entries.Count)
                yield return PopulateRow(i, entries[i]);
            else
                ClearRow(i);
        }
    }

    private IEnumerator PopulateRow(int i, LeaderboardEntry entry)
    {
        if (_leaderboardRegion == null || i >= _leaderboardRegion.childCount) yield break;
        var row = _leaderboardRegion[i];
        if (row.childCount < 4) yield break;

        if (row[0] is Label rankLabel) rankLabel.text = $"{entry.rank}.";
        if (row[1] is Label scoreLabel) scoreLabel.text = entry.score.ToString();
        if (row[2] is Label usernameLabel) usernameLabel.text = entry.username;

        string charName = null;
        if (!string.IsNullOrEmpty(_charactersBaseUrl) && !string.IsNullOrEmpty(entry.username))
            yield return ResolveCharacterName(entry.username, name => charName = name);

        if (_charDisplays != null && i < _charDisplays.Length && _charDisplays[i] != null)
        {
            if (!string.IsNullOrEmpty(charName))
            {
                _charDisplays[i].Show();
                _charDisplays[i].Load(charName, _charactersBaseUrl);
            }
            else
            {
                _charDisplays[i].Hide();
            }
        }
    }

    private IEnumerator ResolveCharacterName(string username, Action<string> onResolved)
    {
        using var req = UnityWebRequest.Get($"{_charactersBaseUrl}/api/characters?username={username}");
        req.timeout = 5;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onResolved?.Invoke(null);
            yield break;
        }

        var response = TryParseCharacterResponse(req.downloadHandler.text);
        onResolved?.Invoke(response?.characterName);
    }

    private static CharacterLookupResponse TryParseCharacterResponse(string json)
    {
        try
        {
            return JsonUtility.FromJson<CharacterLookupResponse>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void ClearAllRows()
    {
        for (int i = 0; i < RowCount; i++)
            ClearRow(i);
    }

    private void ClearRow(int i)
    {
        if (_leaderboardRegion == null || i >= _leaderboardRegion.childCount) return;
        var row = _leaderboardRegion[i];
        if (row.childCount < 4) return;
        if (row[0] is Label r) r.text = "";
        if (row[1] is Label s) s.text = "";
        if (row[2] is Label u) u.text = "";
        if (_charDisplays != null && i < _charDisplays.Length && _charDisplays[i] != null)
            _charDisplays[i].Hide();
    }

    [Serializable]
    private class CharacterLookupResponse
    {
        public string characterName;
    }
}
