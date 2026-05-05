using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FoodDetect", story: "Check [VisionSystem] and detect [FoodTarget] in vision zone", category: "Action", id: "095db0f5b924feb4ee7d4850a319502f")]
public partial class FoodDetectAction : Action
{
    [SerializeReference] public BlackboardVariable<AdaptiveVisionSystem> VisionSystem;
    [SerializeReference] public BlackboardVariable<string> FoodTarget;
    public string foodTag = "Food";      // String variable for food tag
    protected override Status OnUpdate()
    {
        bool foodDetected = VisionSystem.Value.CanSeeFood();
        return foodDetected ? Status.Success : Status.Failure;
    }

}

