/********************
Mã sinh viên: 202418984
Họ tên: Ngô Ngọc Thái
********************/

namespace LabW08;

public class MaintenanceService
{
    private readonly List<string> requests = new();
    public IReadOnlyList<string> Requests => requests.AsReadOnly();

    public void HandleCriticalAlert(object? sender, DeviceAlertEventArgs e)
    {
        if (e.Severity != AlertSeverity.Critical)
            return;

        requests.Add($"Yêu cầu bảo trì {e.DeviceId}: {e.Message}");
    }
}
