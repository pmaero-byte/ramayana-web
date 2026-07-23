// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Kanda Permissions (Day 23/45)
// Phase 2: progression-based unlock using SaveSystem.GetMostRecentSave().
// Rule: a kanda is unlocked if all prior kandas in canonical order have been
// visited (scene in save.visitedScenes) or if no save exists (development mode).
// ════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;

namespace Jambudweep.Ramayana.Gameplay
{
    public static class KandaPermissions
    {
        public static bool IsUnlocked(string kandaId)
        {
            if (string.IsNullOrEmpty(kandaId))
                return false;

            var entry = KandaTree.GetEntry(kandaId);
            if (entry == null)
                return false;

            // Development shortcut: if no save exists, unlock all kandas.
            var mostRecent = Jambudweep.Ramayana.Core.SaveSystem.GetMostRecentSave();
            if (mostRecent == null || string.IsNullOrEmpty(mostRecent.currentActId))
                return true;

            // Build visited scene set from save for quick lookup.
            var visited = new HashSet<string>(mostRecent.visitedScenes ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            // A kanda is unlocked if all prior kandas in canonical order are visited.
            foreach (var prior in KandaTree.Entries)
            {
                if (prior.Order >= entry.Order)
                    break;
                if (!visited.Contains(prior.SceneName))
                    return false;
            }
            return true;
        }
    }
}
