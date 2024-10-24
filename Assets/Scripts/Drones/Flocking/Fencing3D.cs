using UnityEngine;

public class Fencing3D {

    public Vector3 center = Vector3.zero;

    public float xMax = 9f;
    public float xMin = -8f;
    public float yMax = 2.5f; //NILS
    public float yMin = 1.0f; //NILS
    public float zMax = 5f;
    public float zMin = -4.1f;

    public Transform transform;

    public void Init(Transform transform) {
        this.transform = transform;
        var xRange = xMax - xMin;
        var xMiddle = xMin + xRange / 2;
        var yRange = yMax - yMin;
        var yMiddle = yMin + yRange / 2;
        var zRange = zMax - zMin;
        var zMiddle = zMin + zRange / 2;
        center = new Vector3(xMiddle, yMiddle, zMiddle);
    }

    public Vector3? GetCenterVectorWhenAtFence() {

        var x = transform.position.x;
        var y = transform.position.y;
        var z = transform.position.z;

        var xzCenter = center;
        xzCenter.y = transform.position.y;

        var yCenter = transform.position;
        yCenter.y = center.y;

        if (x > xMax || x < xMin || z > zMax || z < zMin)
        {
            var result = xzCenter - transform.position;
            result.Normalize();
            return result;
        } else if (y > yMax)
        {
            var result = transform.forward;
            result.y = -1f;
            result.Normalize();
            
            return result;
        }
        else if (y < yMin)
        {
            var result = transform.forward;
            result.y = 1f;
            result.Normalize();
            return result;
        }
        {
            return null;
        }

    }

    public Vector3 GetRandomPosition()
    {
        var x = Random.Range(xMin + 0.5f, xMax - 0.5f);
        var z = Random.Range(zMin + 0.5f, zMax - 0.5f);
        return new Vector3(x, 0, z);
    }

}

