using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FactoryAuthControllerTests
{
    [UnityTest]
    public IEnumerator Component_AttachesToGameObject()
    {
        var go = new GameObject();
        go.AddComponent<FactoryAuthController>();
        yield return null;
        Assert.IsNotNull(go.GetComponent<FactoryAuthController>());
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator DefaultValues_BeforeStart_IsResolvedFalse()
    {
        var go = new GameObject();
        var controller = go.AddComponent<FactoryAuthController>();
        // Check immediately after Awake, before Start() runs
        Assert.IsFalse(controller.IsResolved);
        Assert.IsFalse(controller.IsGuest);
        Assert.IsNull(controller.Username);
        Assert.IsNull(controller.CharacterName);
        Assert.IsNull(controller.Token);
        yield return null;
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator NoBackend_ResolvesToGuest()
    {
        var go = new GameObject();
        var controller = go.AddComponent<FactoryAuthController>();
        yield return null; // Start() runs; FactoryAuth gets empty URL, fires OnFailed, guest fallback
        Assert.IsTrue(controller.IsResolved);
        Assert.IsTrue(controller.IsGuest);
        Assert.AreEqual("guest", controller.Username);
        Assert.IsNull(controller.CharacterName);
        Assert.AreEqual("", controller.Token);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator NoBackend_FiresOnIdentityResolved()
    {
        var go = new GameObject();
        var controller = go.AddComponent<FactoryAuthController>();
        bool fired = false;
        controller.OnIdentityResolved += _ => fired = true;
        yield return null;
        Assert.IsTrue(fired);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator OnIdentityResolved_PassesThisController()
    {
        var go = new GameObject();
        var controller = go.AddComponent<FactoryAuthController>();
        FactoryAuthController received = null;
        controller.OnIdentityResolved += c => received = c;
        yield return null;
        Assert.AreSame(controller, received);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator NoBackend_AddsFactoryAuthComponent()
    {
        var go = new GameObject();
        go.AddComponent<FactoryAuthController>();
        yield return null;
        Assert.IsNotNull(go.GetComponent<OneTimeGames.CoreSystems.FactoryAuth>());
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator PreWiredFactoryAuth_IsUsed()
    {
        var go = new GameObject();
        // Pre-wire FactoryAuth before FactoryAuthController is added
        var auth = go.AddComponent<OneTimeGames.CoreSystems.FactoryAuth>();
        var controller = go.AddComponent<FactoryAuthController>();
        yield return null;
        // Should still resolve as guest (no URL) and not create a second FactoryAuth
        Assert.AreEqual(1, go.GetComponents<OneTimeGames.CoreSystems.FactoryAuth>().Length);
        Assert.IsTrue(controller.IsResolved);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator OnIdentityResolved_FiresExactlyOnce()
    {
        var go = new GameObject();
        var controller = go.AddComponent<FactoryAuthController>();
        int count = 0;
        controller.OnIdentityResolved += _ => count++;
        yield return null;
        Assert.AreEqual(1, count);
        Object.Destroy(go);
    }
}
