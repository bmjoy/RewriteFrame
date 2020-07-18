#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUnit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public Bounds aabb;
    private void OnEnable()
    {
        CaculateAABB(true, ref aabb);
    }

    private  bool CaculateAABB(bool isExporting, ref Bounds aabb)
    {
        Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
        // 用来防止Encapsulate(new Bounds())
        bool initializedAABB = false;
        for (int iRenderer = 0; iRenderer < renderers.Length; iRenderer++)
        {
            Renderer iterRenderer = renderers[iRenderer];
            if (iterRenderer is ParticleSystemRenderer)
            {
                continue;
            }

            if (initializedAABB)
            {
                aabb.Encapsulate(iterRenderer.bounds);
            }
            else
            {
                initializedAABB = true;
                aabb = iterRenderer.bounds;
            }
        }

        ParticleSystem[] particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
        for (int iParticle = 0; iParticle < particleSystems.Length; iParticle++)
        {
            ParticleSystem iterParticle = particleSystems[iParticle];
            Bounds particleAABB;
            // 只有导出时才计算粒子的AABB，否则只计算粒子的坐标
            if (isExporting)
            {
                ParticleSystemRenderer iterRenderer = iterParticle.GetComponent<ParticleSystemRenderer>();
                if (iterRenderer)
                {
                    particleAABB = iterRenderer.bounds;

                }
                else
                {
                    particleAABB = new Bounds(iterParticle.transform.position, Vector3.zero);
                }
            }
            else
            {
                particleAABB = new Bounds(iterParticle.transform.position, Vector3.zero);
            }

            if (initializedAABB)
            {
                aabb.Encapsulate(particleAABB);
            }
            else
            {
                initializedAABB = true;
                aabb = particleAABB;
            }

        }

        return initializedAABB;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 1, 1f);
        Gizmos.DrawWireCube(aabb.center, aabb.size);
    }
}
#endif