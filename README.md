# sohoa-sign-pdf

Ứng dụng `Windows Forms` chạy trên `.NET 8` để làm cầu nối giữa ứng dụng nội bộ và `USB Token` qua `PKCS#11`, cung cấp:

- giao diện desktop để theo dõi trạng thái token,
- local API chạy trên `127.0.0.1`,
- chức năng ký dữ liệu / ký hash / verify chữ ký,
- hàng đợi ký tuần tự để tránh xung đột khi nhiều request cùng vào,
- tự khởi động cùng Windows và chạy nền ở `system tray`.

> Lưu ý: ở trạng thái hiện tại, project **chưa ký file PDF trực tiếp**. Phần đang triển khai là dịch vụ ký dữ liệu/hash thông qua USB Token.

## 1. Chức năng chính

### 1.1. Quản lý USB Token
- Nạp thư viện `PKCS#11 DLL` từ đường dẫn cấu hình.
- Tự kiểm tra token có đang cắm hay không.
- Đọc thông tin token: label, serial, trạng thái đăng nhập.
- Đăng nhập token bằng `PIN`.
- Tự logout session khi hết thời gian timeout cấu hình.

### 1.2. Quản lý chứng thư số
- Liệt kê danh sách certificate có trên token.
- Hiển thị:
  - `Subject`
  - `SerialNumber`
  - `NotAfter`
  - `Id`
- Tự chọn certificate phù hợp nếu request không truyền `CertId`.

### 1.3. Ký dữ liệu
Hệ thống hiện hỗ trợ các kiểu ký sau:

- `raw`: dữ liệu text thường
- `json`: dữ liệu JSON dạng chuỗi
- `base64`: dữ liệu đã được encode base64
- `sign-hash`: ký trực tiếp một hash đã có sẵn
- `sign-batch`: ký nhiều item theo thứ tự

Thuật toán băm hiện hỗ trợ:
- `SHA256`
- `SHA1`

Cơ chế ký đang dùng:
- `CKM_SHA256_RSA_PKCS`
- `CKM_SHA1_RSA_PKCS`

### 1.4. Verify chữ ký
- Verify chữ ký RSA bằng public key lấy từ certificate trên token.
- Có kiểm tra chain certificate cơ bản bằng `X509Chain`.
- Revocation hiện đang để `NoCheck`.

### 1.5. Local API nội bộ
Ứng dụng tự host một local API qua `ASP.NET Core` trên địa chỉ:

- `http://127.0.0.1:<ApiPort>`

Các cơ chế bảo vệ hiện có:
- chỉ cho phép request từ `localhost / loopback`,
- kiểm tra header `X-Api-Key`,
- kiểm tra `Origin` nếu có cấu hình whitelist,
- giới hạn số request theo phút.

### 1.6. Chạy nền và tiện ích hệ thống
- Chỉ cho phép chạy `1 instance` duy nhất.
- Thu nhỏ xuống `system tray`.
- Có thể bật `Auto Start` cùng Windows.
- Ghi log ra thư mục `logs`.

## 2. Công nghệ sử dụng

- `.NET 8`
- `Windows Forms`
- `ASP.NET Core Minimal API`
- `Pkcs11Interop`
- `PKCS#11`

## 3. Cấu trúc project

```text
sohoa-sign-pdf/
├─ Configuration/
│  └─ AppConfig.cs            # Model cấu hình + load/save appsettings.local.json
├─ Models/
│  ├─ ApiModels.cs            # Request/response cho local API
│  └─ TokenModels.cs          # Model trạng thái token, certificate, sign result
├─ Services/
│  ├─ AppLogger.cs            # Ghi log file và đẩy log lên UI
│  ├─ AutoStartService.cs     # Bật/tắt auto start cùng Windows
│  ├─ LocalApiServer.cs       # Local API chạy trên 127.0.0.1
│  ├─ SigningQueueService.cs  # Hàng đợi ký tuần tự
│  └─ TokenService.cs         # Làm việc với USB Token qua PKCS#11
├─ Form1.cs                   # Logic giao diện chính
├─ Form1.Designer.cs          # Layout giao diện
├─ Program.cs                 # Entry point, DI thủ công và single instance
└─ sohoa-sign-pdf.csproj
```

## 4. Yêu cầu môi trường

Trước khi chạy, cần có:

- Windows
- `.NET 8 SDK` hoặc runtime phù hợp
- USB Token đã cài driver
- file `PKCS#11 DLL` của nhà cung cấp token

