using System.Collections;
using UnityEngine;

// Calls Initialize() on GameScreen widgets after UIDocument panels are ready.
// Must be on the same GameObject as GameScreen and all widget components.
public class SceneBootstrapper : MonoBehaviour
{
    [SerializeField] private PlayfieldController _playfieldController;
    [SerializeField] private TutorialScreen _tutorialScreen;

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
