using System;

namespace LevelSystem.Objectives {
    [System.Serializable]
    public class SpellcastClearObjective : LevelObjective
    {
        public int spellcastClearRequirement;

        public override string GetInstructionString(Board board)
        {
            return string.Format("Spellcast {0}/{1} times", board.spellcastManager.spellcastClears, spellcastClearRequirement);
        }

        public override bool IsConditionMet(Board board)
        {
            return board.spellcastManager.spellcastClears >= spellcastClearRequirement;
        }
    }
}