Ví dụ `PKCS#11 DLL` thường có dạng như:
- `eps2003csp11.dll`
- `aetpkss1.dll`
- hoặc DLL tương đương tùy nhà cung cấp

## 5. Cách chạy project

### 5.1. Build và chạy
Tại thư mục gốc project:

```powershell
dotnet build
```

Chạy bằng Visual Studio hoặc:

```powershell
dotnet run --project .\sohoa-sign-pdf\sohoa-sign-pdf.csproj
```

### 5.2. Lần chạy đầu tiên
Khi mở ứng dụng lần đầu:
- hệ thống sẽ tạo file cấu hình `appsettings.local.json` trong thư mục chạy ứng dụng,
- mặc định API chạy ở cổng `5005`.

## 6. Cấu hình ứng dụng

File cấu hình: `appsettings.local.json`

Ví dụ:

```json
{
  "ApiPort": 5005,
  "ApiKey": "change-me-local-api-key",
  "AllowedOrigins": [
    "http://localhost:3000",
    "https://localhost:3000"
  ],
  "TokenLibraryPath": "C:\\PKCS11\\your-token-pkcs11.dll",
  "AutoStart": false,
  "SessionTimeoutMinutes": 15,
  "RateLimitPerMinute": 30,
  "LogLevel": "Info",
  "DebugMode": false
}
```

### Ý nghĩa các trường

- `ApiPort`: cổng local API.
- `ApiKey`: khóa bảo vệ API, gửi qua header `X-Api-Key`.
- `AllowedOrigins`: danh sách origin được phép nếu request có gửi header `Origin`.
- `TokenLibraryPath`: đường dẫn đến `PKCS#11 DLL`.
- `AutoStart`: tự khởi động cùng Windows.
- `SessionTimeoutMinutes`: thời gian tự logout token nếu không hoạt động.
- `RateLimitPerMinute`: số request tối đa mỗi phút.
- `LogLevel`: mức log dự kiến dùng cho mở rộng sau này.
- `DebugMode`: cờ debug dự kiến dùng cho mở rộng sau này.

## 7. Hướng dẫn sử dụng giao diện

### Bước 1: cấu hình DLL và API key
Trên màn hình chính:
- chọn đường dẫn `PKCS#11 DLL`,
- nhập `API Port`,
- nhập `API Key`,
- khai báo `Allowed Origins` nếu cần,
- bấm `Save Config`.

### Bước 2: cắm token và kiểm tra trạng thái
Ứng dụng sẽ hiển thị:
- trạng thái API,
- trạng thái token,
- trạng thái login,
- label / serial của token.

### Bước 3: đăng nhập token
- nhập `PIN`,
- bấm `Login`.

### Bước 4: tải danh sách chứng thư
- bấm `Refresh` để load lại certificate trên token.

### Bước 5: ký thử dữ liệu
- chọn certificate trong danh sách,
- nhập nội dung cần ký ở vùng test payload,
- bấm `Sign Test`,
- kết quả chữ ký base64 sẽ hiển thị ở ô kết quả.

### Bước 6: chạy nền
- khi thu nhỏ, ứng dụng sẽ ẩn vào `system tray`.
- double click icon tray để mở lại cửa sổ.

## 8. API local hiện có

Base URL:

```text
http://127.0.0.1:5005
```

> Thay `5005` bằng giá trị `ApiPort` thực tế.

### 8.1. `GET /health`
Kiểm tra service còn sống.

Response mẫu:

```json
{
  "status": "ok",
  "apiPort": 5005,
  "version": "1.0.0"
}
```

### 8.2. `GET /token/status`
Lấy trạng thái token.

Header:
- `X-Api-Key: <your-api-key>`

### 8.3. `GET /certificates`
Lấy danh sách certificate trên token.

Header:
- `X-Api-Key: <your-api-key>`

### 8.4. `POST /sign`
Ký dữ liệu đầu vào.

Header:
- `Content-Type: application/json`
- `X-Api-Key: <your-api-key>`

Body mẫu:

```json
{
  "data": "hello world",
  "type": "raw",
  "certId": null,
  "pin": "12345678",
  "hashAlgorithm": "SHA256",
  "dataIsBase64": false
}
```

Ghi chú:
- `type` hiện hỗ trợ: `raw`, `json`, `base64`
- nếu `type = base64`, hệ thống sẽ tự hiểu dữ liệu đầu vào là base64
- nếu không truyền `certId`, hệ thống sẽ tự chọn certificate hợp lệ gần hết hạn nhất theo logic hiện tại

