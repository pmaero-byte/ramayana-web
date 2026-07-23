// ════════════════════════════════════════════════════════════════════════════
// RAMAYANA PS5 — Kanda Permissions (Day 23 stub)
// Phase 1: always true. Replace with save-slot + progression rules later.
// ════════════════════════════════════════════════════════════════════════════

using System;

namespace Jambudweep.Ramayana.Gameplay
{
    public static class KandaPermissions
    {
        public static bool IsUnlocked(string kandaId)
        {
            if (string.IsNullOrEmpty(kandaId))
                return false;

            // Phase 1: all kandas unlocked for development.
            // Hook SaveSystem.GetMostRecentSave() here in Phase 2.
            return true;
        }
    }
}
