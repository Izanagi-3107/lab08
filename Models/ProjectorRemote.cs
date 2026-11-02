/********************
Mã sinh viên: 202418984
Họ tên: Ngô Ngọc Thái
********************/

namespace LabW08;

public class ProjectorRemote : TeachingDevice
{
    public int BatteryLevel { get; private set; } = 100;

    public ProjectorRemote(string id, string name) : base(id, name)
    {
    }

    public void UpdateBatteryLevel(int newLevel)
    {
        ValidateReading(newLevel);
        int oldLevel = BatteryLevel;
        BatteryLevel = newLevel;
        RaiseReadingAlert(oldLevel, newLevel, 20, "Mức pin");
    }
}
