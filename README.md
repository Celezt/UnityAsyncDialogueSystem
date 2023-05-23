# Unity Async Dialogue System

Unity Asynchronous Dialogue System is a dialogue system for unity that tries to solve non-sequence-based dialogue situations. It allows for multiple activations of dialogues in parallel. It also has support for traditional sequence-based conversations.

It uses [Unity Timeline](https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/index.html) and [Unity GraphView API](https://docs.unity3d.com/ScriptReference/Experimental.GraphView.GraphView.html) to display and create dialogues. GraphView is Unity's API for creating custom virtual graphs. Unity Asynchronous Dialogue System has a custom file type, '.dialoguegraph', which is under the hood a JSON file; this is assignable as a reference of type 'Dialogue', which contains a runtime graph.

## Dialogue Graph

![Graph](https://github.com/Celezt/UnityAsyncDialogueSystem/assets/59172226/33773da1-1278-45b6-aa50-b9bbd3b08cc6)

'.dialoguegraph' is created from the dialogue window. It has similar features to Shader Graph and VFX Graph. You build the dialogue using nodes. There are different types of nodes: basic, behaviour, process and connection nodes. It also supports properties that allow the user to affect the dialogue from code. These properties are accessible from the property blackboard.

![Blackboard](https://github.com/Celezt/UnityAsyncDialogueSystem/assets/59172226/66c7cefc-105f-42b7-afe7-6b96bef72797)

* **Behaviour nodes** are logical nodes that execute or change the dialogue. Dialogue Nodes are a type of behaviour. It contains the text and if it should run/when to run.

* **Process nodes** are nodes that process a value or values to something else. Condition Node is one example that compares two values and chooses one of them based on a condition.

* **Connection nodes** are nodes that handle how the dialogue is accessed. Marker Node is a connection node that can be subscribed to in runtime that calls events from the dialogue graph. It is also used as output when reaching the end of a dialogue. 

* **Basic nodes** are constant types such as int, float and bool. Basic nodes are helpful in conjunction with properties to create conditions.

## Parallel Dialogue

![image](https://github.com/Celezt/UnityAsyncDialogueSystem/assets/59172226/ed97f2a9-cde5-4884-81ba-dd9839a469e0)

Achieving parallelism in dialogue in the current version requires the Blend Node. This system, unfortunately, limits the number of dialogues simultaneously to two and can only be achieved by blending two dialogues into each other. The runtime implementation with the timeline, on the other hand, supports unlimited parallelism. The restriction is currently only an expression limitation on how to write parallel code in a graph.

## Runtime Dialogue

![image](https://github.com/Celezt/UnityAsyncDialogueSystem/assets/59172226/f9eaa42b-49b0-4b74-94d3-438e97762e29)

The dialogue file converts to a runtime graph. It differentiates from the one used in the editor and contains only the barebone expression on what runtime type it will create and the data. The runtime graph creates a timeline on demand. The current version of Unity Timeline does not allow adding needed clips to a timeline without rebuilding the whole timeline. It will currently build for one possible path. And when changing to another, it will generate a new timeline for that path.

![image](https://github.com/Celezt/UnityAsyncDialogueSystem/assets/59172226/8c46d1ab-69e8-4c13-b3ce-0ea2826b3c6a)

Processors generate scriptable objects. The reason is the same as creating a timeline; it allows for broader support and debugging capabilities. 

Both processors and clips support manual placement. It is useful when creating a cutscene but wants to use the same system as in dialogues without touching the dialogue graph.

## Dialogue System

A dialogue system is a scriptable object created from right-clicking in the assets Create/Dialogue/Dialogue System. There is no game object needed in every scene. It will automatically generate necessary components on runtime, such as the PlayableDirector. To use the dialogue system, use the scriptable object as a reference in the code where methods are accessed. Connecting buttons to actions is also done from the dialogue system.

Action Override Settings in the inspector of the dialogue system allow for custom behaviour of each action button. It is possible to create custom fade curves for how the button should be displayed when active.

## Install

To install the plugin, clone it and import it into the project as an asset or package, or use Unity Package Manager and click "Add package from git URL..." and add https://github.com/Celezt/UnityAsyncDialogueSystem.git.
