using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticInputController
{
    public static @InputManager inputManager { get; private set; }


    public static void init()
    {
        inputManager = new InputManager();
        inputManager.Gameplay.Movement.Enable();

    }
    public static InputManager getInputManager()
    {
        return inputManager;
    }

    public static Vector2 getMovement()
    {
        return inputManager.Gameplay.Movement.ReadValue<Vector2>();
    }
}
