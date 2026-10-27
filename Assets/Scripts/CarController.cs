using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// Controls the vehicle's physics, audio, camera switching, and HUD speed display.
public class CarController : MonoBehaviour
{
    [SerializeField] WheelCollider frontRight, frontLeft;
    [SerializeField] WheelCollider backRight1, backRight2, backLeft1, backLeft2;

    [SerializeField] Transform frontRightTrans, frontLeftTrans, backRightTrans, backLeftTrans, CenterOfMass;
    public Rigidbody carRigidbody;
    public LapController lapController;
    PlayerControls playerControls;
    EnemyController enemyController;
    public TMP_Text speedTMP;
    public float acceleration, brakingForce, maxTurnAngle, currentSpeed;
    public float speedDisplayMultiplier = 2f;
    public float accelerationFoV = 10f, foVSmoothness = 8f;
    public AudioClip startSound, idleSound, RevSound;
    public AudioClip[] Songs;
    public AudioSource audioSource, radioSource;
    public float currentAcceleration, currentBrakingForce, currentTurnAngle;
    public GameObject[] cameras;

    // Initialize references and play the vehicle start sound.
    void Start()
    {
        playerControls = GetComponent<PlayerControls>();
        enemyController = GetComponent<EnemyController>();

        if (carRigidbody == null)
        {
            carRigidbody = GetComponent<Rigidbody>();
            carRigidbody.centerOfMass = CenterOfMass.localPosition;
        }
        if (cameras != null && cameras.Length > 0 && cameras[0] != null)
        {
            cameras[0].SetActive(true);
        }
        audioSource.clip = startSound;
        audioSource.loop = false;
        audioSource.Play();
    }

    // Update per-frame effects such as camera control and audio.
    void Update()
    {
        CarAudio();
        if (tag == "Player")
        {
            CameraControl();
            CarRadio();
            UpdateSpeedDisplay();
        }
    }

    // Apply steering, acceleration, and braking forces in the physics step.
    void FixedUpdate()
    {
        if (lapController != null && lapController.IsRacerFinished(this))
        {
            StopCar();
            return;
        }

        LimitTopSpeed();

        backRight1.motorTorque = currentAcceleration;
        backRight2.motorTorque = currentAcceleration;
        backLeft1.motorTorque = currentAcceleration;
        backLeft2.motorTorque = currentAcceleration;


        backRight1.brakeTorque = currentBrakingForce;
        backRight2.brakeTorque = currentBrakingForce;
        backLeft1.brakeTorque = currentBrakingForce;
        backLeft2.brakeTorque = currentBrakingForce;

        frontRight.steerAngle = currentTurnAngle;
        frontLeft.steerAngle = currentTurnAngle;

        UpdateWheel(frontLeft, frontLeftTrans);
        UpdateWheel(frontRight, frontRightTrans);
        UpdateWheel(backLeft1, backLeftTrans);
        UpdateWheel(backRight1, backRightTrans);
    }

    /// <summary>
    /// Converts the current input values into engine/brake/steering forces.
    /// </summary>
    public void SetInput(float accelerationInput, float steeringInput, float brakeInput, float handbrakeInput)
    {
        currentAcceleration = acceleration * Mathf.Clamp(accelerationInput, -1f, 1f);
        currentTurnAngle = Mathf.Clamp(steeringInput, -maxTurnAngle, maxTurnAngle);
        currentBrakingForce = brakingForce * Mathf.Clamp01(Mathf.Max(brakeInput, handbrakeInput));
    }

    // Reads the AI or player top-speed settings and converts them to world-space units.
    float GetWorldSpeedLimit()
    {
        float topSpeed = playerControls != null
            ? playerControls.topSpeed
            : enemyController != null
                ? enemyController.topSpeed
                : 0f;
        return topSpeed / (3.6f * speedDisplayMultiplier);
    }

