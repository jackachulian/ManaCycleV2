using System;

namespace LevelSystem.Objectives {
    [System.Serializable]
    public class ScoreObjective : LevelObjective
    {
        public int scoreRequirement;

        public override string GetInstructionString(Board board)
        {
            return string.Format("Score {0}/{1} points", board.scoreManager.score, scoreRequirement);
        }

        public override bool IsConditionMet(Board board)
        {
            return board.scoreManager.score >= scoreRequirement;
        }
    }
}