using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using OneTimeGames.CoreSystems;

public class CharacterIdleDisplay : MonoBehaviour
{
    public VisualElement Root { get; set; }

    private Sprite[] _frames;
    private int _frameCount;
    private float _fps;
    private float _frameAccum;

    public void Load(string characterName, string charactersBaseUrl)
    {
        StartCoroutine(LoadCoroutine(characterName, charactersBaseUrl));
    }

    public void Hide()
    {
        if (Root != null)
            Root.style.display = DisplayStyle.None;
    }

    public void Show()
    {
        if (Root != null)
            Root.style.display = DisplayStyle.Flex;
    }

    private void Update()
    {
        if (Root == null || _frames == null || _frameCount <= 0)
            return;
        if (Root.style.display.value == DisplayStyle.None)
            return;

        _frameAccum += Time.deltaTime * _fps;
        if (_frameAccum >= _frameCount)
            _frameAccum -= _frameCount;

        int frameIndex = (int)_frameAccum;
        Root.style.backgroundImage = Background.FromSprite(_frames[frameIndex]);
    }

    private IEnumerator LoadCoroutine(string characterName, string charactersBaseUrl)
    {
        using var animReq = UnityWebRequest.Get($"{charactersBaseUrl}/api/characters/{characterName}/animations");
        yield return animReq.SendWebRequest();

        if (animReq.result != UnityWebRequest.Result.Success || !animReq.downloadHandler.text.Contains("idle"))
        {
            Hide();
            yield break;
        }

        string pngUrl = $"{charactersBaseUrl}/characters-static/{characterName}/spritesheets/{characterName}-south-30-idle-rgba.png";
        using var texReq = UnityWebRequestTexture.GetTexture(pngUrl);
        yield return texReq.SendWebRequest();

        if (texReq.result != UnityWebRequest.Result.Success)
        {
            Hide();
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(texReq);
        if (texture == null)
        {
            Hide();
            yield break;
        }

        string jsonUrl = $"{charactersBaseUrl}/characters-static/{characterName}/spritesheets/{characterName}-south-30-idle-rgba.json";
        using var jsonReq = UnityWebRequest.Get(jsonUrl);
        yield return jsonReq.SendWebRequest();

        if (jsonReq.result != UnityWebRequest.Result.Success)
        {
            Hide();
            yield break;
        }

        // Parse outside try-catch: CS1626 forbids yield inside try-catch
        SpriteSheetMeta meta = TryParseMeta(jsonReq.downloadHandler.text);
        if (meta == null || meta.frameCount <= 0 || meta.fps <= 0)
        {
            Hide();
            yield break;
        }

        Sprite[] frames = CharacterSpriteLoader.SliceSpriteSheet(texture, meta);
        if (frames == null || frames.Length < meta.frameCount)
        {
            Hide();
            yield break;
        }

        _frames = frames;
        _frameCount = meta.frameCount;
        _fps = meta.fps;
        _frameAccum = 0f;
        Show();
    }

    private static SpriteSheetMeta TryParseMeta(string json)
    {
        try
        {
            return JsonUtility.FromJson<SpriteSheetMeta>(json);
        }
        catch (System.ArgumentException)
        {
            return null;
        }
    }
}
