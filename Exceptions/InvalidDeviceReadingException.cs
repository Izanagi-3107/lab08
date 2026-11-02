/********************
Mã sinh viên: 202418984
Họ tên: Ngô Ngọc Thái
********************/

namespace LabW08;

public class InvalidDeviceReadingException : Exception
{
    public string DeviceId { get; }
    public int InvalidValue { get; }

    public InvalidDeviceReadingException(string deviceId, int invalidValue)
        : base($"Thiết bị {deviceId}: giá trị {invalidValue} không thuộc khoảng 0 đến 100.")
    {
        DeviceId = deviceId;
        InvalidValue = invalidValue;
    }
}
