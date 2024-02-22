using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encircling
{
    public int Id { get; set; }
    public float MinDist { get; set; } = 1.2f;
    public float MaxDist { get; set; } = 3.4f;

    public List<ObstacleBoid> obstacleBoids;

    public Transform Transform;

    public void Init(int id, Transform transform, List<ObstacleBoid> obstacleBoids)
    {
        Id = id;
        this.Transform = transform;
        this.obstacleBoids = obstacleBoids;
    }

    public ObstacleBoid GetRandomObstacleBoid()
    {
        if (obstacleBoids.Count > 0)
        {
            return obstacleBoids[Random.Range(0, obstacleBoids.Count)];
        }
        else
        {
            return null;
        }
    }


    public Vector3? GetTargetPosition(Boid boid, float angle, float distance)
    {
        if (boid != null && Id != boid.Id)
        {

            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 position = direction * distance;
            return boid.Body.position + new Vector3(position.x, 0, position.y);
        }

        return null;
    }

    public Vector3 GetEncirclingVector(Boid boid)
    {
        Vector3 result = Vector3.zero;


        if (boid != null && Id != boid.Id)
        {

            Vector3 difBody = Transform.position - boid.Body.position;
            float distBody = Vector3.Magnitude(difBody);
            if (distBody > MaxDist)
            {
                result = boid.Body.position - Transform.position;
                result.Normalize();
                return new Vector3(result.x, 0, result.z);
            } else if (distBody <= MaxDist && distBody >= MinDist)
            {
                result = Vector3.Cross(boid.Body.position - Transform.position, Vector3.up);
                result.Normalize();
                return new Vector3(result.x, 0, result.z);
            } else
            {
                return result;
            }

        }

        return result;
    }


    public Vector3 GetEncirclingPosition(Boid boid, float angle, float distance)
    {
        Vector3 result = Vector3.zero;


            if (boid != null && Id != boid.Id)
            {

                if (boid.Leader != null)
                {
                    Vector3 difLeader = Transform.position - boid.Leader.position;
                    float distLeader = Vector3.Magnitude(difLeader);
                    if (distLeader <= MaxDist && distLeader >= MinDist)
                    {
                        result += boid.Leader.forward;
                    }
                }
            }
       
        return result;
    }

    public float GetRandomAngle()
    {
        return Random.Range(0, 2 * Mathf.PI);
    }

    public Vector2 GetRandomPosition2D(Transform referenceTransform, float minDistance, float maxDistance)
    {
        // Generate a random direction in 2D
        float randomAngle = Random.Range(0, 2 * Mathf.PI);
        Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

        // Randomize distance
        float randomDistance = Random.Range(minDistance, maxDistance);

        // Scale the random direction by the random distance
        Vector2 randomPosition = randomDirection * randomDistance;

        // Translate this random position based on the reference transform
        randomPosition = (Vector2)referenceTransform.position + randomPosition;

        return randomPosition;
    }

    
}
