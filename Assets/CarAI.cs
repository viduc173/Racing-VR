using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    // Thêm vào phần [Header("General Parameters")]
    [Header("Speed Parameters")]
    public float maxForwardSpeed = 200f;    // Tốc độ tối đa
    public float accelerationForce = 200f;   // Lực tăng tốc
    public float brakeForce = 5000f;        // Lực phanh
    public float reverseSpeed = 200f;        // Tốc độ lùi
    private float rpmBalanceThreshold = 50f; // Ngưỡng chênh lệch cho phép

    [Header("Car Wheels (Wheel Collider)")]// Assign wheel Colliders through the inspector
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider backLeft;
    public WheelCollider backRight;

    [Header("Car Wheels (Transform)")]// Assign wheel Transform(Mesh render) through the inspector
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelBL;
    public Transform wheelBR;

    [Header("Car Front (Transform)")]// Assign a Gameobject representing the front of the car
    public Transform carFront;

    [Header("General Parameters")]// Look at the documentation for a detailed explanation 
    public List<string> NavMeshLayers;
    public int MaxSteeringAngle = 45;
    public int MaxRPM = 150;

    [Header("Debug")]
    public bool ShowGizmos;
    public bool Debugger;

    [Header("Destination Parameters")]// Look at the documentation for a detailed explanation
    public bool Patrol = true;
    public List<Transform> CustomDestinations; // Thay đổi từ Transform thành List<Transform>

    [HideInInspector] public bool move;// Look at the documentation for a detailed explanation

    private Vector3 PostionToFollow = Vector3.zero;
    private int currentWayPoint;
    private float AIFOV = 60;
    private bool allowMovement;
    private int NavMeshLayerBite;
    private List<Vector3> waypoints = new List<Vector3>();
    private float LocalMaxSpeed;
    private int Fails;
    private float MovementTorque = 1;
    private int currentDestinationIndex = 0;

    private float lookAheadDistance = 10f; // Khoảng cách nhìn trước
    private float maxCornerAngle = 45f;    // Góc cua tối đa
    private float stabilityForce = 3000f;   // Lực ổn định
    void Awake()
    {
        currentWayPoint = 0;
        allowMovement = true;
        move = true;
    }

    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
        CalculateNavMashLayerBite();
        SetupCustomDestinationColliders(); // Đổi tên hàm
    }

    void FixedUpdate()
    {
        UpdateWheels();
        ApplySteering();
        PathProgress();
    }

    private void CalculateNavMashLayerBite()
    {
        if (NavMeshLayers == null || NavMeshLayers.Count == 0 || NavMeshLayers[0] == "AllAreas")
        {
            NavMeshLayerBite = NavMesh.AllAreas;
        }
        else if (NavMeshLayers.Count == 1)
        {
            NavMeshLayerBite += 1 << NavMesh.GetAreaFromName(NavMeshLayers[0]);
        }
        else
        {
            foreach (string Layer in NavMeshLayers)
            {
                int I = 1 << NavMesh.GetAreaFromName(Layer);
                NavMeshLayerBite += I;
            }
        }
    }

    private void PathProgress() //Checks if the agent has reached the currentWayPoint or not. If yes, it will assign the next waypoint as the currentWayPoint depending on the input
    {
        wayPointManager();
        Movement();
        ListOptimizer();

        void wayPointManager()
        {
            if (currentWayPoint >= waypoints.Count)
            {
                // Đã đến điểm đích hiện tại, chuyển sang điểm tiếp theo
                currentDestinationIndex++;
                currentWayPoint = 0;
                waypoints.Clear();
                CreatePath();
            }
            else
            {
                PostionToFollow = waypoints[currentWayPoint];
                allowMovement = true;
                if (Vector3.Distance(carFront.position, PostionToFollow) < 2)
                    currentWayPoint++;
            }
        }

        void ListOptimizer()
        {
            if (currentWayPoint > 1 && waypoints.Count > 30)
            {
                waypoints.RemoveAt(0);
                currentWayPoint--;
            }
        }
    }

    private void CreatePath()
    {
        if (CustomDestinations == null || CustomDestinations.Count == 0)
        {
            if (Patrol == true)
                RandomPath();
            else
            {
                debug("No custom destinations assigned and Patrol is set to false", false);
                allowMovement = false;
            }
        }
        else
        {
            // Chỉ tạo đường đến điểm đích tiếp theo
            if (currentDestinationIndex < CustomDestinations.Count)
            {
                CustomPath(CustomDestinations[currentDestinationIndex]);
            }
            else if (Patrol)
            {
                // Reset về điểm đầu nếu đang ở chế độ tuần tra
                currentDestinationIndex = 0;
                CustomPath(CustomDestinations[currentDestinationIndex]);
            }
        }
    }

    public void RandomPath() // Creates a path to a random destination
    {
        NavMeshPath path = new NavMeshPath();
        Vector3 sourcePostion;

        if (waypoints.Count == 0)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 100;
            randomDirection += transform.position;
            sourcePostion = carFront.position;
            Calculate(randomDirection, sourcePostion, carFront.forward, NavMeshLayerBite);
        }
        else
        {
            sourcePostion = waypoints[waypoints.Count - 1];
            Vector3 randomPostion = Random.insideUnitSphere * 100;
            randomPostion += sourcePostion;
            Vector3 direction = (waypoints[waypoints.Count - 1] - waypoints[waypoints.Count - 2]).normalized;
            Calculate(randomPostion, sourcePostion, direction, NavMeshLayerBite);
        }

        void Calculate(Vector3 destination, Vector3 sourcePostion, Vector3 direction, int NavMeshAreaByte)
        {
            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 150, 1 << NavMesh.GetAreaFromName(NavMeshLayers[0])) &&
                NavMesh.CalculatePath(sourcePostion, hit.position, NavMeshAreaByte, path) && path.corners.Length > 2)
            {
                if (CheckForAngle(path.corners[1], sourcePostion, direction))
                {
                    waypoints.AddRange(path.corners.ToList());
                    debug("Random Path generated successfully", false);
                }
                else
                {
                    if (CheckForAngle(path.corners[2], sourcePostion, direction))
                    {
                        waypoints.AddRange(path.corners.ToList());
                        debug("Random Path generated successfully", false);
                    }
                    else
                    {
                        debug("Failed to generate a random path. Waypoints are outside the AIFOV. Generating a new one", false);
                        Fails++;
                    }
                }
            }
            else
            {
                debug("Failed to generate a random path. Invalid Path. Generating a new one", false);
                Fails++;
            }
        }
    }

    public void CustomPath(Transform destination)
    {
        NavMeshPath path = new NavMeshPath();
        Vector3 sourcePostion;

        if (waypoints.Count == 0)
        {
            sourcePostion = carFront.position;
            Calculate(destination.position, sourcePostion, carFront.forward, NavMeshLayerBite);
        }
        else
        {
            sourcePostion = waypoints[waypoints.Count - 1];
            Vector3 direction = (waypoints[waypoints.Count - 1] - waypoints[waypoints.Count - 2]).normalized;
            Calculate(destination.position, sourcePostion, direction, NavMeshLayerBite);
        }

        void Calculate(Vector3 destination, Vector3 sourcePostion, Vector3 direction, int NavMeshAreaBite)
        {
            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 150, NavMeshAreaBite) &&
                NavMesh.CalculatePath(sourcePostion, hit.position, NavMeshAreaBite, path))
            {
                if (path.corners.ToList().Count() > 1 && CheckForAngle(path.corners[1], sourcePostion, direction))
                {
                    waypoints.AddRange(path.corners.ToList());
                    debug("Custom Path generated successfully", false);
                }
                else
                {
                    if (path.corners.Length > 2 && CheckForAngle(path.corners[2], sourcePostion, direction))
                    {
                        waypoints.AddRange(path.corners.ToList());
                        debug("Custom Path generated successfully", false);
                    }
                    else
                    {
                        debug("Failed to generate a Custom path. Waypoints are outside the AIFOV. Generating a new one", false);
                        Fails++;
                    }
                }
            }
            else
            {
                debug("Failed to generate a Custom path. Invalid Path. Generating a new one", false);
                Fails++;
            }
        }
    }
    private bool CheckForAngle(Vector3 pos, Vector3 source, Vector3 direction) //calculates the angle between the car and the waypoint 
    {
        Vector3 distance = (pos - source).normalized;
        float CosAngle = Vector3.Dot(distance, direction);
        float Angle = Mathf.Acos(CosAngle) * Mathf.Rad2Deg;

        if (Angle < AIFOV)
            return true;
        else
            return false;
    }

    private void ApplyBrakes()
    {
        frontLeft.brakeTorque = brakeForce;
        frontRight.brakeTorque = brakeForce;
        backLeft.brakeTorque = brakeForce;
        backRight.brakeTorque = brakeForce;
    }


    private void UpdateWheels() // Updates the wheel's postion and rotation
    {
        ApplyRotationAndPostion(frontLeft, wheelFL);
        ApplyRotationAndPostion(frontRight, wheelFR);
        ApplyRotationAndPostion(backLeft, wheelBL);
        ApplyRotationAndPostion(backRight, wheelBR);
    }

    private void ApplyRotationAndPostion(WheelCollider targetWheel, Transform wheel) // Updates the wheel's postion and rotation
    {
        targetWheel.ConfigureVehicleSubsteps(5, 12, 15);

        Vector3 pos;
        Quaternion rot;
        targetWheel.GetWorldPose(out pos, out rot);
        wheel.position = pos;
        wheel.rotation = rot;
    }
    void ApplySteering()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(PostionToFollow);
        float currentSteeringAngle = (relativeVector.x / relativeVector.magnitude) * MaxSteeringAngle;
        
        // Áp dụng góc lái mượt với tốc độ điều chỉnh chậm hơn
        float smoothSteeringAngle = Mathf.Lerp(frontLeft.steerAngle, currentSteeringAngle, Time.fixedDeltaTime * 3f);
        
        frontLeft.steerAngle = smoothSteeringAngle;
        frontRight.steerAngle = smoothSteeringAngle;
    }

   

   
    void Movement()
    {
        if (allowMovement)
        {
            float avgRPM = (frontLeft.rpm + frontRight.rpm + backLeft.rpm + backRight.rpm) / 4;

            // Cân bằng RPM giữa các bánh
            BalanceWheelRPM(frontLeft, avgRPM);
            BalanceWheelRPM(frontRight, avgRPM);
            BalanceWheelRPM(backLeft, avgRPM);
            BalanceWheelRPM(backRight, avgRPM);

            float torque = CalculateBalancedTorque();
            ApplyTorqueToWheels(torque);
        }
    }
    private void BalanceWheelRPM(WheelCollider wheel, float targetRPM)
    {
        if (wheel.rpm > targetRPM + rpmBalanceThreshold)
        {
            wheel.motorTorque *= 0.9f; // Giảm lực kéo nếu RPM quá cao
        }
        else if (wheel.rpm < targetRPM - rpmBalanceThreshold)
        {
            wheel.motorTorque *= 1.1f; // Tăng lực kéo nếu RPM quá thấp
        }
    }
    private float CalculateBalancedTorque()
    {
        float baseForce = accelerationForce * MovementTorque;
        return Mathf.Min(baseForce, maxForwardSpeed);
    }
    private void ApplyTorqueToWheels(float torque)
    {
        // Áp dụng lực đồng đều cho tất cả các bánh
        frontLeft.motorTorque = torque;
        frontRight.motorTorque = torque;
        backLeft.motorTorque = torque;
        backRight.motorTorque = torque;
    }
    void debug(string text, bool IsCritical)
    {
        if (Debugger)
        {
            if (IsCritical)
                Debug.LogError(text);
            else
                Debug.Log(text);
        }
    }


    private void OnDrawGizmos() // shows a Gizmos representing the waypoints and AI FOV
    {
        if (ShowGizmos == true)
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (i == currentWayPoint)
                    Gizmos.color = Color.blue;
                else
                {
                    if (i > currentWayPoint)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.green;
                }
                Gizmos.DrawWireSphere(waypoints[i], 2f);
            }
            CalculateFOV();
        }

        void CalculateFOV()
        {
            Gizmos.color = Color.white;
            float totalFOV = AIFOV * 2;
            float rayRange = 10.0f;
            float halfFOV = totalFOV / 2.0f;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * transform.forward;
            Vector3 rightRayDirection = rightRayRotation * transform.forward;
            Gizmos.DrawRay(carFront.position, leftRayDirection * rayRange);
            Gizmos.DrawRay(carFront.position, rightRayDirection * rayRange);
        }
    }
    void SetupCustomDestinationColliders()
    {
        if (CustomDestinations != null)
        {
            foreach (Transform destination in CustomDestinations)
            {
                Collider collider = destination.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.isTrigger = true; // Đặt là trigger
                    collider.enabled = true;    // Bật collider
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (CustomDestinations != null)
        {
            debug($"Trigger with: {other.name}", false);
            debug($"Current destination index: {currentDestinationIndex}/{CustomDestinations.Count}", false); // Thêm debug này

            if (other.transform == CustomDestinations[currentDestinationIndex])
            {
                debug($"Reached destination {currentDestinationIndex}", false);

                // Kiểm tra trước khi tăng index
                if (currentDestinationIndex == CustomDestinations.Count - 1)
                {
                    if (Patrol)
                    {
                        debug("Last point reached, resetting to first (Patrol mode)", false);
                        currentDestinationIndex = 0;
                    }
                    else
                    {
                        debug("Reached final destination (Non-Patrol mode)", false);
                        allowMovement = false;
                        return;
                    }
                }
                else
                {
                    currentDestinationIndex++;
                }

                debug($"Moving to destination {currentDestinationIndex}", false);
                debug($"Next destination: {CustomDestinations[currentDestinationIndex].position}", false);

                currentWayPoint = 0;
                waypoints.Clear();
                CreatePath();
            }
        }
    }
}
