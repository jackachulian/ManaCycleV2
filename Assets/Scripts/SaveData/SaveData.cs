using System;
using System.Collections.Generic;

namespace SaveDataSystem {
    [System.Serializable]
    public class SaveData {

        public static SaveData current => SaveDataManager.Instance.saveData;

        /// <summary>
        /// Status of the player's cleared levels.
        /// Key = level ID, value = progress data for that level
        /// </summary>
        public Dictionary<string, LevelProgressData> levelProgressDataEntries = new();

        /// <summary>
        /// To be called after a level is finished (either a wiin or a loss).
        /// </summary>
        /// <param name="level">he level played</param>
        /// <param name="cleared">if the level was successfully beaten or not</param>
        /// <param name="highScore">the score earned - will be sored as a highscore if higher than the current score</param>
        /// <param name="clearTime">the time taken to beat the level - will be saved as fastest time if lower than the current score</param>
        public void TrackLevelProgress(Level level, bool cleared, int score, double clearTime) {
            LevelProgressData existingEntry;
            levelProgressDataEntries.TryGetValue(level.levelId, out existingEntry);

            bool existingCleared;
            int existingHighScore;
            double existingFastestClearTime;
            
            if (existingEntry != null) {
                existingCleared = existingEntry.cleared;
                existingHighScore = existingEntry.highScore;
                existingFastestClearTime = existingEntry.fastestClearTime;
            } else {
                existingCleared = false;
                existingHighScore = 0;
                existingFastestClearTime = double.MaxValue;
            }

            // Only use the new highest score and clear time if the level was actually beaten,
            // otherwise the old records will remain
            if (cleared) {
                existingCleared = true;
                existingHighScore = Math.Max(existingHighScore, score);
                existingFastestClearTime = Math.Min(existingFastestClearTime, clearTime);
            }

            levelProgressDataEntries[level.levelId] = new LevelProgressData() {
                cleared = existingCleared,
                highScore = existingHighScore,
                fastestClearTime = existingFastestClearTime
            };
        }
    }

    [System.Serializable]
    public class LevelProgressData {
        /// <summary>
        /// Whether or not this level has been beaten
        /// </summary>
        public bool cleared;

        /// <summary>
        /// Highest score achieved on the level
        /// </summary>
        public int highScore;

        /// <summary>
        /// Fastest time the level was beaten in
        /// </summary>
        public double fastestClearTime;
    }
}