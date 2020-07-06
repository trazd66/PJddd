using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class CharacterControlAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    // Add fields to your component here. Remember that:
    //
    // * The purpose of this class is to store data for authoring purposes - it is not for use while the game is
    //   running.
    // 
    // * Traditional Unity serialization rules apply: fields must be public or marked with [SerializeField], and
    //   must be one of the supported types.
    //
    // For example,
    //    public float scale;
    
    

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new CharacterController.State {command = new UserCommand(), prevCommand = new UserCommand() });
        dstManager.AddComponentData(entity, new CharacterController.Stat { movementSpeed = 10 });

    }
}

public class CharacterController
{
    public struct Stat : IComponentData
    {
       public float movementSpeed;
    }


    public struct State : IComponentData
    {
        public UserCommand command;
        public UserCommand prevCommand;

        //TODO : add more later
        //[GhostDefaultField]
        public int resetCommandTick;
        //[GhostDefaultField(10)]
        public float resetCommandLookYaw;
        //[GhostDefaultField(10)]
        public float resetCommandLookPitch; // = 90;
        public int lastResetCommandTick;

        public bool IsButtonPressed(UserCommand.Button button)
        {
            return command.buttons.IsSet(button) && !prevCommand.buttons.IsSet(button);
        }

        public void ResetCommand(int tick, float lookYaw, float lookPitch)
        {
            resetCommandTick = tick;
            resetCommandLookYaw = lookYaw;
            resetCommandLookPitch = lookPitch;
        }

    }



}


