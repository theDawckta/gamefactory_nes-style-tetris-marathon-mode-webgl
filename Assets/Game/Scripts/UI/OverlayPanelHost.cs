using UnityEngine;
using UnityEngine.UIElements;

// Shared PanelSettings for OVERLAY UI (tutorial modal, '?' HUD button).
//
// The game's main PanelSettings is orientation-scaled by OrientationPanelMatch: in
// portrait it switches to match-WIDTH so the landscape-authored game layout fits across
// the screen -- which shrinks the panel scale to ~0.33 on a phone. Overlay UI authored in
// panel units becomes microscopic there (a 40-unit button renders ~13 physical px --
// untappable; this is exactly why the '?' button "did not work" on mobile portrait).
//
// Overlays therefore live on their own CLONED PanelSettings that always matches HEIGHT
// (finger-sized in both orientations), with PanelSettings.sortingOrder raised above the
// main panel so overlay documents render and pick on top of every game document
// (screens/gesture zones) regardless of their UIDocument.sortingOrder values.
public static class OverlayPanelHost
{
    private static PanelSettings _overlay;
    private static PanelSettings _source;

    public static PanelSettings GetOrCreate(PanelSettings source)
    {
        if (source == null) return null;
        if (_overlay != null && _source == source) return _overlay;
        _overlay = Object.Instantiate(source);
        _overlay.name = source.name + " (Overlay)";
        _overlay.match = 1f; // always match height -- never shrunk by the portrait width-fit
        _overlay.sortingOrder = source.sortingOrder + 100f;
        _source = source;
        return _overlay;
    }
}
