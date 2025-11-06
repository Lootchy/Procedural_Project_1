using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/NoiseProcedural")]
public class NoiseProcedural : ProceduralGenerationMethod
{
    FastNoiseLite noise = new FastNoiseLite();

    private GridObjectTemplate groundTemplate;
    private GridObjectTemplate waterTemplate;
    private GridObjectTemplate rockTemplate;
    private GridObjectTemplate sandTemplate;

    [Header("Height")]
    [SerializeField, UnityEngine.Range(-1, 1)] private float waterHeight = -0.2f;
    [SerializeField, UnityEngine.Range(-1, 1)] private float sandHeight = 0.0f;
    [SerializeField, UnityEngine.Range(-1, 1)] private float groundHeight = 0.4f;
    [SerializeField, UnityEngine.Range(-1, 1)] private float rockHeight = 0.7f;

    [Header("Settings")]
    [SerializeField, UnityEngine.Range(0f, 0.1f)] private float frenquency = 0.01f;
    [SerializeField, UnityEngine.Range(0f, 0.5f)] private float fractalGain = 0.7f;
    [SerializeField, UnityEngine.Range(-10, 10)] private int fractalOctave = 5;
    [SerializeField, UnityEngine.Range(1.0f, 4.0f)] private float fractalLacunarity = 2.0f;
    [SerializeField] private float magnitude = 0.7f;

    [SerializeField] private FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2;
    [SerializeField] private FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.FBm;
    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
        waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water");
        rockTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Rock");
        sandTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Sand");

        noise.SetSeed(GridGenerator.Seed);
        noise.SetFrequency(frenquency);
        noise.SetFractalType(fractalType);
        noise.SetFractalGain(fractalGain);
        noise.SetNoiseType(noiseType);
        noise.SetFractalOctaves(fractalOctave);
        noise.SetFractalLacunarity(fractalLacunarity);

        float[,] noiseData = new float[Grid.Width, Grid.Lenght];

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Lenght; y++)
            {
                noiseData[x, y] = noise.GetNoise(x, y);
                float noiseValue = noiseData[x, y];
                Grid.GetCellByCoordinates(x, y, out Cell chosenCell);
                if (noiseValue < waterHeight)
                {
                    GridGenerator.AddGridObjectToCell(chosenCell, waterTemplate, true);
                }
                else if (noiseValue < sandHeight)
                {
                    GridGenerator.AddGridObjectToCell(chosenCell, sandTemplate, true);
                }
                else if (noiseValue < groundHeight)
                {
                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, true);
                }
                else if (noiseValue < rockHeight)
                {
                    GridGenerator.AddGridObjectToCell(chosenCell, rockTemplate, true);
                }
                else
                {
                    GridGenerator.AddGridObjectToCell(chosenCell, rockTemplate, true);
                }
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
    }
}
