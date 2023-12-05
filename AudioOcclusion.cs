
using System; 
using UnityEngine;
using FMODUnity;
using FMOD.Studio; 

public class AudioOcclusion : MonoBehaviour
{

    EventInstance audioSource;
    [SerializeField] float maxDistance = 10;
    [SerializeField] Transform playerTransform;
    [SerializeField] LayerMask obstacleLayer;
    [Header("Add tag for objects excluded from occlusion")]
    [SerializeField] string FMODEvent;
    [Header("Start boundary for FMOD Event & Parameter")]
    [SerializeField] float paramDistanceMax = 20;
    [Header("Audio occlusion parameter")]
    [SerializeField] string FMODParam;
    [Space(20)]
    [Header("Optional Settings")] 
    [Space(5)]
    [SerializeField] string NonOcclusionTag;
    [Header("Parameter to & from occluded object")]
    [SerializeField] string FMODParamFade;
    [Header("If true range 0 - 1")]
    [SerializeField] bool paramValToOne = true;
    [Space(20)]
    [Header("Set on Start - Use this for reverb types")]
    [Space(5)]
    [SerializeField] string fmodMaterialParameter; 
    [SerializeField] private ReverbMaterial selectedMaterial;
    [SerializeField]
    enum ReverbMaterial
    {
        Stone,
        Wood,
        Metal,
        FabricThin,
        FabricThick
    }
    private int fmodMaterialInt;
    private float min = 0;
    [Header("Debug tools")]
    [Space(5)]
    [SerializeField] float scaledValDepth;
    [SerializeField] bool Obstruction;
    private bool playEvent;
    private float valToStartEvent;


    void Start()
    {
        Obstruction = false;
        playEvent = false;

        switch (selectedMaterial)
        {
            case ReverbMaterial.Stone:
                fmodMaterialInt = 0;  
                break;
            case ReverbMaterial.Wood:
                fmodMaterialInt = 1;
                break;
            case ReverbMaterial.Metal:
                fmodMaterialInt = 2;
                break;
            case ReverbMaterial.FabricThin:
                fmodMaterialInt = 3;
                break;
            case ReverbMaterial.FabricThick:
                fmodMaterialInt = 4;
                break;
        }
        
    }

    void Update()
    {
        // Ensure there is a player transform to track
        if (playerTransform != null)
        {
            // Calculate the direction from the tracking object to the player 
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, maxDistance, obstacleLayer))
            {
                // Check if the hit point is between the listener and the player
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                float hitDistance = Vector3.Distance(transform.position, hit.point);

                Vector3 size = hit.collider.bounds.size;
                float width = size.x;
                float height = size.y;
                float depth = size.z;

                //keeps the value consistant regardles of range direction - used to start event 
                valToStartEvent = Mathf.Clamp(1f - (distanceToPlayer - width) / (paramDistanceMax - min), 0f, 1f);

                //used to switch the range direction for the parameter value.
                if (paramValToOne)
                {
                    scaledValDepth = Mathf.Clamp(1f - (distanceToPlayer - width) / (paramDistanceMax - min), 0f, 1f);
                }
                else { scaledValDepth = Mathf.Clamp((distanceToPlayer - width) / (paramDistanceMax - min), 0f, 1f); }

               
                if (valToStartEvent > 0)
                {
                    if (!playEvent)
                    {
                        audioSource = RuntimeManager.CreateInstance(FMODEvent);
                        audioSource.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
                        audioSource.start();
                        audioSource.setParameterByName(fmodMaterialParameter, fmodMaterialInt);
                   
                        playEvent = true;
                    }
                    audioSource.setParameterByName(FMODParamFade, scaledValDepth);
                }
                else
                {
                    if (playEvent)
                    {
                        audioSource.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                        audioSource.release();
                        playEvent = false;
                    }
                }

                if (hitDistance < distanceToPlayer)
                {
                    Obstruction = true;
                    audioSource.setParameterByName(FMODParam, 1);
                }
                else
                {
                    Obstruction = false;
                    audioSource.setParameterByName(FMODParam, default);
                }

                if (hit.collider.tag == NonOcclusionTag)
                {
                    audioSource.setParameterByName(FMODParam, default);
                }
            }
            else
            {
                Obstruction = false;
                audioSource.setParameterByName(FMODParam, default);
            }
        }
    }
}
