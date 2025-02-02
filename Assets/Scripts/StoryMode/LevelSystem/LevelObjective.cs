using UnityEngine;

namespace LevelSystem.Objectives {
    [System.Serializable]
    public abstract class LevelObjective {
        public abstract string GetInstructionString(Board board);
        public abstract bool IsConditionMet(Board board);
    }
}