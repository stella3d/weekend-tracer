using System.Collections.Generic;
using RayTracingWeekend;
using Unity.Mathematics;
using UnityEngine;
using Material = UnityEngine.Material;

public class RaytracedSphere : MonoBehaviour
{
    public Sphere sphere { get; private set; }

    public MaterialType materialType
    {
        get => m_MaterialType;
        set => m_MaterialType = value;
    }

    [SerializeField] MaterialType m_MaterialType;

    Material m_UnityMaterial;
    
    Vector3 m_PreviousPosition;
    Color m_PreviousColor;
    
    static readonly int Smoothness = Shader.PropertyToID("Smoothness");

    void Awake()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        if(meshRenderer != null)
            m_UnityMaterial = meshRenderer.material;
    }

    void Update()
    {
        var color = m_UnityMaterial.color.linear;
        var pos = transform.position;
        if (color != m_PreviousColor || pos != m_PreviousPosition)
        {
            OnSphereChanged();
        }

        m_PreviousPosition = pos;
        m_PreviousColor = color;
    }

    public Sphere GetSphere()
    {
        var trans = transform;
        var smoothness = m_UnityMaterial.GetFloat(Smoothness);
        return new Sphere
        {
            material =
            {
                albedo = m_UnityMaterial.GetAlbedo(),
                fuzziness = (1f - smoothness)
            },
            radius = trans.localScale.x / 2,
            center = trans.position
        };
    }

    void OnSphereChanged()
    {
        sphere = GetSphere();
    }
}
