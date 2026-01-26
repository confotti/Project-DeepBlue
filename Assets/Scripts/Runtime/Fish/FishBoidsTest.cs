using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class FishBoidsTest : MonoBehaviour
{
    public int FishCount = 2000;
    public float CellSize = 2f;

    [SerializeField] private float _fishMaxSpeed = 15f;
    [SerializeField] private float _neighborRadius = 10;
    [SerializeField] private float _separationWeight = 1.5f;
    [SerializeField] private float _alignmentWeight = 1f;
    [SerializeField] private float _cohesionWeight = 1f;
    [SerializeField] private float _playerWeight = 1f;

    [SerializeField] private float _spawnRadius = 25f;
    [SerializeField] private Vector3 _spawnCenter;

    [SerializeField] private Mesh _fishMesh;
    [SerializeField] private Material _fishMaterial;

    private Matrix4x4[] _matrices;

    NativeArray<float3> positions;
    NativeArray<float3> velocities;
    NativeArray<float3> accelerations;
    NativeParallelMultiHashMap<int, int> spatialMap;

    void Start()
    {
        _matrices = new Matrix4x4[FishCount];
        
        positions = new NativeArray<float3>(FishCount, Allocator.Persistent);
        velocities = new NativeArray<float3>(FishCount, Allocator.Persistent);
        accelerations = new NativeArray<float3>(FishCount, Allocator.Persistent);
        spatialMap = new NativeParallelMultiHashMap<int, int>(FishCount, Allocator.Persistent);

        SpawnFishSphere();
    }

    void Update()
    {
        var buildJob = new BuildSpatialMapJob
        {
            Positions = positions,
            Map = spatialMap.AsParallelWriter(),
            CellSize = CellSize
        }.Schedule(FishCount, 64);

        var steerJob = new BoidsSteeringJob
        {
            Positions = positions,
            Velocities = velocities,
            Accelerations = accelerations,
            Map = spatialMap,
            CellSize = CellSize,
            NeighborRadius = _neighborRadius,
            SeparationWeight = _separationWeight,
            AlignmentWeight = _alignmentWeight,
            CohesionWeight = _cohesionWeight,
            PlayerPos = PlayerMovement.Instance.transform.position,
            PlayerWeight = _playerWeight
        }.Schedule(FishCount, 64, buildJob);

        var integrateJob = new IntegrateJob
        {
            Positions = positions,
            Velocities = velocities,
            Accelerations = accelerations,
            DeltaTime = Time.deltaTime,
            MaxSpeed = _fishMaxSpeed
        }.Schedule(FishCount, 64, steerJob);

        integrateJob.Complete();

        spatialMap.Clear();

        for (int i = 0; i < FishCount; i++)
        {
            float3 pos = positions[i];
            float3 vel = velocities[i];

            Quaternion rot = Quaternion.LookRotation(
                math.normalizesafe(vel),
                Vector3.up
            );

            _matrices[i] = Matrix4x4.TRS(
                pos,
                rot,
                Vector3.one * 0.5f
            );
        }

        Graphics.DrawMeshInstanced(
        _fishMesh,
        0,
        _fishMaterial,
        _matrices,
        FishCount
    );

        // Apply to transforms (or GPU instancing)
    }

    void OnDestroy()
    {
        positions.Dispose();
        velocities.Dispose();
        accelerations.Dispose();
        spatialMap.Dispose();
    }

    void SpawnFishSphere()
    {
        Unity.Mathematics.Random rng =
        new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

        for (int i = 0; i < FishCount; i++)
        {
            float3 dir = rng.NextFloat3Direction();
            float radius = math.pow(rng.NextFloat(), 1f / 3f) * _spawnRadius;

            positions[i] = _spawnCenter + (Vector3)dir * radius;
            velocities[i] = rng.NextFloat3Direction() * rng.NextFloat(1f, 3f);
            accelerations[i] = float3.zero;
        }
    }

    public static int Hash(float3 position, float cellSize)
    {
        int3 cell = (int3)math.floor(position / cellSize);
        return cell.x * 73856093 ^ cell.y * 19349663 ^ cell.z * 83492791;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_spawnCenter, _spawnRadius);
    }
}

[BurstCompile]
struct BuildSpatialMapJob : IJobParallelFor
{
    [InspectorReadOnly] public NativeArray<float3> Positions;
    public NativeParallelMultiHashMap<int, int>.ParallelWriter Map;
    public float CellSize;

    public void Execute(int index)
    {
        int hash = FishBoidsTest.Hash(Positions[index], CellSize);
        Map.Add(hash, index);
    }
}

[BurstCompile]
struct BoidsSteeringJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<float3> Velocities;
    public NativeArray<float3> Accelerations;

    [ReadOnly] public NativeParallelMultiHashMap<int, int> Map;
    public float CellSize;
    public float NeighborRadius;
    public float SeparationWeight;
    public float AlignmentWeight;
    public float CohesionWeight;
    public float PlayerWeight;
    public float3 PlayerPos;

    public void Execute(int index)
    {
        float3 pos = Positions[index];
        float3 vel = Velocities[index];

        float3 separation = 0;
        float3 alignment = 0;
        float3 cohesion = 0;
        float3 playerScare = 0;
        int count = 0;

        int hash = FishBoidsTest.Hash(pos, CellSize);

         // 3×3×3 neighbor cell search
        for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 1; y++)
        for (int z = -1; z <= 1; z++)
        {
            float3 offsetCell = new float3(x, y, z) * CellSize;
            int neighborHash = FishBoidsTest.Hash(pos + offsetCell, CellSize);

            NativeParallelMultiHashMapIterator<int> it;
            int other;

            if (Map.TryGetFirstValue(neighborHash, out other, out it))
            {
                do
                {
                    if (other == index)
                        continue;

                    float3 offset = Positions[other] - pos;
                    float dist = math.length(offset);

                    if (dist < NeighborRadius)
                    {
                        separation -= offset / math.max(dist, 0.001f);
                        alignment += Velocities[other];
                        cohesion += Positions[other];
                        count++;
                    }
                }
                while (Map.TryGetNextValue(out other, ref it));
            }
        }

        if (count > 0)
        {
            alignment = alignment / count - vel;
            cohesion = (cohesion / count) - pos;
        }
        
        float3 playerOffset = pos - PlayerPos;
        float playerDist = math.length(playerOffset);
        if(playerDist < NeighborRadius)
        {
            playerScare += playerOffset / math.max (playerDist, 0.001f);
        }

        Accelerations[index] =
            separation * SeparationWeight +
            alignment * AlignmentWeight +
            cohesion * CohesionWeight +
            playerScare * PlayerWeight;
    }
}

[BurstCompile]
struct IntegrateJob : IJobParallelFor
{
    public NativeArray<float3> Positions;
    public NativeArray<float3> Velocities;
    [ReadOnly] public NativeArray<float3> Accelerations;

    public float DeltaTime;
    public float MaxSpeed;

    public void Execute(int index)
    {
        float3 vel = Velocities[index] + Accelerations[index] * DeltaTime;
        vel = math.normalize(vel) * math.min(math.length(vel), MaxSpeed);

        Velocities[index] = vel;
        Positions[index] += vel * DeltaTime;
    }
}