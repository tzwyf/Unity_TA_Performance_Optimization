using UnityEngine;

namespace TA_Runtime
{
    public class OfficeLightFogLOD : MonoBehaviour
    {
        public float cullDistance = 15f;
        public float reduceDistance = 8f;
    
        private ParticleSystem[] particleSystems;
        private float[] originalEmissionRates;
        private bool wasCulled;
        private bool wasReduced;
    
        void Start()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            originalEmissionRates = new float[particleSystems.Length];
            for (int i = 0; i < particleSystems.Length; i++)
            {
                originalEmissionRates[i] = particleSystems[i].emission.rateOverTime.constant;
            }
        }
    
        void LateUpdate()
        {
            if (Camera.main == null) return;
    
            float sqrDist = (Camera.main.transform.position - transform.position).sqrMagnitude;
            float sqrCull = cullDistance * cullDistance;
            float sqrReduce = reduceDistance * reduceDistance;
    
            if (sqrDist > sqrCull)
            {
                if (!wasCulled)
                {
                    SetEmissionEnabled(false);
                    wasCulled = true;
                    wasReduced = false;
                }
            }
            else if (sqrDist > sqrReduce)
            {
                if (!wasReduced)
                {
                    SetEmissionRate(0.5f);
                    wasReduced = true;
                    wasCulled = false;
                }
            }
            else
            {
                if (wasCulled || wasReduced)
                {
                    RestoreEmission();
                    wasCulled = false;
                    wasReduced = false;
                }
            }
        }
    
        private void SetEmissionEnabled(bool enabled)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var emission = particleSystems[i].emission;
                emission.enabled = enabled;
            }
        }
    
        private void SetEmissionRate(float multiplier)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var emission = particleSystems[i].emission;
                emission.enabled = true;
                emission.rateOverTime = originalEmissionRates[i] * multiplier;
            }
        }
    
        private void RestoreEmission()
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var emission = particleSystems[i].emission;
                emission.enabled = true;
                emission.rateOverTime = originalEmissionRates[i];
            }
        }
    }
}