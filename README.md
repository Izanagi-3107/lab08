# Lab W08 - Delegate và Event

**Ngô Ngọc Thái - 202418984**  
Môn học: Lập trình hướng đối tượng.

Chương trình Console giám sát điều khiển máy chiếu, điều khiển điều hòa và micro không dây.
Thiết bị phát sự kiện; các dịch vụ nhận sự kiện thực hiện thông báo, ghi lịch sử và tạo yêu cầu bảo trì.

## Chạy chương trình

Cần .NET SDK 8.0. Mở Terminal trong thư mục chứa `LabW08.csproj`:

```sh
dotnet build
dotnet run
dotnet run -- --test
```

Trên Windows có thể chạy `run.bat` để xem minh họa, `test.bat` để kiểm thử.
Trong Visual Studio, mở `LabW08.csproj`. Chương trình chạy kịch bản tự động, không yêu cầu nhập dữ liệu.
Không dùng gói NuGet bên ngoài; `NuGet.Config` bỏ nguồn tải gói để việc build không cần kết nối NuGet.

## Cấu trúc

- `Models/`: enum và các lớp thiết bị.
- `Events/`: hai lớp dữ liệu sự kiện và delegate `AlertFilter`.
- `Services/`: dịch vụ thông báo, nhật ký, bảo trì, lọc và điều phối đăng ký.
- `Exceptions/`: ngoại lệ chỉ số thiết bị ngoài khoảng 0-100.
- `Tests/TestRunner.cs`: 30 ca kiểm thử tự động, trả exit code 1 nếu có ca lỗi.
- `Program.cs`: kịch bản chạy minh họa và lựa chọn chế độ kiểm thử.
- `docs/`: báo cáo, sơ đồ UML và kết quả chạy.

## Quy tắc xử lý

- Mức pin và tín hiệu ban đầu là 100; điều hòa ban đầu có phản hồi.
- Pin máy chiếu: 20-100 bình thường; 1-19 Warning; 0 Critical.
- Pin micro: 30-100 bình thường; 1-29 Warning; 0 Critical.
- Tín hiệu micro: 40-100 bình thường; 1-39 Warning; 0 Critical.
- Chỉ cảnh báo khi chuyển sang một vùng bất thường khác. Ví dụ 19 -> 10 không lặp;
  10 -> 0 phát Critical; 0 -> 10 phát Warning; trở về vùng bình thường không cảnh báo.
- Pin và tín hiệu của micro được xét độc lập.
- Điều hòa chỉ cảnh báo khi chuyển từ có phản hồi sang không phản hồi.
- Dữ liệu sai bị từ chối trước khi đổi giá trị và không phát sự kiện.
- Bảo trì chỉ tạo yêu cầu khi nhận Critical, không tự đổi trạng thái thiết bị.
- Coordinator không đăng ký trùng; bỏ trùng tham chiếu thiết bị; hủy đúng handler đã đăng ký.
  Sau Dispose không cho Subscribe lại. Có thể Subscribe lại sau Unsubscribe khi chưa Dispose.
- Dùng cơ chế event đồng bộ mặc định: nếu một handler ném ngoại lệ, ngoại lệ truyền về bên gọi,
  các handler phía sau không chạy; trạng thái đã cập nhật không bị hoàn tác. Ca T28 kiểm tra hành vi này.
- Danh sách log và yêu cầu bảo trì chỉ lưu trong bộ nhớ, mất khi chương trình kết thúc.

## Kết quả kiểm thử

Đã build bằng .NET SDK 8.0.100 trên Linux: 0 lỗi, 0 cảnh báo.
Bộ kiểm thử: **30 PASS, 0 FAIL**. Chi tiết ở `docs/ket_qua_kiem_thu.txt`.
Kịch bản minh họa tạo 3 bản ghi trạng thái và 3 yêu cầu bảo trì.
Sau khi hủy đăng ký, cập nhật thiết bị không làm tăng hai danh sách này.

## Nộp bài

1. Đưa nội dung thư mục dự án lên repository GitHub; không tải `bin/` hoặc `obj/`.
2. Mở `docs/Bao_cao_LabW08_NgoNgocThai_202418984.docx`, điền URL repository ở dòng Link GitHub.
3. Nộp báo cáo Word theo yêu cầu của giảng viên.
