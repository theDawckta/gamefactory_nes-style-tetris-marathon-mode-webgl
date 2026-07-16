using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using OneTimeGames.CoreSystems;

// Calls Initialize() on GameScreen widgets after UIDocument panels are ready.
// Must be on the same GameObject as GameScreen and all widget components.
public class SceneBootstrapper : MonoBehaviour
{
    [SerializeField] private PlayfieldController _playfieldController;
    [SerializeField] private TutorialScreen _tutorialScreen;

    private void Awake()
    {
        // Portrait width-fit (factory standard, see coresystems-conventions): in portrait
        // the shared PanelSettings switches to match-width so the landscape-authored layout
        // fits across the screen instead of cropping the HUD; the full-screen gesture zones
        // keep covering the whole physical screen either way.
        // CONTENT-FIT 440: the visible content row is ~360 reference units wide (playfield
        // 200 + NEXT ~96 + score sidebar ~60), centered in the 1200-unit reference canvas --
        // fitting the FULL canvas made the game tiny in portrait. 440 (~20% margin) makes
        // the content fill the screen width instead.
        var doc = GetComponent<UIDocument>();
        if (doc != null && doc.panelSettings != null)
        {
            var opm = gameObject.AddComponent<OrientationPanelMatch>();
            opm.Configure(doc.panelSettings, 440f);
        }
    }

    private IEnumerator Start()
    {
        yield return null; // wait one frame for UIDocument panels to initialize

        var gs = GetComponent<GameScreen>();
        if (gs == null) yield break;

        var pr = GetComponent<PlayfieldRenderer>();
        if (pr != null)
        {
            pr.Initialize(gs.PlayfieldRegion, _playfieldController);
            pr.SetActive(true);
        }

        GetComponent<ScoreWidget>()?.Initialize(gs.ScoreRegion, _playfieldController);
        GetComponent<LinesWidget>()?.Initialize(gs.LinesRegion, _playfieldController);
        GetComponent<LevelWidget>()?.Initialize(gs.LevelRegion, _playfieldController);
        GetComponent<NextPieceWidget>()?.Initialize(gs.NextPieceRegion, _playfieldController);
        GetComponent<InGameCharacterWidget>()?.Initialize(gs.CharacterIdleRegion);
        GetComponent<HelpButtonWidget>()?.Initialize(gs.Root, _tutorialScreen);
    }
}
