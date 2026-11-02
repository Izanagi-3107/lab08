/********************
Mã sinh viên: 202418984
Họ tên: Ngô Ngọc Thái
********************/

namespace LabW08;

public class AirConditionerRemote : TeachingDevice
{
    public string AssignedRoom { get; }
    public bool IsResponsive { get; private set; } = true;

    public AirConditionerRemote(string id, string name, string assignedRoom)
        : base(id, name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assignedRoom);
        AssignedRoom = assignedRoom;
    }

    public void UpdateResponseState(bool isResponsive)
    {
        if (IsResponsive == isResponsive)
            return;

        IsResponsive = isResponsive;
        if (!isResponsive)
            RaiseAlert($"Điều khiển điều hòa phòng {AssignedRoom} không phản hồi.",
                AlertSeverity.Critical);
    }
}
