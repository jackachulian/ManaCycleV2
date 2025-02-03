using System;

namespace LevelSystem.Objectives {
    [System.Serializable]
    public class ScoreObjective : LevelObjective
    {
        public int scoreRequirement;

        public override string GetDetailsString()
        {
            return string.Format("Score {0} points", scoreRequirement);
        }

        public override string GetProgressString(Board board)
        {
            return string.Format("Score {0}/{1} points", board.scoreManager.score, scoreRequirement);
        }

        public override float GetProgress(Board board)
        {
            return (float)board.scoreManager.score / scoreRequirement;
        }


        public override void ListenToBoard(Board board)
        {
            board.scoreManager.onScoreChanged += OnScoreChanged;
        }

        public override void StopListeningToBoard(Board board)
        {
            board.scoreManager.onScoreChanged -= OnScoreChanged;
        }

        public void OnScoreChanged(int score) {
            NotifyConditionUpdated();
        }
    }
}