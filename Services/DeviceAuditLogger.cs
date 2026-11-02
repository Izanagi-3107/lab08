/********************
Mã sinh viên: 202418984
Họ tên: Ngô Ngọc Thái
********************/

namespace LabW08;

public class DeviceAuditLogger
{
    private readonly List<string> entries = new();
    public IReadOnlyList<string> Entries => entries.AsReadOnly();

    public void HandleStatusChanged(object? sender, DeviceStatusChangedEventArgs e)
    {
        entries.Add($"{e.ChangedAt:dd/MM/yyyy HH:mm:ss}: {e.DeviceId}, " +
            $"{e.OldStatus} -> {e.NewStatus}");
    }
}
