using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public class NodeErrorData : MonoBehaviour
    {
        public DSErrorData ErrorData { get; set; }
        public List<Node> Nodes { get; set; }

        public NodeErrorData()
        {
            ErrorData = new DSErrorData();
            Nodes = new List<Node>();
        }
    }
}
