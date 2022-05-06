using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateAssetMenu(fileName = "Condition Asset", menuName = "Dialogue/Assets/Condition Asset")]
    public class ConditionAsset : NodeAsset
    {
        public enum Comparisons
        {
            Equal,
            NotEqual,
            Less,
            LessOrEqual,
            Greater,
            GreaterOrEqual,
        }

        private void Awake()
        {
            InputCount = 2;
        }

        public override object Process(object[] inputs, int outputIndex)
        {
            Comparisons comparison = (Comparisons)NodeValues["currentComparison"];

            return CompareValues(comparison, (float)inputs[0], (float)inputs[1]);
        }

        private static bool CompareValues(Comparisons comparison, float first, float second) => comparison switch
        {
            Comparisons.Equal => first == second,
            Comparisons.NotEqual => first != second,
            Comparisons.Less => first < second,
            Comparisons.LessOrEqual => first <= second,
            Comparisons.Greater => first > second,
            Comparisons.GreaterOrEqual => first >= second,
            _ => throw new ArgumentException("", comparison.ToString()),
        };
    }
}
