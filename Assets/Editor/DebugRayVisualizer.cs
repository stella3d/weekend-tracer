using Unity.Collections;
using UnityEngine;

namespace RayTracingWeekend
{
    public class DebugRayVisualizer
    {
        public Camera camera;
        
        public NativeArray<int> PixelRaySegmentCount;
        public NativeArray<Ray> RaySegments;

        public int PixelIndex { get; set; }
        int m_RaySegmentIndex;
        int m_CurrentPixelStartIndex;

        public void DrawCurrent(Color color)
        {
            var segmentCount = PixelRaySegmentCount[PixelIndex];
            m_CurrentPixelStartIndex = m_RaySegmentIndex;
            var endIndex = m_CurrentPixelStartIndex + segmentCount;
            for (int i = m_CurrentPixelStartIndex; i < endIndex; i++)
            {
                var segment = RaySegments[i];
                var nextRay = RaySegments[i + 1];
                Debug.DrawLine(segment.origin, nextRay.origin, color, 10f);
            }
        }

        public bool MoveNext()
        {
            if (PixelIndex >= PixelRaySegmentCount.Length) 
                return false;
            
            PixelIndex++;
            return true;

        }
    }
}