using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Replay;
using UnityEngine;

namespace Replay {
    public class ReplayManager : MonoBehaviour {
        public static readonly bool useBinaryMode = false;
        public ReplayData replayData;
        public List<ReplayPieceEvent> pieceEvents;
        public List<ReplaySpellcastEvent> spellcastEvents;

        void Awake() {
            if (!BattleManager.Instance) {
                Debug.LogError("Replay manager needs a battlemanager to record!");
                return;
            }

            BattleManager.Instance.onBattleInitialized += StartRecordingEvents;
        }

        public void StartRecordingEvents() {
            replayData = new ReplayData(GameManager.Instance);

            pieceEvents = new List<ReplayPieceEvent>();
            spellcastEvents = new List<ReplaySpellcastEvent>();

            for (int i = 0; i < GameManager.Instance.playerManager.players.Count; i++) {
                Board board = BattleManager.Instance.GetBoardByIndex(i);

                board.pieceManager.onPieceMoved += OnPieceMoved;
                board.pieceManager.onPiecePlaced += OnPiecePlaced;

                board.spellcastManager.onSpellcastStarted += OnSpellcastStarted;
                board.spellcastManager.onSpellcastClear += OnSpellcastClear;
                board.spellcastManager.onCascadeEnded += OnChainEnded;
                board.spellcastManager.onChainEnded += OnChainEnded;
            }

            BattleManager.Instance.onBattleEnded += SaveRecording;

            Debug.Log("Recording battle events!");
        }

        public void SaveRecording() {
            replayData.spellcastEvents = spellcastEvents.ToArray();
            replayData.pieceEvents = pieceEvents.ToArray();
            SaveToFile();
        }

        public void SaveToFile(string path = "replay.json") {
            string fullPath = Path.Combine(Application.persistentDataPath, path);
            if (useBinaryMode) {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    formatter.Serialize(stream, this);
                }
            } else {
                string json = JsonUtility.ToJson(this, true); // Convert to JSON string
                File.WriteAllText(fullPath, json);
            }
            
            Debug.Log("Saved replay data to "+fullPath);
        }

        public void LoadFromFile(string path = "replay.json") {
            string fullPath = Path.Combine(Application.persistentDataPath, path);

            if (useBinaryMode) {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    replayData = (ReplayData)formatter.Deserialize(stream);
                    Debug.Log($"");
                }
            } else {
                if (File.Exists(fullPath))
                {
                    string json = File.ReadAllText(fullPath);
                    JsonUtility.FromJsonOverwrite(json, this);
                }
                else
                {
                    Debug.LogError("File not found!");
                    return;
                }
            }

            Debug.Log($"Loaded replay data from: {fullPath}");
        }

        public void EvaluateSpellcastEvent(ReplaySpellcastEvent replayEvent) {
            Board board = BattleManager.Instance.GetBoardByIndex(replayEvent.boardIndex);

            switch (replayEvent.eventType) {
                case SpellcastEventType.START:
                    board.spellcastManager.StartSpellcast();
                    break;
                case SpellcastEventType.CLEAR:
                    board.spellcastManager.SpellcastClear();
                    break;
                case SpellcastEventType.CASCADE_END:
                    board.spellcastManager.EndCascade();
                    break;
                case SpellcastEventType.CHAIN_END:
                    board.spellcastManager.EndChain();
                    break;
            }
        }

        public void OnPieceMoved(Board board) {
            var ev = new ReplayPieceEvent(PieceEventType.PLACE, board.boardIndex, 
                board.pieceManager.currentPiece.position, board.pieceManager.currentPiece.rotation);
            pieceEvents.Add(ev);
        }

        public void OnPiecePlaced(Board board) {
            var ev = new ReplayPieceEvent(PieceEventType.PLACE, board.boardIndex, 
                board.pieceManager.currentPiece.position, board.pieceManager.currentPiece.rotation);
            pieceEvents.Add(ev);
        }

        public void OnSpellcastStarted(Board board) {
            var ev = new ReplaySpellcastEvent(SpellcastEventType.START, board.boardIndex);
            spellcastEvents.Add(ev);
        }
        public void OnSpellcastClear(Board board) {
            var ev = new ReplaySpellcastEvent(SpellcastEventType.CLEAR, board.boardIndex);
            spellcastEvents.Add(ev);
        }
        public void OnCascadeEnded(Board board) {
            var ev = new ReplaySpellcastEvent(SpellcastEventType.CASCADE_END, board.boardIndex);
            spellcastEvents.Add(ev);
        }
        public void OnChainEnded(Board board) {
            var ev = new ReplaySpellcastEvent(SpellcastEventType.CHAIN_END, board.boardIndex);
            spellcastEvents.Add(ev);
        }
    }
}