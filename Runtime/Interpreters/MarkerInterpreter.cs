using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class MarkerInterpreter : AssetInterpreter
    {
        protected override void OnInterpret(DSNode currentNode, DSNode previousNode, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            SignalTrack signalTrack = timeline.FindOrCreate<SignalTrack>();

            // Get receiver;
            SignalReceiver receiever = null;
            {
                if (system.Director.GetGenericBinding(signalTrack) is SignalReceiver foundReceiever)
                    receiever = foundReceiever;
                else if (system.Director.TryGetComponent(out receiever))
                {
                    system.Director.SetGenericBinding(signalTrack, receiever);
                }
                else
                {
                    receiever = system.Director.gameObject.AddComponent<SignalReceiver>();
                    system.Director.SetGenericBinding(signalTrack, receiever);
                }
            }

            double time = timeline.GetOutputTracks().Max(x => x.end);

            string id = (string)currentNode.Values["_id"];

            SignalEmitter emitter = signalTrack.CreateMarker<SignalEmitter>(time);
            emitter.asset = ScriptableObject.CreateInstance<SignalAsset>();

            UnityEvent newEvent = new UnityEvent();
            newEvent.AddListener(() =>
            {
                system.InvokeEvent(id);
            });

            receiever.AddReaction(emitter.asset, newEvent);
        }

        protected override void OnNext(DSNode currentNode, DSNode previousNode, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            DSNode nextNode = null;
            if (currentNode.Outputs.TryGetValue(0, out DSPort outPort))
                nextNode = outPort.Connections.First().Input.Node;

            if (nextNode != null)
            {
                if (nextNode.TryGetInterpreter(out var interpreter))
                {
                    interpreter.OnInterpret(system, this);
                    interpreter.OnNext(system);
                }
            }
        }
    }
}
