using UnityEngine;
using VR = UnityEngine.VR;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(CharacterMotor))]
public class VRStepDetect : MonoBehaviour
{

    public float stepHMDThreshold = 0.1f;
    public float stepHandThreshold = 0.3f;
    public float shakeHMDThreshold = 0.1f;
    public float shakeHandThreshold = 0.3f;
    public float stepHMDRunThreshold = 0.1f;
    public float stepHandRunThreshold = 0.8f;
    public float walkSpeed = 1f;
    public float runSpeed = 2.3f;
    public float framesPerStep = 30;

    private GameObject viveHand1;
    private GameObject viveHand2;
    private GameObject player;
    private CharacterMotor playerMotor;

    private SteamVR_Controller.Device viveHMD;
    private SteamVR_Controller.Device viveHand1_cont;
    private SteamVR_Controller.Device viveHand2_cont;

    private int stepFrame = 0;
    private double lastupdate = 0.0f;
    private Vector3 dirForward;
    private bool integratingStep = false;

    void Start()
    {

        playerMotor = GetComponent<CharacterMotor>();
        player = GameObject.Find("Player");
        GameObject[] controllers = GameObject.FindGameObjectsWithTag("GameController");

        if (controllers.Length == 2)
        {
            viveHand1 = controllers[0];
            viveHand2 = controllers[1];
        }

        try
        {
            viveHMD = SteamVR_Controller.Input((int)SteamVR_TrackedObject.EIndex.Hmd);
            //playerMotor.setCharacterHeight(viveHMD.he)
            viveHand1_cont = SteamVR_Controller.Input(SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost));
            viveHand2_cont = SteamVR_Controller.Input(SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost));
        }
        catch
        {

        }

    }

    float velocity;

    void Update()
    {
        UpdateMovement();
    }

    void UpdateMovement()
    {
        if (viveHand1 == null || viveHand2 == null)
        {
            GameObject[] controllers = GameObject.FindGameObjectsWithTag("GameController");

            if (controllers.Length == 2)
            {
                viveHand1 = controllers[0];
                viveHand2 = controllers[1];
            }
        }
        if (viveHand1_cont == null || viveHand2_cont == null || viveHand1_cont.uninitialized || viveHand2_cont.uninitialized)
        {
            try
            {
                viveHMD = SteamVR_Controller.Input((int)SteamVR_TrackedObject.EIndex.Hmd);
                viveHand1_cont = SteamVR_Controller.Input(SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost));
                viveHand2_cont = SteamVR_Controller.Input(SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost));
            }
            catch
            {

            }
        }
        else
        {
            double curTime = Time.time;
            if (!integratingStep)
            {
                if (stepDetect())
                {

                    dirForward = (viveHand1.transform.forward + viveHand2.transform.forward);
                    dirForward = dirForward.normalized;
                    if ( runDetect() ) { 
                        dirForward *= runSpeed;
                    }
                    else
                    {
                        dirForward *= walkSpeed;
                    }
                    //player.transform.Translate(dirForward.x * speed, 0, dirForward.z * speed);

                    stepFrame = 0;
                    integratingStep = true;
                }
            }
            else
            {
                playerMotor.inputMoveDirection = dirForward;
                //float dT = (float)(curTime - lastupdate);
                //playerController.SimpleMove(dirForward * (stepDistance * dT));
                //player.transform.Translate(dirForward.x * stepDistance * dT, 0, dirForward.z * stepDistance * dT);
                //player.transform.Translate(dirForward.x * speed, 0, dirForward.z * speed);
                stepFrame++;

                if (stepFrame >= (framesPerStep / 2.0f))
                {
                    Vector3 velHMD = viveHMD.velocity;
                    Vector3 velHand1 = viveHand1_cont.velocity;
                    Vector3 velHand2 = viveHand2_cont.velocity;
                    float hand1Sign = Mathf.Sign(velHand1.y);
                    float hand2Sign = Mathf.Sign(velHand2.y);
                    if ((hand1Sign != hand2Sign)
                        && (velHMD.y > stepHMDThreshold || velHMD.y < -stepHMDThreshold)
                        && ((velHand1.y > stepHandThreshold || velHand1.y < -stepHandThreshold)
                            || (velHand2.y > stepHandThreshold || velHand2.y < -stepHandThreshold))
                        && (velHMD.x < shakeHMDThreshold && velHMD.x > -shakeHMDThreshold)
                        && (velHMD.z < shakeHMDThreshold && velHMD.z > -shakeHMDThreshold)
                        && (velHand1.x < stepHandThreshold && velHand1.x > -stepHandThreshold)
                        && (velHand1.z < stepHandThreshold && velHand1.z > -stepHandThreshold)
                        && (velHand2.x < stepHandThreshold && velHand2.x > -stepHandThreshold)
                        && (velHand2.z < stepHandThreshold && velHand2.z > -stepHandThreshold)
                        )
                    {
                        dirForward = (viveHand1.transform.forward + viveHand2.transform.forward);
                        dirForward = dirForward.normalized;
                        if ( runDetect() )
                        { 
                            dirForward *= runSpeed;
                        } else
                        {
                            dirForward *= walkSpeed;
                        }
                        stepFrame = 0;
                    }
                }
            }

            if (stepFrame >= framesPerStep)
            {

                playerMotor.inputMoveDirection = Vector3.zero;
                integratingStep = false;
                stepFrame = 0;
            }

            player.transform.position = playerMotor.transform.position;
            lastupdate = curTime;
        }
    }

    private bool stepDetect()
    {
        Vector3 velHMD = viveHMD.velocity;
        //Debug.Log("HMD Vel X: " + velHMD.x);
        //Debug.Log("HMD Vel Y: " + velHMD.y);
        //Debug.Log("HMD Vel Z: " + velHMD.z);
        Vector3 velHand1 = viveHand1_cont.velocity;
        //Debug.Log("hand 1 Vel X: " + velHand1.x);
        //Debug.Log("hand 1 Vel Y: " + velHand1.y);
        //Debug.Log("hand 1 Vel Z: " + velHand1.z);
        Vector3 velHand2 = viveHand2_cont.velocity;
        //Debug.Log("hand 2 Vel X: " + velHand2.x);
        //Debug.Log("hand 2 Vel Y: " + velHand2.y);
        //Debug.Log("hand 2 Vel Z: " + velHand2.z);
        float hand1Sign = Mathf.Sign(velHand1.y);
        float hand2Sign = Mathf.Sign(velHand2.y);
        if ((hand1Sign != hand2Sign)
            && (velHMD.y > stepHMDThreshold || velHMD.y < -stepHMDThreshold)
            && ((velHand1.y > stepHandThreshold || velHand1.y < -stepHandThreshold)
                || (velHand2.y > stepHandThreshold || velHand2.y < -stepHandThreshold))
            && (velHMD.x < shakeHMDThreshold && velHMD.x > -shakeHMDThreshold)
            && (velHMD.z < shakeHMDThreshold && velHMD.z > -shakeHMDThreshold)
            && (velHand1.x < stepHandThreshold && velHand1.x > -stepHandThreshold)
            && (velHand1.z < stepHandThreshold && velHand1.z > -stepHandThreshold)
            && (velHand2.x < stepHandThreshold && velHand2.x > -stepHandThreshold)
            && (velHand2.z < stepHandThreshold && velHand2.z > -stepHandThreshold)
            )
        {
            //Debug.Log("Step Taken to X: " + player.transform.position.x + " Z: " + player.transform.position.z);
            return true;
        }
        return false;
    }

    private bool runDetect()
    {
        Vector3 velHMD = viveHMD.velocity;
        //Debug.Log("HMD Vel X: " + velHMD.x);
        //Debug.Log("HMD Vel Y: " + velHMD.y);
        //Debug.Log("HMD Vel Z: " + velHMD.z);
        Vector3 velHand1 = viveHand1_cont.velocity;
        //Debug.Log("hand 1 Vel X: " + velHand1.x);
        //Debug.Log("hand 1 Vel Y: " + velHand1.y);
        //Debug.Log("hand 1 Vel Z: " + velHand1.z);
        Vector3 velHand2 = viveHand2_cont.velocity;
        return (((velHand1.y > stepHandRunThreshold || velHand1.y < -stepHandRunThreshold)
                        || (velHand2.y > stepHandRunThreshold || velHand2.y < -stepHandRunThreshold))
                        );
                        //&& (velHMD.y > stepHMDRunThreshold || velHMD.y < -stepHMDRunThreshold));
    }
}