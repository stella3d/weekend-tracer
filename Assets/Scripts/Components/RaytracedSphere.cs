using RayTracingWeekend;
using UnityEngine;
using Material = UnityEngine.Material;

[ExecuteAlways]
public class RaytracedSphere : MonoBehaviour
{
    public Sphere sphere { get; private set; }

    [SerializeField] MaterialType m_MaterialType = MaterialType.Lambertian;

    Material m_UnityMaterial;
    
    Vector3 m_PreviousPosition;
    Color m_PreviousColor;
    
    static readonly int Smoothness = Shader.PropertyToID("_Glossiness");

    void OnEnable()
    {
        GetUnityMaterial();
    }

    void Awake()
    {
        GetUnityMaterial();
    }

    void GetUnityMaterial()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        m_UnityMaterial = meshRenderer.sharedMaterial;
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
        var pos = trans.position;
        var smoothness = m_UnityMaterial.GetFloat(Smoothness);
        return new Sphere
        {
            material =
            {
                albedo = m_UnityMaterial.GetAlbedo(),
                fuzziness = (1f - smoothness),
                type = m_MaterialType
            },
            radius = trans.localScale.x / 2,
            center = pos,
        };
    }

    void OnSphereChanged()
    {
        sphere = GetSphere();
    }
}
