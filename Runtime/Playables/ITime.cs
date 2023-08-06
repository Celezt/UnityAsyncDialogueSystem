using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public interface ITime
    {
        public AnimationCurve VisibilityCurve { get; }
        public float StartOffset { get; set; }
        public float EndOffset { get; set; }
        public float VisibilityInterval { get; }
        public float Interval { get; }
    }
}
