using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;

namespace TGStage
{

    [System.Serializable]
    public class TGStageItemsGroup
    {
        public string Name;
        public List<Transform> Items = new List<Transform>();
    }

    public class TGStageSimpleManager : MonoBehaviour
    {
        public Shader animatedShader;
        public List<Toggle> UIControls = new List<Toggle>();
        public List<TGStageItemsGroup> StageGroups = new List<TGStageItemsGroup>();

        
        void Start()
        {
            // Turn off all groups
            foreach (TGStageItemsGroup group in StageGroups)
            {
                ToggleGroup(group, false);
            }
            // Toggle events
            foreach (Toggle control in UIControls)
            {
                TMP_Text txt = control.GetComponentInChildren<TMP_Text>(true);
                if (txt)
                    control.onValueChanged.AddListener((value) => onToggleChanged(txt.text, value));
            }
        }


        // UI Toggle changed event
        void onToggleChanged(string groupName, bool isOn)
        {
            ToggleGroup(groupName,isOn);   
        }


        void Update()
        {

        }

        public void ToggleGroup(string groupName, bool isOn)
        {
            TGStageItemsGroup group = StageGroups.Find(group => group.Name == groupName);
            if (group != null)
            {
                ToggleGroup(group, isOn);
            }
        }


        public void ToggleGroup(TGStageItemsGroup group, bool isOn)
        {
            foreach(Transform item in group.Items)
            {
                toggleItem(item,isOn);
            }
        }

        void toggleItem(Transform item, bool isOn)
        {
            if (!item) return;
            // Animator
            Animator anim = item.GetComponent<Animator>();
            if (anim)
            {
                try
                {
                    anim.SetBool("Enabled", isOn);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(item.name+" Animator does not have a bool parameter named 'Enabled'");
                }
            }
            
            // Lights
            Light[] lights = item.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                light.enabled = isOn;
            }
            
            // Particle Systems
            ParticleSystem[] particleSystems = item.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in particleSystems)
            {
                if (isOn)
                {
                    ps.Play(true);
                }
                else
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            // Audio Source
            AudioSource audioSrc = item.GetComponent<AudioSource>();
            if (audioSrc) {
                audioSrc.enabled = isOn;
            }

            // Beam objects/ Emissive / Animated shader
            MeshRenderer[] renderers = item.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer renderer in renderers)
            {
                // Beams
                if (renderer.shadowCastingMode == ShadowCastingMode.Off)
                {
                    renderer.gameObject.SetActive(isOn);
                }
                else
                {                    
                    // Control Emission
                    Material mat = renderer.material;
                    if (mat != null)
                    {
                        if (mat.shader != animatedShader)
                        {
                            // Regular Lit shader
                            setEmission(mat, renderer.sharedMaterial, isOn);
                        }
                        else
                        {
                            // Animated shader
                            mat.SetFloat("_Anims_Enabled", isOn ? 1 : 0);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Enables or disables emission on a material in a universal way.
        /// Supports Built-in, URP, and HDRP render pipelines:
        /// - For Built-in and URP: controls the _EMISSION keyword.
        /// - For HDRP: controls the _EmissiveIntensity property.
        /// Emission is only modified if an emission map is assigned
        /// (_EmissionMap or _EmissiveColorMap depending on the pipeline).
        /// The emission color is not changed by this method.
        /// </summary>
        /// <param name="mat">The material to modify.</param>
        /// <param name="isOn">True to enable emission, False to disable it.</param>
        void setEmission(Material mat, Material sharedMat, bool isOn)
        {
            if (mat == null) return;
            bool isHDRP = GraphicsSettings.currentRenderPipeline != null &&
                          GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HDRenderPipeline");

            if (isHDRP)
            {
                if (mat.HasProperty("_EmissiveColorMap"))
                {
                    Texture emissionMap = mat.GetTexture("_EmissiveColorMap");
                    if (emissionMap == null)
                        return;
                }
                else
                {
                    return;
                }
                mat.SetColor("_EmissiveColor", isOn ? sharedMat.GetColor("_EmissionColor") * sharedMat.GetFloat("_EmissiveIntensity") : Color.black);

                mat.globalIlluminationFlags = isOn
                    ? MaterialGlobalIlluminationFlags.RealtimeEmissive
                    : MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
            else
            {
                if (mat.HasProperty("_EmissionMap"))
                {
                    Texture emissionMap = mat.GetTexture("_EmissionMap");
                    if (emissionMap == null)
                        return; 
                }
                else
                {
                    return; 
                }

                if (isOn)
                    mat.EnableKeyword("_EMISSION");
                else
                    mat.DisableKeyword("_EMISSION");

                mat.globalIlluminationFlags = isOn
                    ? MaterialGlobalIlluminationFlags.RealtimeEmissive
                    : MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }
    



}
}
