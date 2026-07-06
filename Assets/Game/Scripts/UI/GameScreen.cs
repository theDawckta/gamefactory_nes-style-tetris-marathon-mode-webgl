using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameScreen : BaseScreen
{
    [SerializeField] private FontAsset _font;

    public VisualElement PlayfieldRegion { get; private set; }
    public VisualElement ScoreRegion { get; private set; }
    public VisualElement LinesRegion { get; private set; }
    public VisualElement LevelRegion { get; private set; }
    public VisualElement NextPieceRegion { get; private set; }
    public VisualElement CharacterIdleRegion { get; private set; }

    private void OnEnable()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        var root = Root;
        if (root == null) return;

        root.Clear();
        root.style.flexDirection = FlexDirection.Row;
        root.style.alignItems = Align.Center;
        root.style.justifyContent = Justify.Center;
        root.style.backgroundColor = new StyleColor(new Color(0.039f, 0.039f, 0.039f, 1f));
        root.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
        root.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));

        var leftSidebar = new VisualElement();
        leftSidebar.style.flexDirection = FlexDirection.Column;

        ScoreRegion = new VisualElement();
        ScoreRegion.name = "scoreRegion";

        LinesRegion = new VisualElement();
        LinesRegion.name = "linesRegion";

        LevelRegion = new VisualElement();
        LevelRegion.name = "levelRegion";

        leftSidebar.Add(ScoreRegion);
        leftSidebar.Add(LinesRegion);
        leftSidebar.Add(LevelRegion);

        PlayfieldRegion = new VisualElement();
        PlayfieldRegion.name = "playfieldRegion";
        PlayfieldRegion.style.width = 200;
        PlayfieldRegion.style.height = 400;
        PlayfieldRegion.style.borderTopWidth = 1;
        PlayfieldRegion.style.borderBottomWidth = 1;
        PlayfieldRegion.style.borderLeftWidth = 1;
        PlayfieldRegion.style.borderRightWidth = 1;
        PlayfieldRegion.style.borderTopColor = new StyleColor(Color.white);
        PlayfieldRegion.style.borderBottomColor = new StyleColor(Color.white);
        PlayfieldRegion.style.borderLeftColor = new StyleColor(Color.white);
        PlayfieldRegion.style.borderRightColor = new StyleColor(Color.white);

        var rightSidebar = new VisualElement();
        rightSidebar.style.flexDirection = FlexDirection.Column;

        NextPieceRegion = new VisualElement();
        NextPieceRegion.name = "nextPieceRegion";

        CharacterIdleRegion = new VisualElement();
        CharacterIdleRegion.name = "characterIdleRegion";
        CharacterIdleRegion.style.display = DisplayStyle.None;

        rightSidebar.Add(NextPieceRegion);
        rightSidebar.Add(CharacterIdleRegion);

        root.Add(leftSidebar);
        root.Add(PlayfieldRegion);
        root.Add(rightSidebar);
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }
}
