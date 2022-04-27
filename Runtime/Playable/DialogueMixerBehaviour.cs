using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    public class DialogueMixerBehaviour : PlayableBehaviour
    {
        private List<BehaviourData> _oldBehaviours = new List<BehaviourData>();

        private PlayableDirector _director;

        private struct BehaviourData
        {
            public Playable Playable;
            public DialogueBehaviour Behaviour;
        }

        public override void OnPlayableCreate(Playable playable)
        {
            _director = playable.GetGraph().GetResolver() as PlayableDirector;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            ProcessClips(playable, info, playerData);
        }

        private void ProcessClips(Playable playable, FrameData info, object playerData)
        {
            List<BehaviourData> currentBehaviours = new List<BehaviourData>();

            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                if (playable.GetInputWeight(i) > 0.0f)
                {
                    Playable currentPlayable = playable.GetInput(i);

                    if (!currentPlayable.GetPlayableType().IsSubclassOf(typeof(DialogueBehaviour)))
                        return;

                    DialogueBehaviour input = ((ScriptPlayable<DialogueBehaviour>)currentPlayable).GetBehaviour();

                    if (input != null)
                    {
                        input.ProcessMixerFrame(_director, currentPlayable, info, playerData);
                        currentBehaviours.Add(new BehaviourData
                        {
                            Playable = currentPlayable,
                            Behaviour = input,
                        });
                    }
                }
            }

            // Remove until only clips no longer inside the scope is left.
            for (int i = 0; i < currentBehaviours.Count; i++)
                _oldBehaviours.Remove(currentBehaviours[i]);

            // Calls after exiting a clip.
            for (int i = 0; i < _oldBehaviours.Count; i++)
                _oldBehaviours[i].Behaviour.PostMixerFrame(_director, _oldBehaviours[i].Playable, info, playerData);

            _oldBehaviours = currentBehaviours;
        }
    }
}
