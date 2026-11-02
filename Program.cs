/********************
Mã sinh viên: 202418984
Họ tên: Ngô Ngọc Thái
********************/

using System.Text;

namespace LabW08;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        if (args.Length == 1 && args[0] == "--test")
            return TestRunner.RunAll();
        if (args.Length != 0)
        {
            Console.Error.WriteLine("Cách chạy: dotnet run hoặc dotnet run -- --test");
            return 2;
        }

        Console.WriteLine("LAB W08 - DELEGATE VÀ EVENT");
        Console.WriteLine("Ngô Ngọc Thái - 202418984");
        var projector = new ProjectorRemote("PR01", "Điều khiển máy chiếu");
        var air = new AirConditionerRemote("AC01", "Điều khiển điều hòa", "D3-201");
        var microphone = new WirelessMicrophone("MC01", "Micro không dây");
        var notification = new ConsoleNotificationService();
        var audit = new DeviceAuditLogger();
        var maintenance = new MaintenanceService();
        var criticalOnly = new FilteredAlertSubscriber(
            alert => alert.Severity == AlertSeverity.Critical,
            alert => Console.WriteLine($"[BỘ LỌC CRITICAL] {alert.DeviceId}"));

        using var coordinator = new DeviceMonitoringCoordinator(
            new TeachingDevice[] { projector, air, microphone },
            notification, audit, maintenance, criticalOnly);
        coordinator.Subscribe();
        coordinator.Subscribe();

        Console.WriteLine("\n1. Thay đổi trạng thái");
        projector.ChangeStatus(DeviceStatus.Borrowed);
        projector.ChangeStatus(DeviceStatus.Borrowed);
        projector.ChangeStatus(DeviceStatus.Available);

        Console.WriteLine("\n2. Cảnh báo thiết bị");
        projector.UpdateBatteryLevel(20);
        projector.UpdateBatteryLevel(19);
        projector.UpdateBatteryLevel(10);
        projector.UpdateBatteryLevel(0);
        air.UpdateResponseState(false);
        air.UpdateResponseState(false);
        microphone.UpdateBatteryLevel(29);
        microphone.UpdateSignalQuality(39);
        microphone.UpdateSignalQuality(0);
        air.ChangeStatus(DeviceStatus.UnderMaintenance);

        Console.WriteLine("\n3. Dữ liệu không hợp lệ");
        try { microphone.UpdateBatteryLevel(101); }
        catch (InvalidDeviceReadingException ex) { Console.WriteLine(ex.Message); }

        Console.WriteLine($"\n4. Lịch sử trạng thái ({audit.Entries.Count})");
        foreach (string entry in audit.Entries) Console.WriteLine(entry);
        Console.WriteLine($"\n5. Yêu cầu bảo trì ({maintenance.Requests.Count})");
        foreach (string request in maintenance.Requests) Console.WriteLine(request);

        Console.WriteLine("\n6. Hủy đăng ký");
        coordinator.Unsubscribe();
        microphone.UpdateBatteryLevel(0);
        projector.ChangeStatus(DeviceStatus.Borrowed);
        Console.WriteLine($"Sau hủy: {audit.Entries.Count} nhật ký, " +
            $"{maintenance.Requests.Count} yêu cầu bảo trì.");
        return 0;
    }
}
