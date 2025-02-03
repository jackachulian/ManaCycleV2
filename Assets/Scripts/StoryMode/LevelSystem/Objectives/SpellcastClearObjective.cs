using System;

namespace LevelSystem.Objectives {
    [System.Serializable]
    public class SpellcastClearObjective : LevelObjective
    {
        public int spellcastClearRequirement;

        public override string GetDetailsString()
        {
            return string.Format("Spellcast {0} times", spellcastClearRequirement);
        }

        public override string GetProgressString(Board board)
        {
            return string.Format("Spellcast {0}/{1} times", board.spellcastManager.spellcastClears, spellcastClearRequirement);
        }

        public override float GetProgress(Board board)
        {
            return (float)board.spellcastManager.spellcastClears / spellcastClearRequirement;
        }


        public override void ListenToBoard(Board board)
        {
            board.spellcastManager.onSpellcastClear += OnSpellcastClear;
        }

        public override void StopListeningToBoard(Board board)
        {
            board.spellcastManager.onSpellcastClear -= OnSpellcastClear;
        }

        public void OnSpellcastClear(Board board) {
            NotifyConditionUpdated();
        }
    }
}