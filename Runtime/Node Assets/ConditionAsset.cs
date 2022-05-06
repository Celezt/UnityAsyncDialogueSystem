using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateAssetMenu(fileName = "Condition Asset", menuName = "Dialogue/Assets/Condition Asset")]
    public class ConditionAsset : NodeAsset
    {
        public Comparisons _comparison = Comparisons.Equal;

        public enum Comparisons
        {
            Equal,
            NotEqual,
            Less,
            LessOrEqual,
            Greater,
            GreaterOrEqual,
        }

        protected override void OnCreateAsset(IReadOnlyDictionary<string, object> values)
        {
            InputCount = 2;

            if (values != null)
                _comparison = (Comparisons)values["currentComparison"];
        }

        protected override object Process(object[] inputs, int outputIndex)
        {
            return CompareValues(_comparison, (float)inputs[0], (float)inputs[1]);
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
