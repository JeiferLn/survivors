using UnityEngine;

public class CountdownEventData
{
    public ZoneId Zone;
    public float DeltaTime;

    public CountdownEventData(ZoneId zone, float deltaTime)
    {
        Zone = zone;
        DeltaTime = deltaTime;
    }
}
