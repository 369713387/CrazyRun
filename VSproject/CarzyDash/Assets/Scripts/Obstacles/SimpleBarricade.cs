using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBarricade : Obstacle {

    protected const int k_MinObstacleCount = 1;
    protected const int k_MaxObstacleCount = 2;
    protected const int k_LeftMostLaneIndex = -1;
    protected const int k_RightMostLaneIndex = 1;

    public override void Spawn(TrackSegment segment, float t)
    {
        int count = Random.Range(k_MinObstacleCount, k_MaxObstacleCount + 1);
        int startLane = Random.Range(k_LeftMostLaneIndex, k_RightMostLaneIndex + 1);

        Vector3 position;
        Quaternion rotation;
        segment.GetPointAt(t, out position, out rotation);

        for(int i = 0;i < count; ++i)
        {
            int lane = startLane + i;
            lane = lane > k_RightMostLaneIndex ? k_LeftMostLaneIndex : lane;

            GameObject obj = Instantiate(gameObject, position, rotation);
            obj.transform.position += obj.transform.right * lane * segment.manager.laneOffset;

            obj.transform.SetParent(segment.objectRoot, true);
        }
    }
}
