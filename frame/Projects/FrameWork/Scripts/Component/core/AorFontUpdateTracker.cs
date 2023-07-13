using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class AorFontUpdateTracker {
    private static Dictionary<Font, List<AorTextIncSprites>> m_Tracked = new Dictionary<Font, List<AorTextIncSprites>>();

    public static void TrackText(AorTextIncSprites t) {
        if (t.font == null)
            return;

        List<AorTextIncSprites> exists;
        m_Tracked.TryGetValue(t.font, out exists);
        if (exists == null) {
            exists = new List<AorTextIncSprites>();
            m_Tracked.Add(t.font, exists);

            t.font.textureRebuildCallback += RebuildForFont(t.font);
        }

        for (int i = 0; i < exists.Count; i++) {
            if (exists[i] == t)
                return;
        }

        exists.Add(t);
    }

    private static Font.FontTextureRebuildCallback RebuildForFont(Font f) {
        return () => {
            List<AorTextIncSprites> texts;
            m_Tracked.TryGetValue(f, out texts);

            if (texts == null)
                return;

            for (var i = 0; i < texts.Count; i++)
                texts[i].FontTextureChanged();
        };
    }

    public static void UntrackText(AorTextIncSprites t) {
        if (t.font == null)
            return;

        List<AorTextIncSprites> texts;
        m_Tracked.TryGetValue(t.font, out texts);

        if (texts == null)
            return;

        texts.Remove(t);
    }
}