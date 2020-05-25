using System;
using UnityEngine;

public class CurveTestMesh : MonoBehaviour
{
    public float width = 1;
    public float height = 1;
    public float offset = 0;
    public float degrees = 0;

    public bool use_smooth = true;

    public Material straightMaterial;
    public Material curveMaterial;

    float last_offset;
    float last_degrees;
    bool last_smooth;

    MeshFilter meshFilter;

    public void Start()
    {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = new Material[]{ straightMaterial, curveMaterial };

        meshFilter = gameObject.AddComponent<MeshFilter>();

        UdpateSmooth();
    }

    public void Update()
    {
        degrees = Mathf.Clamp(degrees, 0, 90);
        offset %= 1; // wrap 0 to 1

        if (use_smooth != last_smooth || 
            !Mathf.Approximately(last_offset-offset, 0) || 
            !Mathf.Approximately(last_degrees - degrees, 0))
        {
            if(use_smooth)
            {
                UdpateSmooth();
            }
            else
            {
                UpdateJanky();
            }
            curveMaterial.SetFloat("_VOffset", offset);

            last_offset = offset;
            last_degrees = degrees;
            last_smooth = use_smooth;
        }
    }

    private void UdpateSmooth()
    {
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;

        float r = 0.5f;
        float theta = degrees * Mathf.Deg2Rad;
        float arclen = theta * r;

        Quaternion edge_rot = Quaternion.Euler(0, 0, degrees - 90f);

        Vector3[] vertices = new Vector3[12]
        {
            // first box
            new Vector3(0, -height, 0),
            new Vector3(width, -height, 0),
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),

            // second box
            Quaternion.Euler(0, 0, degrees) * new Vector3(0, 0, 0),
            Quaternion.Euler(0, 0, degrees) * new Vector3(width, 0, 0),
            Quaternion.Euler(0, 0, degrees) * new Vector3(0, height, 0),
            Quaternion.Euler(0, 0, degrees) * new Vector3(width, height, 0),

            // corner cap
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            edge_rot * new Vector3(0, height, 0),
            new Vector3(width, height * Mathf.Tan(theta * 0.5f), 0),
        };
        mesh.vertices = vertices;

        Vector3[] normals = new Vector3[12];
        for (int i = 0; i < normals.Length; ++i)
        {
            normals[i] = -Vector3.forward;
        }
        mesh.normals = normals;

        Vector2[] uv = new Vector2[12]
        {
            new Vector2(0, 0+offset),
            new Vector2(1, 0+offset),
            new Vector2(0, 1+offset),
            new Vector2(1, 1+offset),

            new Vector2(0, 0+offset+arclen),
            new Vector2(1, 0+offset+arclen),
            new Vector2(0, 1+offset+arclen),
            new Vector2(1, 1+offset+arclen),

            new Vector2(0, 0),
            new Vector2(1, 0),
            edge_rot * new Vector2(0, 1),
            new Vector2(1, Mathf.Tan(theta * 0.5f)),
        };
        mesh.uv = uv;

        int[] straightTris = new int[12]
        {
            0, 2, 1,
            2, 3, 1,

            0+4, 2+4, 1+4,
            2+4, 3+4, 1+4,
        };

        int[] curvedTris = new int[6]
        {
            0+8, 2+8, 1+8,
            2+8, 3+8, 1+8,
        };

        mesh.SetTriangles(straightTris, 0);
        mesh.SetTriangles(curvedTris, 1);

        meshFilter.mesh = mesh;

    }

    private void UpdateJanky()
    {
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;

        float some_fudge = 0.65f;
        float theta = degrees * Mathf.Deg2Rad;
        float corner_offset = (theta * some_fudge) % 1;

        Quaternion edge_rot = Quaternion.Euler(0, 0, degrees - 90f);
        Quaternion corner_rot = Quaternion.Euler(0, 0, (degrees * 0.5f) - 90f);

        // 4 corner tris
        float corner_cap_deg = degrees / 4; 
        float corner_cap_offset = corner_offset / 4;

        Vector3[] vertices = new Vector3[14]
        {
            // first box
            new Vector3(0, -height, 0),
            new Vector3(width, -height, 0),
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),

            //second box
            Quaternion.Euler(0, 0, degrees) * new Vector3(0, 0, 0),
            Quaternion.Euler(0, 0, degrees) * new Vector3(width, 0, 0),
            Quaternion.Euler(0, 0, degrees) * new Vector3(0, height, 0),
            Quaternion.Euler(0, 0, degrees) * new Vector3(width, height, 0),

            // corner cap
            new Vector3(0, 0, 0),
            Quaternion.Euler(0, 0, corner_cap_deg * 0) * new Vector3(width, 0, 0),
            Quaternion.Euler(0, 0, corner_cap_deg * 1) * new Vector3(width, 0, 0),
            Quaternion.Euler(0, 0, corner_cap_deg * 2) * new Vector3(width, 0, 0),
            Quaternion.Euler(0, 0, corner_cap_deg * 3) * new Vector3(width, 0, 0),
            Quaternion.Euler(0, 0, corner_cap_deg * 4) * new Vector3(width, 0, 0),
        };
        mesh.vertices = vertices;

        Vector3[] normals = new Vector3[14];
        for(int i = 0; i < normals.Length; ++i)
        {
            normals[i] = -Vector3.forward;
        }
        mesh.normals = normals;

        Vector2[] uv = new Vector2[14]
        {
            new Vector2(0, 0 + offset),
            new Vector2(1, 0 + offset),
            new Vector2(0, 1 + offset),
            new Vector2(1, 1 + offset),

            new Vector2(0, 0 + offset + corner_offset),
            new Vector2(1, 0 + offset + corner_offset),
            new Vector2(0, 1 + offset + corner_offset),
            new Vector2(1, 1 + offset + corner_offset),

            new Vector2(0, 0),
            new Vector2(1, offset + corner_cap_offset * 0),
            new Vector2(1, offset + corner_cap_offset * 1),
            new Vector2(1, offset + corner_cap_offset * 2),
            new Vector2(1, offset + corner_cap_offset * 3),
            new Vector2(1, offset + corner_cap_offset * 4),
        };
        mesh.uv = uv;

        int[] tris = new int[3 * 8]
        {
            0, 2, 1,
            2, 3, 1,

            0+4, 2+4, 1+4,
            2+4, 3+4, 1+4,

            8, 10, 9,
            8, 11, 10,
            8, 12, 11,
            8, 13, 12,
        };

        mesh.SetTriangles(tris, 0);

        meshFilter.mesh = mesh;
    }
}