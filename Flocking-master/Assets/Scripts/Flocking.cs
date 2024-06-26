using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using static UnityEngine.GraphicsBuffer;

public class Flocking : MonoBehaviour
{
    public GameObject Obstacle;
    public GameObject manager;
    public Vector3 baseRotation;

    [Range(0, 10)]
    public float maxSpeed = 1f;

    [Range(.1f, .5f)]
    public float maxForce = .03f;

    [Range(1, 10)]
    public float neighborhoodRadius = 3f;

    [Range(0, 3)]
    public float separationAmount = 1f;

    [Range(0, 3)]
    public float cohesionAmount = 1f;

    [Range(0, 3)]
    public float alignmentAmount = 1f;

    public Vector2 acceleration;
    public Vector2 velocity;
    public Vector2 desiredVelocity;
    private LayerMask obstacleLayerMask;
    private float avoidanceRadius = 3f;

    private Vector2 Position {
        get {
            return gameObject.transform.position;
        }
        set {
            gameObject.transform.position = value;
        }
    }

    

    private void Start()
    {
        float angle = Random.Range(0,  Mathf.PI);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + baseRotation);
        velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        
    }

    private void Awake()
    {
        // Set the obstacle layer using the LayerMask.GetMask function
        obstacleLayerMask = LayerMask.GetMask("Obstacle");
    }

    private void Update()
    {
        var boidColliders = Physics2D.OverlapCircleAll(Position, neighborhoodRadius);
        var boids = boidColliders.Select(o => o.GetComponent<Flocking>()).ToList();
        boids.Remove(this);

        var obstacleColliders = Physics2D.OverlapCircleAll(Position, neighborhoodRadius, obstacleLayerMask);
        var obstacles = obstacleColliders.Select(o => o.GetComponent<Collider2D>()).ToList();


        Flock(boids,obstacles);
        UpdateVelocity();
        UpdatePosition();
        UpdateRotation();

        
    }

    private void Flock(IEnumerable<Flocking> boids,IEnumerable<Collider2D> obstacles)
    {
        var alignment = Alignment(boids);
        var separation = Separation(boids);
        var cohesion = Cohesion(boids);


        acceleration = alignmentAmount * alignment + cohesionAmount * cohesion + separationAmount * separation;


        var avoidanceForce = AvoidObstacles(obstacles);
        acceleration += avoidanceForce * 0.8f;
       
        
        if (manager != null)
        {
            var hiddenLeader = manager.GetComponent<FishInstantiator>().hiddelLeader;
            Vector2 managerPos = new Vector2(hiddenLeader.transform.position.x, hiddenLeader.transform.position.y);
            desiredVelocity = (managerPos - Position).normalized * maxSpeed;
            var leaderFollowingAmount = 0.8f;
            acceleration += leaderFollowingAmount * Steer(desiredVelocity);
        }
    }


    public void UpdateVelocity()
    {
        velocity += acceleration;
        velocity = LimitMagnitude(velocity, maxSpeed);
    }

    private void UpdatePosition()
    {
        
 
        Position += velocity * Time.deltaTime;

    }

    private void UpdateRotation()
    {
        var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + baseRotation);
    }

    private Vector2 Alignment(IEnumerable<Flocking> boids)
    {
        var velocity = Vector2.zero;
        if (!boids.Any()) return velocity;

         foreach (var boid in boids)
        {
            if (boid != null)
            {
                velocity += boid.velocity;
            }
        }
        velocity /= boids.Count();

        var steer = Steer(velocity.normalized * maxSpeed);
        return steer;
    }

    private Vector2 Cohesion(IEnumerable<Flocking> boids)
    {
        if (!boids.Any()) return Vector2.zero;

        var sumPositions = Vector2.zero;
        foreach (var boid in boids)
        {
            if (boid != null)
            {
                sumPositions += boid.Position;
            }
        }
        var average = sumPositions / boids.Count();
        var direction = average - Position;

        var steer = Steer(direction.normalized * maxSpeed);
        return steer;
    }

    private Vector2 Separation(IEnumerable<Flocking> boids)
    {
        var direction = Vector2.zero;
        boids = boids.Where(o => DistanceTo(o) <= neighborhoodRadius / 2);
        if (!boids.Any()) return direction;

        foreach (var boid in boids)
        {
            var difference = Position - boid.Position;
            direction += difference.normalized / difference.magnitude;
        }
        direction /= boids.Count();

        var steer = Steer(direction.normalized * maxSpeed);
        return steer;
    }

    private Vector2 AvoidObstacles(IEnumerable<Collider2D> obstacles)
    {
        Vector2 avoidanceForce = Vector2.zero;

        foreach (var obstacle in obstacles)
        {
            Vector2 obstaclePosition = obstacle.transform.position;
            Vector2 boidPosition = Position;
            float distanceToObstacle = Vector2.Distance(boidPosition, obstaclePosition);
            Vector2 directionToObstacle = obstaclePosition - boidPosition;

            if (distanceToObstacle < avoidanceRadius) // You can define a suitable avoidance radius
            {
                avoidanceForce += -directionToObstacle.normalized * (1 - distanceToObstacle / avoidanceRadius);
            }
        }

        return avoidanceForce;
    }

    private Vector2 AvoidObstacle(GameObject obstacle)
    {
        var obstaclePosition = new Vector2(obstacle.transform.position.x, obstacle.transform.position.y);
        var boidPosition = this.Position;
        var distanceToObstacle = Vector3.Distance(boidPosition, obstaclePosition);
        var directionToObstacle = obstaclePosition - boidPosition;
        var desiredSeparation = neighborhoodRadius / 2;

        var avoidanceForce = directionToObstacle.normalized * desiredSeparation;

        return avoidanceForce;
    }

    private Vector2 Steer(Vector2 desired)
    {
        var steer = desired - velocity;
        steer = LimitMagnitude(steer, maxForce);

        return steer;
    }

    private float DistanceTo(Flocking boid)
    {
        if (boid == null)
            return float.MaxValue; // or some other appropriate value
        return Vector3.Distance(boid.transform.position, Position);
    }

    private Vector2 LimitMagnitude(Vector2 baseVector, float maxMagnitude)
    {
        if (baseVector.sqrMagnitude > maxMagnitude * maxMagnitude)
        {
            baseVector = baseVector.normalized * maxMagnitude;
        }
        return baseVector;
    }


}