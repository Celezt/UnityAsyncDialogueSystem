using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Events;
using System;

namespace Celezt.DialogueSystem
{
    public class SetInterpreter : AssetInterpreter
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

            SignalEmitter emitter = signalTrack.CreateMarker<SignalEmitter>(time);
            emitter.asset = ScriptableObject.CreateInstance<SignalAsset>();

            string propertyName = (string)currentNode.Values["_propertyName"];
            string assignOption = (string)currentNode.Values["_assignOption"];

            UnityEvent newEvent = new UnityEvent();
            newEvent.AddListener(() =>
            {
                if (propertyName != "None")
                {
                    if (system.ExposedProperties.TryGetValue(propertyName, out ExposedProperty property))
                    {
                        if (currentNode.Inputs.TryGetValue(1, out DSPort setInputPort))
                        {
                            DSNode setNode = setInputPort.Connections.First().Output.Node;
                            if (setNode.TryGetAllProcessors(out var processors))
                            {
                                object value = processors.First().GetValue(0);
                                switch (assignOption)
                                {
                                    case "Assign":
                                        property.Value = value;
                                        break;
                                    case "PlusAssign":
                                        try
                                        {
                                            property.Value =  Convert.ToSingle(property.Value) + Convert.ToSingle(value);
                                        }
                                        catch(Exception e)
                                        {
                                            Debug.LogError(e.Message);
                                        }
                                        break;
                                    case "MinusAssign":
                                        try
                                        {
                                            property.Value = Convert.ToSingle(property.Value) - Convert.ToSingle(value);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.LogError(e.Message);
                                        }
                                        break;
                                    case "MultiplyAssign":
                                        try
                                        {
                                            property.Value = Convert.ToSingle(property.Value) * Convert.ToSingle(value);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.LogError(e.Message);
                                        }
                                        break;
                                    case "DivideAssign":
                                        try
                                        {
                                            property.Value = Convert.ToSingle(property.Value) / Convert.ToSingle(value);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.LogError(e.Message);
                                        }
                                        break;
                                    case "ModAssign":
                                        try
                                        {
                                            property.Value = Convert.ToSingle(property.Value) % Convert.ToSingle(value);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.LogError(e.Message);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
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
