namespace StoryMode.ConvoSystem
{
    [System.Serializable]
    public class ConvoLine
    {
        public string convoText;
        public Actor[] actors;
        public int activeActorIndex;
    }
}