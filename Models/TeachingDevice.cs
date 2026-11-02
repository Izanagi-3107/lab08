/********************
Mã sinh viên: 202418984
Họ tên: Ngô Ngọc Thái
********************/

namespace LabW08;

public abstract class TeachingDevice
{
    public string Id { get; }
    public string Name { get; }
    public DeviceStatus Status { get; private set; }

    public event EventHandler<DeviceStatusChangedEventArgs>? StatusChanged;
    public event EventHandler<DeviceAlertEventArgs>? AlertRaised;

    protected TeachingDevice(string id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id;
        Name = name;
        Status = DeviceStatus.Available;
    }

    public void ChangeStatus(DeviceStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
            throw new ArgumentOutOfRangeException(nameof(newStatus));
        if (newStatus == Status)
            return;

        DeviceStatus oldStatus = Status;
        // Subscriber phải đọc được trạng thái mới khi xử lý sự kiện.
        Status = newStatus;
        OnStatusChanged(new DeviceStatusChangedEventArgs(
            Id, oldStatus, newStatus, DateTime.Now));
    }

    protected void ValidateReading(int value)
    {
        if (value < 0 || value > 100)
            throw new InvalidDeviceReadingException(Id, value);
    }

    protected void RaiseReadingAlert(int oldValue, int newValue,
        int warningThreshold, string readingName)
    {
        int oldRegion = GetRegion(oldValue, warningThreshold);
        int newRegion = GetRegion(newValue, warningThreshold);
        // Chỉ cảnh báo khi chuyển sang một vùng bất thường khác.
        if (oldRegion == newRegion || newRegion == 0)
            return;

        RaiseAlert($"{readingName}: {newValue}%.",
            newRegion == 2 ? AlertSeverity.Critical : AlertSeverity.Warning);
    }

    private static int GetRegion(int value, int threshold)
    {
        if (value == 0) return 2;
        return value < threshold ? 1 : 0;
    }

    protected void RaiseAlert(string message, AlertSeverity severity)
    {
        OnAlertRaised(new DeviceAlertEventArgs(Id, message, severity, DateTime.Now));
    }

    protected virtual void OnStatusChanged(DeviceStatusChangedEventArgs e)
    {
        StatusChanged?.Invoke(this, e);
    }

    protected virtual void OnAlertRaised(DeviceAlertEventArgs e)
    {
        AlertRaised?.Invoke(this, e);
    }
}
