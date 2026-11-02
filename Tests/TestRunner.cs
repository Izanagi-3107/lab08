/********************
Mã sinh viên: 202418984
Họ tên: Ngô Ngọc Thái
********************/

namespace LabW08;

internal static class TestRunner
{
    private static int passed;
    private static int failed;

    private static ProjectorRemote Projector() => new("P1", "Máy chiếu");
    private static WirelessMicrophone Microphone() => new("M1", "Micro");
    private static List<DeviceAlertEventArgs> Track(TeachingDevice device)
    {
        var alerts = new List<DeviceAlertEventArgs>();
        device.AlertRaised += (_, e) => alerts.Add(e);
        return alerts;
    }

    private static void Check(bool condition, string message = "Sai kết quả")
    {
        if (!condition) throw new Exception(message);
    }

    private static T Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T ex) { return ex; }
        throw new Exception($"Không phát sinh {typeof(T).Name}");
    }

    private static void Test(string name, Action action)
    {
        try
        {
            action();
            passed++;
            Console.WriteLine($"PASS | {name}");
        }
        catch (Exception ex)
        {
            failed++;
            Console.WriteLine($"FAIL | {name}: {ex.Message}");
        }
    }

    private static DeviceMonitoringCoordinator Coordinator(TeachingDevice device,
        DeviceAuditLogger audit, MaintenanceService maintenance,
        FilteredAlertSubscriber? filter = null)
    {
        return new DeviceMonitoringCoordinator(new[] { device },
            new ConsoleNotificationService(), audit, maintenance, filter);
    }

    public static int RunAll()
    {
        passed = failed = 0;
        Console.WriteLine("KIỂM THỬ LAB W08 - Ngô Ngọc Thái - 202418984");
        Test("T01 Khởi tạo thiết bị", () =>
        {
            var p = Projector(); var m = Microphone();
            var a = new AirConditionerRemote("A1", "Điều hòa", "D3");
            Check(p.Status == DeviceStatus.Available && p.BatteryLevel == 100);
            Check(m.BatteryLevel == 100 && m.SignalQuality == 100);
            Check(a.IsResponsive && a.AssignedRoom == "D3");
        });
        Test("T02 Không có subscriber", () =>
        {
            var p = Projector();
            p.ChangeStatus(DeviceStatus.Borrowed);
            p.UpdateBatteryLevel(0);
            Check(p.Status == DeviceStatus.Borrowed && p.BatteryLevel == 0);
        });
        Test("T03 Gán lại cùng trạng thái", () =>
        {
            var p = Projector(); int count = 0;
            p.StatusChanged += (_, _) => count++;
            p.ChangeStatus(DeviceStatus.Available);
            p.ChangeStatus(DeviceStatus.Borrowed);
            p.ChangeStatus(DeviceStatus.Borrowed);
            Check(count == 1);
        });
        Test("T04 Trạng thái và dữ liệu event", () =>
        {
            var p = Projector(); int count = 0;
            DateTime before = DateTime.Now;
            p.StatusChanged += (sender, e) =>
            {
                count++;
                Check(ReferenceEquals(sender, p) && e.DeviceId == "P1");
                Check(e.OldStatus == DeviceStatus.Available);
                Check(e.NewStatus == DeviceStatus.UnderMaintenance);
                Check(p.Status == e.NewStatus);
                Check(e.ChangedAt >= before && e.ChangedAt <= DateTime.Now);
            };
            p.ChangeStatus(DeviceStatus.UnderMaintenance);
            Check(count == 1);
        });
        Test("T05 Từ chối trạng thái enum không hợp lệ", () =>
        {
            var p = Projector(); int count = 0;
            p.StatusChanged += (_, _) => count++;
            Throws<ArgumentOutOfRangeException>(() => p.ChangeStatus((DeviceStatus)99));
            Check(p.Status == DeviceStatus.Available && count == 0);
        });
        Test("T06 Máy chiếu tại ngưỡng 20 và 19", () =>
        {
            var p = Projector(); var log = Track(p);
            p.UpdateBatteryLevel(20); Check(log.Count == 0);
            p.UpdateBatteryLevel(19);
            Check(log.Count == 1 && log[0].Severity == AlertSeverity.Warning);
        });
        Test("T07 Máy chiếu không cảnh báo lặp cùng vùng", () =>
        {
            var p = Projector(); var log = Track(p);
            foreach (int x in new[] { 80, 20, 19, 10, 1, 1 }) p.UpdateBatteryLevel(x);
            Check(log.Count == 1);
        });
        Test("T08 Máy chiếu cạn pin và cập nhật 0 lặp lại", () =>
        {
            var p = Projector(); var log = Track(p);
            p.UpdateBatteryLevel(0); p.UpdateBatteryLevel(0);
            Check(log.Count == 1 && log[0].Severity == AlertSeverity.Critical);
        });
        Test("T09 Phục hồi pin và tái cảnh báo", () =>
        {
            var p = Projector(); var log = Track(p);
            foreach (int x in new[] { 19, 0, 10, 100, 19 }) p.UpdateBatteryLevel(x);
            Check(log.Select(e => e.Severity).SequenceEqual(new[] {
                AlertSeverity.Warning, AlertSeverity.Critical,
                AlertSeverity.Warning, AlertSeverity.Warning }));
        });
        Test("T10 Dữ liệu cảnh báo và cập nhật trước event", () =>
        {
            var p = Projector(); int count = 0;
            DateTime before = DateTime.Now;
            p.AlertRaised += (sender, e) =>
            {
                count++;
                Check(ReferenceEquals(sender, p) && p.BatteryLevel == 0);
                Check(e.DeviceId == "P1" && e.Message.Contains("0%"));
                Check(e.OccurredAt >= before && e.OccurredAt <= DateTime.Now);
            };
            p.UpdateBatteryLevel(0); Check(count == 1);
        });
        Test("T11 Máy chiếu từ chối pin ngoài khoảng", () =>
        {
            var p = Projector(); var log = Track(p);
            foreach (int x in new[] { -1, 101, int.MinValue, int.MaxValue })
            {
                var ex = Throws<InvalidDeviceReadingException>(() => p.UpdateBatteryLevel(x));
                Check(ex.DeviceId == "P1" && ex.InvalidValue == x);
            }
            Check(p.BatteryLevel == 100 && log.Count == 0);
        });
        Test("T12 Điều hòa mất phản hồi không lặp", () =>
        {
            var a = new AirConditionerRemote("A1", "Điều hòa", "D3");
            var log = Track(a);
            a.UpdateResponseState(true); Check(log.Count == 0);
            a.UpdateResponseState(false); a.UpdateResponseState(false);
            Check(log.Count == 1 && log[0].Severity == AlertSeverity.Critical);
            Check(!a.IsResponsive);
        });
        Test("T13 Điều hòa phục hồi rồi mất phản hồi", () =>
        {
            var a = new AirConditionerRemote("A1", "Điều hòa", "D3");
            var log = Track(a);
            a.UpdateResponseState(false); a.UpdateResponseState(true);
            Check(log.Count == 1 && a.IsResponsive);
            a.UpdateResponseState(false); Check(log.Count == 2);
        });
        Test("T14 Pin micro tại 30 và 29", () =>
        {
            var m = Microphone(); var log = Track(m);
            m.UpdateBatteryLevel(30); Check(log.Count == 0);
            m.UpdateBatteryLevel(29);
            Check(log.Count == 1 && log[0].Severity == AlertSeverity.Warning);
        });
        Test("T15 Tín hiệu micro tại 40 và 39", () =>
        {
            var m = Microphone(); var log = Track(m);
            m.UpdateSignalQuality(40); Check(log.Count == 0);
            m.UpdateSignalQuality(39);
            Check(log.Count == 1 && log[0].Severity == AlertSeverity.Warning);
        });
        Test("T16 Pin và tín hiệu micro độc lập", () =>
        {
            var m = Microphone(); var log = Track(m);
            m.UpdateBatteryLevel(0); m.UpdateSignalQuality(0);
            Check(log.Count == 2 && log.All(e => e.Severity == AlertSeverity.Critical));
        });
        Test("T17 Micro không cảnh báo lặp cùng vùng", () =>
        {
            var m = Microphone(); var log = Track(m);
            foreach (int x in new[] { 29, 10, 0, 0 }) m.UpdateBatteryLevel(x);
            foreach (int x in new[] { 39, 10, 0, 0 }) m.UpdateSignalQuality(x);
            Check(log.Count == 4);
        });
        Test("T18 Micro từ chối dữ liệu ngoài khoảng", () =>
        {
            var m = Microphone(); var log = Track(m);
            foreach (int x in new[] { -1, 101 })
            {
                Throws<InvalidDeviceReadingException>(() => m.UpdateBatteryLevel(x));
                Throws<InvalidDeviceReadingException>(() => m.UpdateSignalQuality(x));
            }
            Check(m.BatteryLevel == 100 && m.SignalQuality == 100 && log.Count == 0);
        });
        Test("T19 Bộ lọc delegate chỉ nhận Critical", () =>
        {
            var p = Projector(); int calls = 0;
            var filter = new FilteredAlertSubscriber(
                e => e.Severity == AlertSeverity.Critical, _ => calls++);
            p.AlertRaised += filter.HandleAlert;
            p.UpdateBatteryLevel(19); p.UpdateBatteryLevel(0);
            Check(calls == 1);
        });
        Test("T20 Nhiều subscriber và hủy riêng một handler", () =>
        {
            var p = Projector(); int a = 0, b = 0;
            EventHandler<DeviceAlertEventArgs> first = (_, _) => a++;
            p.AlertRaised += first;
            p.AlertRaised += (_, _) => b++;
            p.UpdateBatteryLevel(19);
            p.AlertRaised -= first;
            p.UpdateBatteryLevel(0);
            Check(a == 1 && b == 2);
        });
        Test("T21 Bảo trì chỉ nhận mức Critical", () =>
        {
            var service = new MaintenanceService();
            foreach (AlertSeverity s in Enum.GetValues<AlertSeverity>())
                service.HandleCriticalAlert(null, new DeviceAlertEventArgs("P1", "Lỗi", s, DateTime.Now));
            Check(service.Requests.Count == 1);
        });
        Test("T22 Coordinator đăng ký hai lần không trùng", () =>
        {
            var p = Projector(); var audit = new DeviceAuditLogger();
            var maintenance = new MaintenanceService();
            using var c = Coordinator(p, audit, maintenance);
            c.Subscribe(); c.Subscribe();
            p.ChangeStatus(DeviceStatus.Borrowed); p.UpdateBatteryLevel(0);
            Check(audit.Entries.Count == 1 && maintenance.Requests.Count == 1);
        });
        Test("T23 Thiết bị trùng tham chiếu trong danh sách", () =>
        {
            var p = Projector(); var audit = new DeviceAuditLogger();
            using var c = new DeviceMonitoringCoordinator(new[] { p, p },
                new ConsoleNotificationService(), audit, new MaintenanceService());
            c.Subscribe(); p.ChangeStatus(DeviceStatus.Borrowed);
            Check(audit.Entries.Count == 1);
        });
        Test("T24 Hủy đăng ký và giữ handler bên ngoài", () =>
        {
            var p = Projector(); var audit = new DeviceAuditLogger();
            var maintenance = new MaintenanceService(); int outside = 0, filtered = 0;
            p.AlertRaised += (_, _) => outside++;
            var f = new FilteredAlertSubscriber(_ => true, _ => filtered++);
            using var c = Coordinator(p, audit, maintenance, f);
            c.Subscribe(); c.Unsubscribe(); c.Unsubscribe();
            p.ChangeStatus(DeviceStatus.Borrowed); p.UpdateBatteryLevel(0);
            Check(audit.Entries.Count == 0 && maintenance.Requests.Count == 0);
            Check(outside == 1 && filtered == 0);
        });
        Test("T25 Đăng ký lại sau hủy", () =>
        {
            var p = Projector(); var audit = new DeviceAuditLogger();
            using var c = Coordinator(p, audit, new MaintenanceService());
            c.Unsubscribe(); c.Subscribe(); c.Unsubscribe(); c.Subscribe();
            p.ChangeStatus(DeviceStatus.Borrowed); Check(audit.Entries.Count == 1);
        });
        Test("T26 Dispose hủy mọi handler do coordinator tạo", () =>
        {
            var p = Projector(); var audit = new DeviceAuditLogger();
            var maintenance = new MaintenanceService(); int filtered = 0;
            var f = new FilteredAlertSubscriber(_ => true, _ => filtered++);
            var c = Coordinator(p, audit, maintenance, f);
            c.Subscribe(); c.Dispose(); c.Dispose();
            p.ChangeStatus(DeviceStatus.Borrowed); p.UpdateBatteryLevel(0);
            Check(audit.Entries.Count == 0 && maintenance.Requests.Count == 0 && filtered == 0);
            Throws<ObjectDisposedException>(() => c.Subscribe());
        });
        Test("T27 Danh sách log và bảo trì chỉ đọc", () =>
        {
            var audit = new DeviceAuditLogger(); var maintenance = new MaintenanceService();
            Throws<NotSupportedException>(() => ((IList<string>)audit.Entries).Add("Sai"));
            Throws<NotSupportedException>(() => ((IList<string>)maintenance.Requests).Add("Sai"));
        });
        Test("T28 Subscriber ném lỗi theo cơ chế event đồng bộ", () =>
        {
            var p = Projector(); bool nextCalled = false;
            p.AlertRaised += (_, _) => throw new InvalidOperationException("Lỗi handler");
            p.AlertRaised += (_, _) => nextCalled = true;
            Throws<InvalidOperationException>(() => p.UpdateBatteryLevel(0));
            Check(p.BatteryLevel == 0 && !nextCalled);
        });
        Test("T29 Kiểm tra tham số khởi tạo", () =>
        {
            Throws<ArgumentException>(() => new ProjectorRemote(" ", "Máy chiếu"));
            Throws<ArgumentException>(() => new AirConditionerRemote("A1", "Điều hòa", ""));
            Throws<ArgumentNullException>(() => new FilteredAlertSubscriber(null!, _ => { }));
            Throws<ArgumentNullException>(() => new FilteredAlertSubscriber(_ => true, null!));
        });
        Test("T30 Tích hợp nhiều thiết bị", () =>
        {
            var p = Projector(); var m = Microphone();
            var a = new AirConditionerRemote("A1", "Điều hòa", "D3");
            var audit = new DeviceAuditLogger(); var maintenance = new MaintenanceService();
            int filtered = 0;
            using var c = new DeviceMonitoringCoordinator(new TeachingDevice[] { p, a, m },
                new ConsoleNotificationService(), audit, maintenance,
                new FilteredAlertSubscriber(e => e.Severity == AlertSeverity.Critical, _ => filtered++));
            c.Subscribe();
            p.ChangeStatus(DeviceStatus.Borrowed); m.ChangeStatus(DeviceStatus.Borrowed);
            p.UpdateBatteryLevel(0); a.UpdateResponseState(false); m.UpdateSignalQuality(0);
            Check(audit.Entries.Count == 2 && maintenance.Requests.Count == 3 && filtered == 3);
        });
        Console.WriteLine($"\nKẾT QUẢ: {passed} PASS, {failed} FAIL");
        return failed == 0 ? 0 : 1;
    }
}
