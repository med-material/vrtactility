using System.Collections.Generic;

public interface ITouchable
{
    public ref readonly PressurePoint?[] GetAllPressurePoints();
    public ref readonly List<PressurePoint> GetActivePressurePoints();
    public int GetPressurePointCount();
}
