using System;
using System.Collections;
using OneTimeGames.CoreSystems;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class CharacterIdleDisplay : MonoBehaviour
{
    public VisualElement Root { get; set; }

    private Sprite[] _frames;
    private int _frameCount;
    private float _fps;
    private float _frameAccum;
    private bool _isVisible = true;

    public void Load(string characterName, string charactersBaseUrl)
    {
        _frames = null;
        _frameAccum = 0f;
        StartCoroutine(LoadCoroutine(characterName, charactersBaseUrl));
    }

    public void Hide()
    {
        _isVisible = false;
        if (Root != null)
            Root.style.display = DisplayStyle.None;
    }

    public void Show()
    {
        _isVisible = true;
        if (Root != null)
            Root.style.display = DisplayStyle.Flex;
    }

    private void Update()
    {
        if (Root == null || _frames == null || _frameCount <= 0)
            return;
        if (!_isVisible)
            return;

        _frameAccum = (_frameAccum + Time.deltaTime * _fps) % _frameCount;
        int frameIndex = Mathf.FloorToInt(_frameAccum) % _frameCount;
        Root.style.backgroundImage = Background.FromSprite(_frames[frameIndex]);
    }

    private IEnumerator LoadCoroutine(string characterName, string charactersBaseUrl)
    {
        // Step 1: Confirm "idle" animation exists in the character's animation list.
        using var animReq = UnityWebRequest.Get($"{charactersBaseUrl}/api/characters/{characterName}/animations");
        animReq.timeout = 5;
        yield return animReq.SendWebRequest();

        if (animReq.result != UnityWebRequest.Result.Success || !animReq.downloadHandler.text.Contains("\"idle\""))
        {
            Hide();
            yield break;
        }

        // Step 2: Fetch the south idle spritesheet PNG.
        string pngUrl = $"{charactersBaseUrl}/characters-static/{characterName}/spritesheets/{characterName}-south-30-idle-rgba.png";
        using var texReq = UnityWebRequestTexture.GetTexture(pngUrl);
        texReq.timeout = 10;
        yield return texReq.SendWebRequest();

        if (texReq.result != UnityWebRequest.Result.Success)
        {
            Hide();
            yield break;
        }

        var texture = DownloadHandlerTexture.GetContent(texReq);
        if (texture == null)
        {
            Hide();
            yield break;
        }

        // Step 3: Fetch the sidecar JSON.
        string jsonUrl = $"{charactersBaseUrl}/characters-static/{characterName}/spritesheets/{characterName}-south-30-idle-rgba.json";
        using var jsonReq = UnityWebRequest.Get(jsonUrl);
        jsonReq.timeout = 5;
        yield return jsonReq.SendWebRequest();

        if (jsonReq.result != UnityWebRequest.Result.Success)
        {
            Hide();
            yield break;
        }

        if (!TryParseMeta(jsonReq.downloadHandler.text, out var meta) || meta == null)
        {
            Hide();
            yield break;
        }

        // Step 4: Slice the spritesheet using frameCount from the sidecar -- never frames.Length.
        if (!TrySlice(texture, meta, out var frames) || frames == null || frames.Length == 0)
        {
            Hide();
            yield break;
        }

        _frames = frames;
        _frameCount = meta.frameCount;
        _fps = meta.fps;
        _frameAccum = 0f;
    }

    private static bool TryParseMeta(string json, out SpriteSheetMeta meta)
    {
        meta = null;
        try
        {
            meta = JsonUtility.FromJson<SpriteSheetMeta>(json);
            return meta != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool TrySlice(Texture2D texture, SpriteSheetMeta meta, out Sprite[] frames)
    {
        frames = null;
        try
        {
            frames = CharacterSpriteLoader.SliceSpriteSheet(texture, meta);
            return frames != null;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
