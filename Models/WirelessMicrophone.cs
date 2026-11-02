/********************
Mã sinh viên: 202418984
Họ tên: Ngô Ngọc Thái
********************/

namespace LabW08;

public class WirelessMicrophone : TeachingDevice
{
    public int BatteryLevel { get; private set; } = 100;
    public int SignalQuality { get; private set; } = 100;

    public WirelessMicrophone(string id, string name) : base(id, name)
    {
    }

    public void UpdateBatteryLevel(int newLevel)
    {
        ValidateReading(newLevel);
        int oldLevel = BatteryLevel;
        BatteryLevel = newLevel;
        RaiseReadingAlert(oldLevel, newLevel, 30, "Mức pin");
    }

    public void UpdateSignalQuality(int newQuality)
    {
        ValidateReading(newQuality);
        int oldQuality = SignalQuality;
        SignalQuality = newQuality;
        RaiseReadingAlert(oldQuality, newQuality, 40, "Chất lượng tín hiệu");
    }
}
