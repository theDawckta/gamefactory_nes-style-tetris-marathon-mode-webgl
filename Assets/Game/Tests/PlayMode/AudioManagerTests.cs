using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AudioManagerTests
{
    [UnityTest]
    public IEnumerator AudioManager_SingletonIsSet_AfterAwake()
    {
        var go = new GameObject();
        go.AddComponent<AudioManager>();
        yield return null;
        Assert.IsNotNull(AudioManager.Instance);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator AudioManager_PlayMusic_DoesNotThrow()
    {
        var go = new GameObject();
        var am = go.AddComponent<AudioManager>();
        yield return null;
        Assert.DoesNotThrow(() => am.PlayMusic("theme"));
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator AudioManager_PlaySFX_DoesNotThrow()
    {
        var go = new GameObject();
        var am = go.AddComponent<AudioManager>();
        yield return null;
        Assert.DoesNotThrow(() => am.PlaySFX("clear"));
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator AudioManager_DuplicateInstance_IsDestroyed()
    {
        var go1 = new GameObject();
        go1.AddComponent<AudioManager>();
        yield return null;
        var first = AudioManager.Instance;

        var go2 = new GameObject();
        go2.AddComponent<AudioManager>();
        yield return null;

        Assert.AreSame(first, AudioManager.Instance);
        Object.Destroy(go1);
        Object.Destroy(go2);
    }

    [TearDown]
    public void TearDown()
    {
        // Reset singleton between tests
        var existing = AudioManager.Instance;
        if (existing != null)
            Object.Destroy(existing.gameObject);
    }
}
