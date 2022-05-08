using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public class DSPort
    {
        public IEnumerable<DSEdge> Connections => _edges;
        public DSNode Node => _node;
        public Direction PortDirection => _direction;

        private List<DSEdge> _edges = new List<DSEdge>();

        private DSNode _node;
        private Direction _direction;

        public enum Direction
        {
            Input,
            Output,
        }

        public DSPort(DSNode parent, Direction direction)
        {
            _node = parent;
            _direction = direction;
        }

        /// <summary>
        /// Creates an edge between this port and the 'other' port.
        /// </summary>
        /// <param name="other">Other port to connect to.</param>
        /// <returns>Newly created edge.</returns>
        public DSEdge ConnectTo(DSPort other)
        {
            if (other._direction == _direction)
                return null;

            DSEdge edge = new DSEdge();
            _edges.Add(edge);
            other._edges.Add(edge);
            edge.Input = _direction == Direction.Input ? this : other;
            edge.Output = _direction == Direction.Output ? this : other;

            return edge;
        }
    }
}
