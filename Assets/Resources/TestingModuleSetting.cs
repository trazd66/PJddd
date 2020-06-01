using UnityEngine;

[CreateAssetMenu(fileName = "TestingModuleSetting", menuName = "ScriptableObjects/TestingModuleSetting")]
public class TestingModuleSetting : ScriptableObject
{
    public WeakAssetReference test0Prefab;
    public WeakAssetReference test1Prefab;
}
