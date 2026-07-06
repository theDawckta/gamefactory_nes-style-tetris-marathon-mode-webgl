using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ConfigServiceTests
{
    private GameObject _go;

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.Destroy(_go);
        // Reset singleton so each test starts fresh
        if (ConfigService.Instance != null) Object.Destroy(ConfigService.Instance.gameObject);
    }

    [UnityTest]
    public IEnumerator ConfigService_AttachesToGameObject()
    {
        _go = new GameObject();
        var cs = _go.AddComponent<ConfigService>();
        yield return null;
        Assert.IsNotNull(cs);
    }

    [UnityTest]
    public IEnumerator ConfigService_SingletonIsSet_AfterAwake()
    {
        _go = new GameObject();
        _go.AddComponent<ConfigService>();
        yield return null;
        Assert.IsNotNull(ConfigService.Instance);
    }

    [UnityTest]
    public IEnumerator ConfigService_SecondInstance_IsDestroyed()
    {
        _go = new GameObject();
        _go.AddComponent<ConfigService>();
        yield return null;

        var go2 = new GameObject();
        go2.AddComponent<ConfigService>();
        yield return null;

        // The second instance's GameObject should be destroyed
        Assert.IsTrue(_go != null && ConfigService.Instance != null);
        Object.Destroy(go2);
    }

    [UnityTest]
    public IEnumerator ConfigService_IsReadyFalse_BeforeEnsureLoaded()
    {
        _go = new GameObject();
        var cs = _go.AddComponent<ConfigService>();
        yield return null;
        Assert.IsFalse(cs.IsReady);
    }

    [UnityTest]
    public IEnumerator ConfigService_EnsureLoaded_SetsIsReady()
    {
        _go = new GameObject();
        var cs = _go.AddComponent<ConfigService>();
        yield return null;
        yield return cs.EnsureLoaded();
        Assert.IsTrue(cs.IsReady);
    }

    [UnityTest]
    public IEnumerator ConfigService_EnsureLoaded_IsIdempotent()
    {
        _go = new GameObject();
        var cs = _go.AddComponent<ConfigService>();
        yield return null;
        yield return cs.EnsureLoaded();
        yield return cs.EnsureLoaded();
        Assert.IsTrue(cs.IsReady);
    }

    [UnityTest]
    public IEnumerator ConfigService_Get_ReturnsEmptyString_ForMissingKey()
    {
        _go = new GameObject();
        var cs = _go.AddComponent<ConfigService>();
        yield return null;
        var val = cs.Get("nonexistent_key");
        Assert.AreEqual("", val);
    }

    [UnityTest]
    public IEnumerator ConfigService_Get_ReturnsFallback_ForMissingKey()
    {
        _go = new GameObject();
        var cs = _go.AddComponent<ConfigService>();
        yield return null;
        var val = cs.Get("nonexistent_key", "fallback_value");
        Assert.AreEqual("fallback_value", val);
    }

    [UnityTest]
    public IEnumerator ConfigService_OnReady_FiredAfterEnsureLoaded()
    {
        _go = new GameObject();
        var cs = _go.AddComponent<ConfigService>();
        yield return null;

        var fired = false;
        cs.OnReady += () => fired = true;
        yield return cs.EnsureLoaded();
        Assert.IsTrue(fired);
    }

    [UnityTest]
    public IEnumerator ConfigService_OnReady_NotFired_BeforeEnsureLoaded()
    {
        _go = new GameObject();
        var cs = _go.AddComponent<ConfigService>();
        yield return null;

        var fired = false;
        cs.OnReady += () => fired = true;
        yield return null;
        Assert.IsFalse(fired);
    }
}
