using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

public static class SharedData
{
    public static readonly SharedStatic<Entity> singtonEnitty = SharedStatic<Entity>.GetOrCreate<keyClass1>();
    public static readonly SharedStatic<GameSharedData> gameSharedData = SharedStatic<GameSharedData>.GetOrCreate<GameSharedData>();
    public static readonly SharedStatic<float2> playerPos = SharedStatic<float2>.GetOrCreate<keyClass2>();

    public struct keyClass1 { }
    public struct keyClass2 { }
}

public struct GameSharedData
{
    public int deadCounter;
    public float spawnInterval;
    public int spawnCount;
    public bool playHitAudio;
    public double playHitAudioTime;
}