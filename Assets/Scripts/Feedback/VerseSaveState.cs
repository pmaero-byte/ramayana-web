// Round 27 — VerseSaveState: lightweight persistent state for verse progression.
// Stores last 20 played verse IDs, best streak, and total played in PlayerPrefs.

using System;
using UnityEngine;

namespace Jambudweep.Ramayana.Feedback
{
    public static class VerseSaveState
    {
        private const string KEY_HISTORY = "verse.history.v1";
        private const string KEY_BEST_STREAK = "verse.best_streak.v1";
        private const string KEY_TOTAL = "verse.total.v1";

        /// <summary>
        /// Restore core fields from canonical SaveData.
        /// </summary>
        /// <param name="data">SaveData from SaveSystem.Load.</param>
        public static bool RestoreFromSaveData(Core.SaveData data)
        {
            if (data == null) return false;
            try
            {
                if (!string.IsNullOrEmpty(data.episodeId))
                {
                    PlayerPrefs.SetString(KEY_HISTORY, data.episodeId);
                }
                PlayerPrefs.SetInt(KEY_TOTAL,
                    Mathf.Max(PlayerPrefs.GetInt(KEY_TOTAL, 0), Mathf.RoundToInt(data.totalPlayTimeSec)));
                if (data.dharmaScore > 0)
                {
                    PlayerPrefs.SetInt(KEY_BEST_STREAK,
                        Mathf.Max(PlayerPrefs.GetInt(KEY_BEST_STREAK, 0), data.dharmaScore));
                }
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[VerseSaveState] Restore failed: " + e.Message);
                return false;
            }
        }

        public static void RecordPlayed(string verseId, int streakAfter)
        {
            // History (last 20, comma-separated)
            string cur = PlayerPrefs.GetString(KEY_HISTORY, "");
            string[] parts = string.IsNullOrEmpty(cur) ? new string[0] : cur.Split('|');
            var list = new System.Collections.Generic.List<string>(parts);
            list.Add(verseId);
            while (list.Count > 20) list.RemoveAt(0);
            PlayerPrefs.SetString(KEY_HISTORY, string.Join("|", list));

            // Total
            int total = PlayerPrefs.GetInt(KEY_TOTAL, 0);
            PlayerPrefs.SetInt(KEY_TOTAL, total + 1);

            // Best streak
            int best = PlayerPrefs.GetInt(KEY_BEST_STREAK, 0);
            if (streakAfter > best) PlayerPrefs.SetInt(KEY_BEST_STREAK, streakAfter);

            PlayerPrefs.Save();
        }

        public static int TotalPlayed() => PlayerPrefs.GetInt(KEY_TOTAL, 0);
        public static int BestStreak() => PlayerPrefs.GetInt(KEY_BEST_STREAK, 0);
        public static string[] History()
        {
            string cur = PlayerPrefs.GetString(KEY_HISTORY, "");
            return string.IsNullOrEmpty(cur) ? new string[0] : cur.Split('|');
        }
    }
}
