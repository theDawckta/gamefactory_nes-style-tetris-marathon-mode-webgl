using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class LeaderboardEntry
{
    public int rank;
    public int score;
    public string username;
}

public class LeaderboardClient : MonoBehaviour
{
    public static LeaderboardClient Instance { get; private set; }

    private string _baseUrl = "";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private IEnumerator EnsureConfigReady()
    {
        if (!string.IsNullOrEmpty(_baseUrl)) yield break;

        if (ConfigService.Instance == null)
        {
            yield break;
        }

        if (!ConfigService.Instance.IsReady)
        {
            var ready = false;
            Action onReady = () => ready = true;
            ConfigService.Instance.OnReady += onReady;
            while (!ready)
                yield return null;
            ConfigService.Instance.OnReady -= onReady;
        }

        var url = ConfigService.Instance.Get("leaderboardUrl");
        if (!string.IsNullOrEmpty(url))
            _baseUrl = url;
    }

    public IEnumerator FetchTopFive(Action<List<LeaderboardEntry>> onSuccess, Action<string> onError)
    {
        yield return EnsureConfigReady();

        if (string.IsNullOrEmpty(_baseUrl))
        {
            onError?.Invoke("Leaderboard service is not configured.");
            yield break;
        }

        using var req = UnityWebRequest.Get(_baseUrl + "/leaderboard");
        req.timeout = 5;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Failed to fetch leaderboard: " + req.error);
            yield break;
        }

        var entries = ParseEntryArray(req.downloadHandler.text, out var parseError);
        if (parseError != null)
        {
            onError?.Invoke(parseError);
            yield break;
        }

        onSuccess?.Invoke(entries);
    }

    public IEnumerator SubmitScore(string username, string bearerToken, int score,
        Action<List<LeaderboardEntry>> onSuccess, Action<string> onError)
    {
        yield return EnsureConfigReady();

        if (string.IsNullOrEmpty(_baseUrl))
        {
            onError?.Invoke("Leaderboard service is not configured.");
            yield break;
        }

        var body = "{\"username\":\"" + username + "\",\"score\":" + score + "}";
        var bodyBytes = Encoding.UTF8.GetBytes(body);

        using var req = new UnityWebRequest(_baseUrl + "/leaderboard", "POST");
        req.uploadHandler = new UploadHandlerRaw(bodyBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + bearerToken);
        req.timeout = 5;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke("Failed to submit score: " + req.error);
            yield break;
        }

        var entries = ParseEntryArray(req.downloadHandler.text, out var parseError);
        if (parseError != null)
        {
            onError?.Invoke(parseError);
            yield break;
        }

        onSuccess?.Invoke(entries);
    }

    // Worker returns a raw JSON array; wrap it so JsonUtility can parse it.
    private static List<LeaderboardEntry> ParseEntryArray(string json, out string error)
    {
        error = null;
        EntryListWrapper wrapper = null;
        try
        {
            wrapper = JsonUtility.FromJson<EntryListWrapper>("{\"entries\":" + json + "}");
        }
        catch (Exception ex)
        {
            error = "Failed to parse leaderboard response: " + ex.Message;
            return null;
        }

        if (wrapper?.entries == null)
        {
            error = "Unexpected leaderboard response format.";
            return null;
        }

        var result = new List<LeaderboardEntry>(wrapper.entries);
        return result;
    }

    [Serializable]
    private class EntryListWrapper
    {
        public LeaderboardEntry[] entries;
    }
}
