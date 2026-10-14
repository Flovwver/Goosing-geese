# Code style

In C# code, separate control-flow statements (`if`, `else`, `for`, `foreach`, `while`, `do`, `switch`, and similar constructs) from adjacent ordinary statements with a blank line before and after the whole construct.

Do not insert blank lines between a control-flow construct and the opening or closing boundary of its enclosing block. This applies to method bodies, nested control-flow bodies, and `case`/`default` sections. Do not separate connected branches such as `} else`, `} catch`, `} finally`, or the `while` part of `do ... while` with a blank line. A surrounding block boundary takes precedence over the separation rule.

Example:

```csharp
public void UpdateMovement()
{
    float speed = GetCurrentSpeed();

    if (speed > 0f)
    {
        ApplyMovement();

        if (isTurning)
        {
            ApplyRotation();
        }
    }

    UpdateSpeedometer();

    foreach (var wheel in wheels)
    {
        if (wheel.IsGrounded)
        {
            wheel.UpdateGrip();
        }
    }

    SaveState();
}
```

## C# member ordering

Follow the user's StyleCop-based member ordering (SA1201, SA1202, SA1203, SA1204, SA1214, SA1215) when adding or editing C# types.

Within a class, struct, or interface, use this order:

1. Constant fields
2. Fields
3. Constructors
4. Finalizers (destructors)
5. Delegates
6. Events
7. Enums
8. Interfaces (nested interface declarations; group interface implementation members in a partial type when necessary)
9. Properties
10. Indexers
11. Methods
12. Structs
13. Classes

Within each member-kind group, order by accessibility first:

1. public
2. internal
3. protected internal
4. protected
5. private protected
6. private

Within each accessibility group, static members precede instance members. Within each static/instance field group, readonly fields precede mutable fields. Constants remain in the constant-field group.

User-defined exception to method ordering: Unity lifecycle callbacks (Awake, OnEnable, Start, Update, FixedUpdate, LateUpdate, OnDisable, OnDestroy, OnValidate, OnDrawGizmos, OnApplicationFocus, OnApplicationPause, and similar Unity callbacks) always come first within the methods group, before all ordinary methods regardless of accessibility or static/instance status. Thus a private Awake or FixedUpdate comes before a public ordinary method. Keep Unity callbacks together and order them by their lifecycle phases, rather than accessibility or alphabetical order. The core source-order convention is Awake -> OnEnable -> Start -> FixedUpdate -> Update -> LateUpdate -> OnDisable -> OnDestroy. Place physics callbacks (OnCollision*/OnTrigger*) after FixedUpdate and before Update; place rendering callbacks after LateUpdate. Place editor initialization callbacks Reset and OnValidate before Awake, and editor drawing callbacks OnDrawGizmos/OnDrawGizmosSelected after the runtime update/rendering callbacks. Group application-state callbacks OnApplicationFocus/OnApplicationPause after update/rendering/editor drawing callbacks, and OnApplicationQuit before teardown callbacks OnDisable/OnDestroy. For additional callbacks, consult the Unity event-function execution-order documentation and place them in the corresponding phase. This is a source-formatting convention, not a guarantee of one universal runtime sequence: FixedUpdate can run zero or multiple times per frame, and editor/application-state events do not have a fixed position in every frame. After the Unity callback group, order ordinary methods by the accessibility and static/instance rules above. Other member-kind ordering remains unchanged.

Preserve relative order among members with equal ordering keys unless another change requires reordering. Do not reorder serialized fields across intentional Inspector Header groups without preserving the Inspector grouping. If interface members need to stay together despite their different kinds, use a partial class rather than silently ignoring the ordering rules.

Reference: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1201.md

Unity lifecycle reference: https://docs.unity3d.com/6000.2/Documentation/Manual/execution-order.html (user-provided saved manual).