    // Prevents the car from exceeding its configured speed limit.
    void LimitTopSpeed()
    {
        float maxWorldSpeed = GetWorldSpeedLimit();
        if (carRigidbody == null)
        {
            return;
        }

        float worldSpeed = carRigidbody.linearVelocity.magnitude;
        currentSpeed = worldSpeed * 3.6f;
        if (maxWorldSpeed > 0f && worldSpeed > maxWorldSpeed)
        {
            Vector3 clampedVelocity = carRigidbody.linearVelocity.normalized * maxWorldSpeed;
            carRigidbody.linearVelocity = clampedVelocity;
            currentSpeed = maxWorldSpeed * 3.6f;
        }
    }

    // Brings the car to a stop when a race result is finalized.
    public void StopCar()
    {
        currentAcceleration = 0f;
        currentBrakingForce = brakingForce;
        currentTurnAngle = 0f;

        backRight1.motorTorque = 0f;
        backRight2.motorTorque = 0f;
        backLeft1.motorTorque = 0f;
        backLeft2.motorTorque = 0f;
        backRight1.brakeTorque = brakingForce;
        backLeft1.brakeTorque = brakingForce;
        frontRight.steerAngle = 0f;
        frontLeft.steerAngle = 0f;

        if (carRigidbody != null)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
        }
    }

    // Toggles between the active camera views when the player presses C.
    void CameraControl()
    {
        if (Input.GetKeyDown(KeyCode.C) && cameras != null && cameras.Length >= 2
            && cameras[0] != null && cameras[1] != null)
        {
            if (cameras[0].activeSelf)
            {
                cameras[0].SetActive(false);
                cameras[1].SetActive(true);
            }
            else
            {
                cameras[0].SetActive(true);
                cameras[1].SetActive(false);
            }
        }

        UpdateCameraFoV();
    }

    // Adjusts field-of-view to simulate acceleration and speed changes.
    void UpdateCameraFoV()
    {
        if (cameras == null || cameras.Length == 0)
        {
            return;
        }

        float baseFoV = PlayerPrefs.GetFloat("CameraFoV", 60f);
        float forwardInput = Mathf.Clamp01(Input.GetAxis("Vertical"));
        float speed = carRigidbody == null
            ? 0f
            : carRigidbody.linearVelocity.magnitude;
        float maxWorldSpeed = GetWorldSpeedLimit();
        float speedFactor = Mathf.Clamp01(speed / Mathf.Max(maxWorldSpeed, 0.1f));
        float targetFoV = baseFoV + accelerationFoV * forwardInput * speedFactor;

        foreach (GameObject cameraObject in cameras)
        {
            if (cameraObject == null)
            {
                continue;
            }

            Camera camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraObject.GetComponentInChildren<Camera>(true);
            }
            if (camera != null)
            {
                camera.fieldOfView = Mathf.Lerp(
                    camera.fieldOfView,
                    targetFoV,
                    foVSmoothness * Time.deltaTime);
            }
        }
    }

    // Synchronizes the wheel visual meshes with their collider poses.
    void UpdateWheel(WheelCollider col, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);
        trans.position = pos;
        trans.rotation = rot;
    }

    // Switches the engine sound between idle and revving based on input.
    void CarAudio()
    {
        if (audioSource.isPlaying && audioSource.clip == startSound)
        {
            return;
        }

        AudioClip targetSound = Input.GetAxis("Vertical") > 0.01f
            ? RevSound
            : idleSound;

        if (audioSource.clip != targetSound || !audioSource.isPlaying)
        {
            audioSource.clip = targetSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // Plays a random radio clip when the current one has finished.
    void CarRadio()
    {
        if (radioSource == null || Songs == null || Songs.Length == 0)
            return;
        if (!radioSource.isPlaying)
        {
            int randomIndex = Random.Range(0, Songs.Length);
            radioSource.clip = Songs[randomIndex];
            radioSource.loop = false;
            radioSource.Play();
        }
    }

    // Updates the dashboard speed readout for the player.
    void UpdateSpeedDisplay()
    {
        if (speedTMP == null)
        {
            return;
        }

        float speed = carRigidbody == null
            ? 0f
            : carRigidbody.linearVelocity.magnitude * 3.6f * speedDisplayMultiplier;

        speedTMP.text = Mathf.RoundToInt(speed) + " km/h";
    }

}