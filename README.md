# UnityAsyncDialogueSystem

Unity Asynchronous Dialogue System is a dialogue system for unity that tries to solve non-sequence-based dialogue situations. It allows for multiple activations of dialogues in parallel. It also has support for traditional sequence-based conversations.

It uses [Unity Timeline](https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/index.html) and [Unity GraphView API](https://docs.unity3d.com/ScriptReference/Experimental.GraphView.GraphView.html) to display and create dialogues. GraphView is Unity's API for creating custom virtual graphs containing nodes, the successor used in Shader Graph. Unity Asynchronous Dialogue System has a custom file type, '.dialoguegraph', similar to '.shadergraph'; this is assignable as a reference of type 'Dialogue', which contains a constructed graph.

![dialogue window](https://github.com/Celezt/UnityAsyncDialogueSystem/assets/59172226/0a0db00f-d3f5-4214-bb8e-aa2037b165d5)

'.dialoguegraph' is created from the dialogue window. It has similar properties to Shader Graph and VFX Graph. You build the dialogue using nodes. There are different types of nodes: behaviour, process and connection nodes. 

'.dialoguegraph' is a '.json', meaning it is possible to build by hand or by a third-party creator.
