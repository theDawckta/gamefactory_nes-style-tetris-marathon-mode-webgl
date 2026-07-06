using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LeaderboardClientTests
{
    private GameObject _clientGo;
    private GameObject _configGo;

    [SetUp]
    public void SetUp()
    {
        // Destroy any lingering singletons from previous tests
        if (LeaderboardClient.Instance != null)
            Object.Destroy(LeaderboardClient.Instance.gameObject);
        if (ConfigService.Instance != null)
            Object.Destroy(ConfigService.Instance.gameObject);
    }

    [TearDown]
    public void TearDown()
    {
        if (_clientGo != null) Object.Destroy(_clientGo);
        if (_configGo != null) Object.Destroy(_configGo);
        if (LeaderboardClient.Instance != null)
            Object.Destroy(LeaderboardClient.Instance.gameObject);
        if (ConfigService.Instance != null)
            Object.Destroy(ConfigService.Instance.gameObject);
    }

    [UnityTest]
    public IEnumerator LeaderboardClient_AttachesToGameObject()
    {
        _clientGo = new GameObject();
        var client = _clientGo.AddComponent<LeaderboardClient>();
        yield return null;
        Assert.IsNotNull(client);
    }

    [UnityTest]
    public IEnumerator LeaderboardClient_SingletonIsSet_AfterAwake()
    {
        _clientGo = new GameObject();
        _clientGo.AddComponent<LeaderboardClient>();
        yield return null;
        Assert.IsNotNull(LeaderboardClient.Instance);
    }

    [UnityTest]
    public IEnumerator LeaderboardClient_SecondInstance_IsDestroyed()
    {
        _clientGo = new GameObject();
        _clientGo.AddComponent<LeaderboardClient>();
        yield return null;

        var go2 = new GameObject();
        go2.AddComponent<LeaderboardClient>();
        yield return null;

        Assert.AreSame(_clientGo.GetComponent<LeaderboardClient>(), LeaderboardClient.Instance);
        Object.Destroy(go2);
    }

    [UnityTest]
    public IEnumerator FetchTopFive_CallsOnError_WhenNoLeaderboardUrlConfigured()
    {
        _configGo = new GameObject();
        var cs = _configGo.AddComponent<ConfigService>();
        yield return null;
        // EnsureLoaded will fail to fetch config.json in test env, but sets IsReady = true with no values.
        yield return cs.EnsureLoaded();
        Assert.IsTrue(cs.IsReady);

        _clientGo = new GameObject();
        _clientGo.AddComponent<LeaderboardClient>();
        yield return null;

        string errorMsg = null;
        List<LeaderboardEntry> successData = null;
        yield return LeaderboardClient.Instance.FetchTopFive(
            entries => successData = entries,
            err => errorMsg = err
        );

        Assert.IsNull(successData);
        Assert.IsNotNull(errorMsg);
        Assert.IsTrue(errorMsg.Length > 0);
    }

    [UnityTest]
    public IEnumerator SubmitScore_CallsOnError_WhenNoLeaderboardUrlConfigured()
    {
        _configGo = new GameObject();
        var cs = _configGo.AddComponent<ConfigService>();
        yield return null;
        yield return cs.EnsureLoaded();
        Assert.IsTrue(cs.IsReady);

        _clientGo = new GameObject();
        _clientGo.AddComponent<LeaderboardClient>();
        yield return null;

        string errorMsg = null;
        List<LeaderboardEntry> successData = null;
        yield return LeaderboardClient.Instance.SubmitScore(
            "TestUser", "test-token", 1000,
            entries => successData = entries,
            err => errorMsg = err
        );

        Assert.IsNull(successData);
        Assert.IsNotNull(errorMsg);
        Assert.IsTrue(errorMsg.Length > 0);
    }

    [UnityTest]
    public IEnumerator FetchTopFive_WaitsForConfigService_BeforeIssuing()
    {
        _configGo = new GameObject();
        var cs = _configGo.AddComponent<ConfigService>();
        yield return null;
        // ConfigService is NOT yet ready

        _clientGo = new GameObject();
        _clientGo.AddComponent<LeaderboardClient>();
        yield return null;

        string errorMsg = null;
        var coroutineDone = false;
        IEnumerator Wrapper()
        {
            yield return LeaderboardClient.Instance.FetchTopFive(
                _ => { },
                err => errorMsg = err
            );
            coroutineDone = true;
        }

        var go = new GameObject();
        var mb = go.AddComponent<CoroutineHost>();
        mb.StartCoroutine(Wrapper());

        // Not done yet because ConfigService is not ready
        yield return null;
        Assert.IsFalse(coroutineDone);

        // Now make ConfigService ready -- this will trigger OnReady, unblocking FetchTopFive
        yield return cs.EnsureLoaded();

        // Wait a frame for the coroutine to proceed
        yield return null;
        yield return null;

        Assert.IsTrue(coroutineDone);
        // onError should be called because leaderboardUrl is not set
        Assert.IsNotNull(errorMsg);

        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator SubmitScore_WaitsForConfigService_BeforeIssuing()
    {
        _configGo = new GameObject();
        var cs = _configGo.AddComponent<ConfigService>();
        yield return null;
        // ConfigService is NOT yet ready

        _clientGo = new GameObject();
        _clientGo.AddComponent<LeaderboardClient>();
        yield return null;

        string errorMsg = null;
        var coroutineDone = false;
        IEnumerator Wrapper()
        {
            yield return LeaderboardClient.Instance.SubmitScore(
                "User", "token", 42,
                _ => { },
                err => errorMsg = err
            );
            coroutineDone = true;
        }

        var go = new GameObject();
        var mb = go.AddComponent<CoroutineHost>();
        mb.StartCoroutine(Wrapper());

        yield return null;
        Assert.IsFalse(coroutineDone);

        yield return cs.EnsureLoaded();
        yield return null;
        yield return null;

        Assert.IsTrue(coroutineDone);
        Assert.IsNotNull(errorMsg);

        Object.Destroy(go);
    }
}

// Minimal MonoBehaviour that can host a coroutine in tests.
public class CoroutineHost : MonoBehaviour { }
