using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField] float sightAngle = 150f;
    [SerializeField] float sightAnglePlayer = 180f;
    [SerializeField] public float sightDistance = 30f;
    [SerializeField] float sightDistancePlayer = 7f;
    [SerializeField] float minDistanceThreshold = 3f;
    [SerializeField] Camera mainCamera;
    CombatTarget[] enemies;

    private void Start()
    {
        enemies = FindObjectsOfType<CombatTarget>();
    }

    private void Update()
    {

    }

    public CombatTarget LockOn()
    {
        List<CombatTarget> targets = GetAvailableTargets();
        if (targets.Count == 0)
        {
            return null;
        }
        if (targets.Count == 1)
        {
            return targets[0];
        }

        float minAngle = Mathf.Infinity;

        foreach (CombatTarget target in targets)
        {
            if (GetDistance(target) < minDistanceThreshold)
            {
                return target;
            }

            if (GetAngle(target) < minAngle) { minAngle = GetAngle(target); }
        }

        foreach (CombatTarget target in targets)
        {
            if (GetAngle(target) == minAngle)
            {
                return target;
            }
        }

        return null;
    }
    public CombatTarget SwitchTarget(CombatTarget currentTarget, Vector2 input)
    {
        List<CombatTarget> targets = GetAvailableTargets();

        if (targets.Count == 0 || targets.Count == 1) { return currentTarget; }
        if (targets.Count == 2)
        {
            foreach (CombatTarget target in targets)
            {
                if (currentTarget != target)
                {
                    return target;
                }
            }
        }

        float angleThreshold = 35;
        float minDistance = Mathf.Infinity;

        List<CombatTarget> targetsWithinAngle = new List<CombatTarget>();

        foreach (CombatTarget target in targets)
        {
            if (currentTarget == target) continue;

            float angle = GetAngleTarget(currentTarget, target, input);
            if (angle < angleThreshold) { targetsWithinAngle.Add(target); }
        }

        foreach (CombatTarget target in targetsWithinAngle)
        {
            float distanceBetweenTargets = (currentTarget.gameObject.transform.position - target.gameObject.transform.position).magnitude;

            if (distanceBetweenTargets < minDistance) { minDistance = distanceBetweenTargets; }
        }


        foreach (CombatTarget target in targetsWithinAngle)
        {
            float distanceBetweenTargets = (currentTarget.gameObject.transform.position - target.gameObject.transform.position).magnitude;

            if (distanceBetweenTargets == minDistance) { return target; }
        }
        return currentTarget;
    }

    private List<CombatTarget> GetAvailableTargets()
    {
        List<CombatTarget> availableTargets = new List<CombatTarget>();

        foreach (CombatTarget enemy in enemies)
        {
            if (enemy.gameObject == null) continue;

            if (GetDistance(enemy) > sightDistance) continue;

            float angleThresholdCamera = Mathf.Cos(sightAngle * 0.5f * Mathf.Deg2Rad);

            if (GetDotCamera(enemy) > angleThresholdCamera)
            {
                availableTargets.Add(enemy);
            }
            else if (InCameraFrustum(enemy) && GetDistance(enemy) < sightDistancePlayer)
            {
                float angleThresholdPlayer = Mathf.Cos(sightAnglePlayer * 0.5f * Mathf.Deg2Rad);
                if (GetDotPlayer(enemy) > angleThresholdPlayer)
                {
                    availableTargets.Add(enemy);
                }
                else if (GetDistance(enemy) < minDistanceThreshold)
                {
                    availableTargets.Add(enemy);
                }
            }
        }
        return availableTargets;
    }

    private Vector3 GetVectorToTarget(CombatTarget target)
    {
        return target.transform.position - transform.position;
    }

    private float GetDistance(CombatTarget target)
    {
        return GetVectorToTarget(target).magnitude;
    }

    private float GetDotCamera(CombatTarget target)
    {
        Vector3 CameraDirection = (transform.position - mainCamera.transform.position).normalized;
        return Vector3.Dot(CameraDirection, GetVectorToTarget(target).normalized);
    }

    private float GetDotPlayer(CombatTarget target)
    {
        return Vector3.Dot(transform.forward, GetVectorToTarget(target).normalized);
    }

    private bool InCameraFrustum(CombatTarget enemy)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        Renderer rend = enemy.gameObject.GetComponentInChildren<Renderer>();
        if (rend == null) return false;

        Bounds bounds = rend.bounds;

        if (GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
        {
            return true;
        }
        return false;
    }

    private float GetAngle(CombatTarget target)
    {
        Vector3 vectorToTarget = (target.transform.position - mainCamera.transform.position).normalized;
        Vector3 cameraForward = mainCamera.transform.forward;
        return Vector3.Angle(cameraForward, vectorToTarget);
    }

    private float GetAngleTarget(CombatTarget target1, CombatTarget target2, Vector2 input)
    {
        Vector3 screenPos1 = mainCamera.WorldToScreenPoint(target1.gameObject.transform.position);
        Vector2 screenXY1 = new Vector2(screenPos1.x, screenPos1.y);

        Vector3 screenPos2 = mainCamera.WorldToScreenPoint(target2.gameObject.transform.position);
        Vector2 screenXY2 = new Vector2(screenPos2.x, screenPos2.y);

        Vector2 vectorBetweenTargets = screenXY2 - screenXY1;
        vectorBetweenTargets.Normalize();

        return Vector2.Angle(vectorBetweenTargets, input.normalized);
    }
}
