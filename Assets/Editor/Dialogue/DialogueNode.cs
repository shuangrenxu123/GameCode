using UnityEditor.Experimental.GraphView;

namespace DialogueEdtior
{
    public class DialogueNode : Node
    {
        public string GUID;
        public string DialogueText;
        public bool EntryPoint = false;
    }
}