using System.Collections;
using System.Linq;
using UnityEngine;
using System;

namespace StoryMode.ConvoSystem
{
    public abstract class ConvoUI : MonoBehaviour
    {
        /// <summary>
        /// Interval of time between writing each character
        /// </summary>
        [SerializeField] float glyphInterval;

        /// <summary>
        /// Interval of time to wait after writing punctuation
        /// </summary>
        [SerializeField] float puncuationInterval;

        /// <summary>
        /// Convo this UI is displaying
        /// </summary>
        private Convo currentConvo;

        /// <summary>
        /// Current line in the current convo
        /// </summary>
        private ConvoLine currentLine;

        /// <summary>
        /// Current line index of the current convo
        /// </summary>
        private int currentLineIndex;

        /// <summary>
        /// Index of the current glyph in line
        /// </summary>
        private int currentGlyphIndex;

        /// <summary>
        /// TMP asset that the text gets written to 
        /// (Maybe this should be moved out of abstract class?)
        /// </summary>
        [SerializeField] private TMPro.TextMeshPro displayText;

        private bool inConvo = false;
        private static char[] puncuation = {'.', ',', ':', '!', '?', ';'};

        public bool LineEnded {get => currentGlyphIndex == currentConvo.lines[currentLineIndex].convoText.Length - 1;}

        private WaitForSeconds WaitForGlyph;
        private WaitForSeconds WaitForPunctuation;

        void Start()
        {
            WaitForGlyph = new WaitForSeconds(glyphInterval);
            WaitForPunctuation = new WaitForSeconds(puncuationInterval);
        }

        public virtual void StartConvo(Convo c)
        {
            currentConvo = c;
            inConvo = true;
            currentLineIndex = 0;
            StartCoroutine(StartLine(currentLineIndex));
        }

        public virtual IEnumerator StartLine(int lineIndex)
        {
            currentLine = currentConvo.lines[lineIndex];
            displayText.text = currentConvo.lines[lineIndex].convoText;

            for (currentGlyphIndex = 0; currentGlyphIndex < currentLine.convoText.Length; currentGlyphIndex++)
            {
                WriteGlyph(currentGlyphIndex);
                yield return puncuation.Contains(currentLine.convoText[currentGlyphIndex]) 
                    ? WaitForPunctuation : WaitForGlyph;
            }
            OnEndLine();
        }

        public virtual void SkipLine()
        {
            WriteGlyph(currentLine.convoText.Length - 1);
            OnEndLine();
        }

        public virtual void OnEndLine(){}

        public virtual void NextLine()
        {
            currentLineIndex++;
            if (currentLineIndex >= currentConvo.lines.Length) EndConvo();
            else
                StartCoroutine(StartLine(currentLineIndex));
        }

        public virtual void WriteGlyph(int glyphIndex)
        {
            displayText.maxVisibleCharacters = glyphIndex + 1;
        }

        public virtual void EndConvo()
        {
            inConvo = false;
        }

        public virtual void HandleForwardInput()
        {
            if (LineEnded) 
                NextLine();
            else 
                SkipLine();
        }

        public virtual void HandleBackwardInput()
        {
            currentLineIndex = Math.Max(0, currentLineIndex - 1);
            StartCoroutine(StartLine(currentLineIndex));
        }
    }
}