Response thành công:

```json
{
  "success": true,
  "signatureBase64": "...",
  "algorithm": "SHA256",
  "certificateId": "...",
  "error": null
}
```

### 8.5. `POST /sign-hash`
Ký trực tiếp hash đã được base64 encode.

Body mẫu:

```json
{
  "hashBase64": "...",
  "certId": null,
  "pin": "12345678",
  "hashAlgorithm": "SHA256"
}
```

### 8.6. `POST /sign-batch`
Ký nhiều item trong một request.

Body mẫu:

```json
{
  "items": [
    {
      "data": "document-1",
      "type": "raw",
      "pin": "12345678",
      "hashAlgorithm": "SHA256"
    },
    {
      "data": "document-2",
      "type": "raw",
      "pin": "12345678",
      "hashAlgorithm": "SHA256"
    }
  ]
}
```

### 8.7. `GET /get-public-key?certId=...`
Lấy public key của certificate.

### 8.8. `POST /verify`
Verify dữ liệu và chữ ký.

Body mẫu:

```json
{
  "data": "hello world",
  "signatureBase64": "...",
  "certId": null,
  "dataIsBase64": false,
  "hashAlgorithm": "SHA256"
}
```

Response mẫu:

```json
{
  "isValid": true,
  "chainValid": true,
  "error": null
}
```

## 9. Ví dụ gọi API bằng PowerShell

### Kiểm tra health

```powershell
Invoke-RestMethod -Uri "http://127.0.0.1:5005/health" -Method Get
```

### Lấy trạng thái token

```powershell
Invoke-RestMethod -Uri "http://127.0.0.1:5005/token/status" -Method Get -Headers @{
  "X-Api-Key" = "change-me-local-api-key"
}
```

### Ký dữ liệu text

```powershell
$body = @{
  data = "hello world"
  type = "raw"
  pin = "12345678"
  hashAlgorithm = "SHA256"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://127.0.0.1:5005/sign" -Method Post -ContentType "application/json" -Headers @{
  "X-Api-Key" = "change-me-local-api-key"
} -Body $body
```

## 10. Luồng xử lý chính của ứng dụng

1. `Program.cs` khởi tạo các service.
2. `Form1` load cấu hình và tự khởi động local API.
3. `LocalApiServer` mở API tại `127.0.0.1:<port>`.
4. Khi có request ký, API chuyển sang `SigningQueueService`.
5. `SigningQueueService` khóa tuần tự bằng `SemaphoreSlim`.
6. `TokenService` làm việc với token để login, chọn certificate và ký.
7. Kết quả trả về cho API/UI dưới dạng `base64`.

## 11. Log và chẩn đoán

Log được lưu trong thư mục:

```text
logs/
```

Tên file dạng:

```text
app-yyyyMMdd.log
```

Các sự kiện thường được ghi log:
- khởi động / dừng local API,
- login token,
- lỗi login,
- lỗi ký,
- timeout session.

## 12. Lưu ý bảo mật

Project hiện đã có một số lớp bảo vệ cơ bản, nhưng khi triển khai thực tế nên lưu ý thêm:

- đổi `ApiKey` mặc định ngay khi cài đặt,
- chỉ cấu hình `AllowedOrigins` thật sự cần thiết,
- không hard-code `PIN` trong ứng dụng client,
- hạn chế quyền truy cập máy chạy local signer,
- sao lưu log nhưng không ghi lộ dữ liệu nhạy cảm,
- nếu triển khai production, nên bổ sung thêm cơ chế audit và kiểm soát truy cập phía client gọi API.

## 13. Hạn chế hiện tại

Một số điểm chưa có hoặc mới ở mức MVP:

- chưa ký PDF trực tiếp,
- chưa có đóng gói installer,
- chưa có service Windows,
- verify hiện dựa trên certificate đang đọc từ token,
- chưa có quản lý nhiều token/song song nhiều thiết bị,
- `LogLevel` và `DebugMode` mới là cấu hình dự phòng, chưa dùng sâu trong code.

## 14. Gợi ý hướng phát triển tiếp

- bổ sung ký PDF/PAdES,
- thêm endpoint ký file hoặc ký hash chuẩn từ hệ thống khác,
- thêm audit log chi tiết theo request,
- mã hóa cấu hình nhạy cảm,
- thêm UI quản trị cấu hình tốt hơn,
- đóng gói thành installer hoặc background service.
