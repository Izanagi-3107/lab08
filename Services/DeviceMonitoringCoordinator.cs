/********************
Mã sinh viên: 202418984
Họ tên: Ngô Ngọc Thái
********************/

namespace LabW08;

public class DeviceMonitoringCoordinator : IDisposable
{
    private readonly List<TeachingDevice> devices;
    private readonly ConsoleNotificationService notificationService;
    private readonly DeviceAuditLogger auditLogger;
    private readonly MaintenanceService maintenanceService;
    private readonly FilteredAlertSubscriber? filteredSubscriber;
    private bool subscribed;
    private bool disposed;

    public DeviceMonitoringCoordinator(IEnumerable<TeachingDevice> devices,
        ConsoleNotificationService notificationService, DeviceAuditLogger auditLogger,
        MaintenanceService maintenanceService,
        FilteredAlertSubscriber? filteredSubscriber = null)
    {
        ArgumentNullException.ThrowIfNull(devices);
        this.devices = new List<TeachingDevice>();
        foreach (TeachingDevice device in devices)
        {
            ArgumentNullException.ThrowIfNull(device);
            if (!this.devices.Any(item => ReferenceEquals(item, device)))
                this.devices.Add(device);
        }
        this.notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));
        this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        this.maintenanceService = maintenanceService
            ?? throw new ArgumentNullException(nameof(maintenanceService));
        this.filteredSubscriber = filteredSubscriber;
    }

    public void Subscribe()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (subscribed) return;

        foreach (TeachingDevice device in devices)
        {
            device.StatusChanged += auditLogger.HandleStatusChanged;
            device.AlertRaised += notificationService.HandleAlert;
            device.AlertRaised += maintenanceService.HandleCriticalAlert;
            if (filteredSubscriber != null)
                device.AlertRaised += filteredSubscriber.HandleAlert;
        }
        subscribed = true;
    }

    public void Unsubscribe()
    {
        if (!subscribed) return;
        foreach (TeachingDevice device in devices)
        {
            device.StatusChanged -= auditLogger.HandleStatusChanged;
            device.AlertRaised -= notificationService.HandleAlert;
            device.AlertRaised -= maintenanceService.HandleCriticalAlert;
            if (filteredSubscriber != null)
                device.AlertRaised -= filteredSubscriber.HandleAlert;
        }
        subscribed = false;
    }

    public void Dispose()
    {
        if (disposed) return;
        Unsubscribe();
        disposed = true;
    }
}
