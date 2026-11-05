using System;
using System.Reflection;
using GoosingGeese.Input;
using GoosingGeese.Racing;
using UnityEngine;
using UnityEngine.InputSystem;

public static class SceneWiringTests
{
    public static string Main()
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();
        var input = UnityEngine.Object.FindFirstObjectByType<ThrottleInput>();
        var keyboard = UnityEngine.Object.FindFirstObjectByType<KeyboardAccelerator>();
        var pedal = UnityEngine.Object.FindFirstObjectByType<PedalButton>(FindObjectsInactive.Include);
        Require(typeof(KeyboardAccelerator).GetField("_input", Flags).GetValue(keyboard) == input, "Keyboard must reference the scene throttle input.");
        Require(typeof(PedalButton).GetField("_input", Flags).GetValue(pedal) == input, "Pedal must reference the scene throttle input.");
        var action = (InputActionReference)typeof(KeyboardAccelerator).GetField("_accelerateAction", Flags).GetValue(keyboard);
        Require(action != null && action.action != null && action.action.enabled, "Keyboard action must be enabled.");

        try
        {
            race.Prepare();
            typeof(ThrottleInput).GetField("_hasFocus", Flags).SetValue(input, true);
            typeof(ThrottleInput).GetField("_isPaused", Flags).SetValue(input, false);
            typeof(KeyboardAccelerator).GetMethod("HandleAccelerationPerformed", Flags).Invoke(keyboard, new object[] { default(InputAction.CallbackContext) });
            Require(input.IsThrottlePressed, "Keyboard adapter must deliver throttle.");
            typeof(KeyboardAccelerator).GetMethod("HandleAccelerationCanceled", Flags).Invoke(keyboard, new object[] { default(InputAction.CallbackContext) });
            Require(input.IsThrottlePressed == false, "Keyboard adapter must release throttle.");
            pedal.OnPointerDown(null);
            Require(input.IsThrottlePressed, "Pedal adapter must deliver throttle.");
            pedal.OnPointerUp(null);
            Require(input.IsThrottlePressed == false, "Pedal adapter must release throttle.");
            return "PASS: serialized input wiring, enabled action, keyboard and pedal press/release.";
        }
        finally
        {
            input.ResetInput();
            race.Prepare();
        }
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
