using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Replay {
    [System.Serializable]
    public class ReplayData {
        public BattleData battleData;

        /// <summary>
        /// Data about all players to reconstruct the players in the battle.
        /// </summary>
        public ReplayPlayer[] replayPlayers;

        /// <summary>
        /// The times that events are replayed at. Parallel with events array
        /// </summary>
        public float[] eventTiming;

        /// <summary>
        /// The events to be replayed. Parallel with eventTiming array.
        /// </summary>
        public Replayable[] events;

        public ReplayData(GameManager gameManager) {
            battleData = GameManager.Instance.battleData;
        }
    }


    [System.Serializable]
    public struct ReplayPlayer {
        public string battlerId;
        public string username;
        public bool isCpu;
    }

    
    public interface Replayable {
        void Replay(BattleManager battleManager);
    }
}