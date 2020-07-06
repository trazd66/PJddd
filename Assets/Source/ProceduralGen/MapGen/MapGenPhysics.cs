using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MapGen
{
	public class MapGenPhysics
	{

		//TODO : Remove these code once the math library is more complete
		unsafe static NativeArray<float3> GetNativeVertexArrays(Vector3[] vertexArray)
		{
			// create a destination NativeArray to hold the vertices
			NativeArray<float3> verts = new NativeArray<float3>(vertexArray.Length, Allocator.Persistent,
				NativeArrayOptions.UninitializedMemory);

			// pin the mesh's vertex buffer in place...
			fixed (void* vertexBufferPointer = vertexArray)
			{
				// ...and use memcpy to copy the Vector3[] into a NativeArray<float3> without casting. whould be fast!
				UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(verts),
					vertexBufferPointer, vertexArray.Length * (long)UnsafeUtility.SizeOf<float3>());
			}
			// we only hve to fix the .net array in place, the NativeArray is allocated in the C++ side of the engine and
			// wont move arround unexpectedly. We have a pointer to it not a reference! thats basically what fixed does,
			// we create a scope where its 'safe' to get a pointer and directly manipulate the array

			return verts;
		}

		unsafe static void SetNativeVertexArray(Vector3[] vertexArray, NativeArray<float3> vertexBuffer)
		{
			// pin the target vertex array and get a pointer to it
			fixed (void* vertexArrayPointer = vertexArray)
			{
				// memcopy the native array over the top
				UnsafeUtility.MemCpy(vertexArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(vertexBuffer), vertexArray.Length * (long)UnsafeUtility.SizeOf<float3>());
			}
		}

		unsafe static NativeArray<int3> GetNativeTriangleArrays(int[] tirangleArray)
		{
			// create a destination NativeArray to hold the vertices
			NativeArray<int3> trigs = new NativeArray<int3>(tirangleArray.Length/3, Allocator.Persistent,
				NativeArrayOptions.UninitializedMemory);

			// pin the mesh's vertex buffer in place...
			fixed (void* vertexBufferPointer = tirangleArray)
			{
				// ...and use memcpy to copy the Vector3[] into a NativeArray<float3> without casting. whould be fast!
				UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(trigs),
					vertexBufferPointer, tirangleArray.Length/3 * (long)UnsafeUtility.SizeOf<int3>());
			}
			// we only hve to fix the .net array in place, the NativeArray is allocated in the C++ side of the engine and
			// wont move arround unexpectedly. We have a pointer to it not a reference! thats basically what fixed does,
			// we create a scope where its 'safe' to get a pointer and directly manipulate the array

			return trigs;
		}




		public static BlobAssetReference<Unity.Physics.Collider> createMeshCollider(Vector3[] vertices, int[] triangles)
		{
			NativeArray<float3> vBuffer = GetNativeVertexArrays(vertices);
			NativeArray<int3> tBuffer = GetNativeTriangleArrays(triangles);

			BlobAssetReference<Unity.Physics.Collider> mCollider = Unity.Physics.MeshCollider.Create(vBuffer, tBuffer);

			vBuffer.Dispose();
			tBuffer.Dispose();

			return mCollider;
		}

		public static unsafe Entity CreatePhysicalMapEntity(EntityManager entityManager, RenderMesh renderMesh, float3 position, quaternion orientation,
			BlobAssetReference<Unity.Physics.Collider> collider)
		{
			ComponentType[] componentTypes = { typeof(RenderMesh), typeof(PhysicsCollider) };

			Entity entity = entityManager.CreateEntity(componentTypes);
			entityManager.AddSharedComponentData(entity, renderMesh);
			entityManager.AddComponentData(entity, new Translation { Value = position });
			entityManager.AddComponentData(entity, new Rotation { Value = orientation });
			entityManager.SetComponentData(entity, new PhysicsCollider { Value = collider });


			return entity;
		}
	}
}

