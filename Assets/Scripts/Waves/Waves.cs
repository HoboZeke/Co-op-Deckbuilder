using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves : MonoBehaviour
{
    public int dimension = 10;
    public float uvScale;
    float minY, maxY;
    public float seed;

    MeshFilter meshFilter;
    Mesh mesh;

    public Octave[] octaves;
    public bool generateWaves, tryDeform;
    Vector3[] meshVerts;

    public bool steps;
    public float stepSize;

    public Vector2 animateDirection;
    public bool animate;
    public bool skipDeform;

    [Space(10)]
    [SerializeField] private string meshString;

    private void Start()
    {
        //Mesh Setup
        mesh = new Mesh();
        mesh.name = gameObject.name;

        if(meshString != "") {
            SetupFromString(meshString);
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            mesh.RecalculateNormals();
            return;
        }

        mesh.vertices = GenerateVerts();
        mesh.triangles = GenerateTries();
        GenerateWaves(false);
        StartCoroutine(Deform());
        mesh.RecalculateBounds();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        mesh.RecalculateNormals();
        meshString = Stringify();
    }

    private void Update()
    {
        if (generateWaves)
        {
            GenerateWaves(true);
            generateWaves = false;
        }

        if (tryDeform)
        {
            StartCoroutine(Deform());
            tryDeform = false;
        }

        if (animate)
        {
            AnimateWaves();
        }
    }

    void AnimateWaves()
    {
        float xMod = animateDirection.x * Time.deltaTime;
        float yMod = animateDirection.y * Time.deltaTime;

        for(int i = 0; i < octaves.Length; i++)
        {
            octaves[i].speed = new Vector2(octaves[i].speed.x + xMod, octaves[i].speed.y + yMod);
        }

        GenerateWaves(false);
        //Deform();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    Vector3[] GenerateVerts()
    {
        Vector3[] verts = new Vector3[(dimension + 1) * (dimension + 1)];

        for(int x = 0; x <= dimension; x++)
        {
            for(int z = 0; z <= dimension; z++)
            {
                verts[index(x, z)] = new Vector3(x, 0, z);
            }
        }

        meshVerts = verts;
        return verts;
    }

    int index(int x, int z)
    {
        return x * (dimension + 1) + z;
    }

    int[] GenerateTries()
    {
        int[] tries = new int[mesh.vertices.Length * 6];

        for(int x = 0; x < dimension; x++)
        {
            for(int z = 0; z < dimension; z++)
            {
                tries[(index(x, z) * 6) + 0] = index(x, z);
                tries[(index(x, z) * 6) + 1] = index(x + 1, z + 1);
                tries[(index(x, z) * 6) + 2] = index(x + 1, z);
                tries[(index(x, z) * 6) + 3] = index(x, z);
                tries[(index(x, z) * 6) + 4] = index(x, z + 1);
                tries[(index(x, z) * 6) + 5] = index(x + 1, z + 1);
            }
        }

        return tries;
    }

    Vector2[] GenerateUVs()
    {
        Vector2[] uvs = new Vector2[mesh.vertices.Length];

        for(int x = 0; x <= dimension; x++)
        {
            for(int z = 0; z <= dimension; z++)
            {
                float valueToBringMinToZero = 0 - minY;
                float adjustedMax = maxY + valueToBringMinToZero;

                float t = mesh.vertices[(index(x, z))].y + valueToBringMinToZero;
                t = t / adjustedMax;
                Vector2 uv = new Vector2((x / uvScale) % 2, Mathf.Lerp(0f, 1f, t));
                uvs[index(x, z)] = new Vector2(uv.x <= 1 ? uv.x : 2 - uv.x, uv.y);
            }
        }

        return uvs;
    }

    void GenerateWaves(bool random)
    {
        Vector3[] verts = mesh.vertices;
        if(random) seed = Time.time;
        maxY = 0f;
        minY = 0f;


        for(int x = 0; x <= dimension; x++)
        {
            for(int z = 0; z <=dimension; z++)
            {
                float y = 0f;
                for(int o = 0; o <octaves.Length; o++)
                {
                    if (octaves[o].alternate)
                    {
                        float noiseValue = Mathf.PerlinNoise((x * octaves[0].scale.x) / dimension, (z * octaves[o].scale.y) / dimension) * Mathf.PI * 2f;
                        y += Mathf.Cos(noiseValue + octaves[0].speed.magnitude * seed) * octaves[o].height;
                    }
                    else
                    {
                        float noiseValue = Mathf.PerlinNoise((x * octaves[0].scale.x + seed * octaves[o].speed.x) / dimension, (z * octaves[o].scale.y) / dimension + seed * octaves[o].speed.y) - 0.5f;
                        y += noiseValue * octaves[o].height;
                    }
                }

                if (steps)
                {
                    float stepY = Mathf.Round(y / stepSize);
                    y = stepY * stepSize;
                }

                verts[(index(x, z))] = new Vector3(x, y, z);
                if(y < minY) { minY = y; }
                if(y > maxY) { maxY = y; }
            }
        }

        meshVerts = verts;
        mesh.vertices = verts;
        mesh.triangles = GenerateTries();
        mesh.RecalculateBounds();
        mesh.uv = GenerateUVs();
        mesh.RecalculateNormals();
    }

    float UndeformedYValue(int index)
    {
        int x = Mathf.FloorToInt((index / (dimension + 1)) / 10);
        int z = index - (x * (dimension + 1));
        return UndeformedYValue(x, z);
    }

    float UndeformedYValue(int x, int z)
    {
        float y = 0f;

        for (int o = 0; o < octaves.Length; o++)
        {
            if (octaves[o].alternate)
            {
                float noiseValue = Mathf.PerlinNoise((x * octaves[0].scale.x) / dimension, (z * octaves[o].scale.y) / dimension) * Mathf.PI * 2f;
                y += Mathf.Cos(noiseValue + octaves[0].speed.magnitude * seed) * octaves[o].height;
            }
            else
            {
                float noiseValue = Mathf.PerlinNoise((x * octaves[0].scale.x + seed * octaves[o].speed.x) / dimension, (z * octaves[o].scale.y) / dimension + seed * octaves[o].speed.y) - 0.5f;
                y += noiseValue * octaves[o].height;
            }
        }
        return y;
    }

    public float GetHeight(Vector3 pos)
    {
        Vector3 scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        Vector3 localPos = Vector3.Scale((pos - transform.position), scale);

        //Edge points
        Vector3 p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        Vector3 p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        Vector3 p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        Vector3 p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        //Clamp pos to dimensions
        p1.x = Mathf.Clamp(p1.x, 0, dimension);
        p1.z = Mathf.Clamp(p1.z, 0, dimension);
        p2.x = Mathf.Clamp(p2.x, 0, dimension);
        p2.z = Mathf.Clamp(p2.z, 0, dimension);
        p3.x = Mathf.Clamp(p3.x, 0, dimension);
        p3.z = Mathf.Clamp(p3.z, 0, dimension);
        p4.x = Mathf.Clamp(p4.x, 0, dimension);
        p4.z = Mathf.Clamp(p4.z, 0, dimension);

        //max dist to an edge
        float max = Mathf.Max(Vector3.Distance(p1, localPos), Vector3.Distance(p2, localPos), Vector3.Distance(p3, localPos), Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        float dist = (max - Vector3.Distance(p1, localPos)) + (max - Vector3.Distance(p2, localPos)) + (max - Vector3.Distance(p3, localPos)) + (max - Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        float height = mesh.vertices[index((int)p1.x, (int)p1.z)].y * (max - Vector3.Distance(p1, localPos))
                     + mesh.vertices[index((int)p2.x, (int)p2.z)].y * (max - Vector3.Distance(p1, localPos))
                     + mesh.vertices[index((int)p3.x, (int)p3.z)].y * (max - Vector3.Distance(p1, localPos))
                     + mesh.vertices[index((int)p4.x, (int)p4.z)].y * (max - Vector3.Distance(p1, localPos));

        return height * transform.lossyScale.y / dist;
    }

    #region Deform
    [Header("Deformation")]
    [SerializeField] Vector3 deformRayOffset;
    [SerializeField] Vector3 deformDirection;
    [SerializeField] float rayDist;
    [SerializeField] float deformedVertSetValue;

    public void RunDeform()
    {
        StartCoroutine(Deform());
    }

    IEnumerator Deform()
    {
        if (!skipDeform)
        {
            float startTime = Time.time;
            int frameCount = 0;
            Debug.Log("Deforming");
            int meshDeformedCount = 0;
            int rayCount = 0;

            for (int i = 0; i < meshVerts.Length; i++)
            {
                Vector3 flattenedVert = new Vector3(meshVerts[i].x, deformedVertSetValue, meshVerts[i].z);
                Vector3 rayOrigin = transform.TransformPoint(flattenedVert);
                rayOrigin += deformRayOffset;

                RaycastHit hit;
                Ray ray = new Ray(rayOrigin, deformDirection);
                rayCount++;

                if (Physics.Raycast(ray, out hit, rayDist))
                {
                    meshVerts[i] = flattenedVert;
                    meshDeformedCount++;

                    Debug.DrawLine(ray.origin, ray.origin + (deformDirection * rayDist), Color.red, 5f);
                }
                else
                {
                    if (meshVerts[i] == flattenedVert) { meshVerts[i] = new Vector3(meshVerts[i].x, UndeformedYValue(i), meshVerts[i].z); }
                    Debug.DrawLine(ray.origin, ray.origin + (deformDirection * rayDist), Color.green, 5f);
                }

                if(startTime - Time.time > 0.2f)
                {
                    frameCount++;
                    yield return null;
                }
            }

            Debug.Log("Deformed " + meshDeformedCount + " vertices, from " + rayCount + " rays, in " + frameCount + " frames");


            mesh.vertices = meshVerts;
            mesh.uv = GenerateUVs();
            mesh.RecalculateNormals();
        }
    }

    #endregion

    string Stringify()
    {
        string s = "";

        for(int i = 0; i < mesh.vertices.Length; i++)
        {
            s += mesh.vertices[i].x + "," + mesh.vertices[i].y + "," + mesh.vertices[i].z;
            s += "/";
        }

        s += "|";

        for(int i = 0; i < mesh.uv.Length; i++)
        {
            s += mesh.uv[i].x + "," + mesh.uv[i].y;
            s += "/";
        }

        return s;
    }

    void SetupFromString(string s)
    {
        Vector3[] verts = new Vector3[(dimension + 1) * (dimension + 1)];
        Vector2[] uvs = new Vector2[mesh.vertices.Length];


        string[] segments = s.Split("|");

        string[] vertString = segments[0].Split("/");

        for(int i = 0; i < verts.Length; i++)
        {
            string[] v = vertString[i].Split(",");
            verts[i] = new Vector3(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
        }

        string[] uvString = segments[1].Split("/");

        for(int i = 0; i < uvs.Length; i++)
        {
            string[] u = uvString[i].Split(",");
            uvs[i] = new Vector2(float.Parse(u[0]), float.Parse(u[1]));
        }

        meshVerts = verts;
        mesh.vertices = verts;
        mesh.triangles = GenerateTries();
        mesh.RecalculateBounds();
        mesh.uv = GenerateUVs();
    }

    [System.Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }
}
