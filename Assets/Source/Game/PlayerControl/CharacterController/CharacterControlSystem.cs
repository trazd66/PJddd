using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;


[DisableAutoCreation]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class CharacterControlSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Assign values to local variables captured in your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        float deltaTime = Time.DeltaTime;
        Vector2 dir = StaticInputController.getMovement();
        Entities.ForEach((ref Translation translation, ref Rotation rotation, ref CharacterController.State state, ref CharacterController.Stat stat) =>
        {
            //first update userCommand
            RotateNMove(dir, ref state, ref stat, deltaTime, ref translation, ref rotation);

        }).WithoutBurst().Run();
    }

    public static void updateUserCmd()
    {

    }

    private void RotateNMove(Vector2 direction, 
        ref CharacterController.State state,
        ref CharacterController.Stat stat,
        float deltaTime, 
        ref Translation translation, ref Rotation rot)
    {
        if (direction.sqrMagnitude < 0.01)
            return;
        var scaledMoveSpeed = stat.movementSpeed * deltaTime;
        // For simplicity's sake, we just keep movement in a single plane here. Rotate
        // direction according to world Y rotation of player.
        var move = Quaternion.Euler(0, translation.Value.y, 0) * new Vector3(direction.x, 0, direction.y);
/*        rot.Value.value.y += direction.x;
        rot.Value.value.x = Mathf.Clamp(rot.Value.value.x - direction.y, -89, 89);
*/        translation.Value += (float3)move * scaledMoveSpeed;
    }
/*
    private void Look(Vector2 rotate)
    {
        if (rotate.sqrMagnitude < 0.01)
            return;
        var scaledRotateSpeed = rotateSpeed * Time.deltaTime;
        m_Rotation.y += rotate.x * scaledRotateSpeed;
        m_Rotation.x = Mathf.Clamp(m_Rotation.x - rotate.y * scaledRotateSpeed, -89, 89);
        transform.localEulerAngles = m_Rotation;
    }
*/
}
