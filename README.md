# WorkManagement.ServiceDefaults

Thư viện [.NET Aspire ServiceDefaults](https://learn.microsoft.com/dotnet/aspire/fundamentals/service-defaults)
dùng chung cho **QuanLyCongViec**. Tách từ `server/ServiceDefaults` và `server/Tests/Api.UnitTests/ServiceDefaults`
(giữ lịch sử commit).

## API

Các extension method đặt trong namespace `Microsoft.Extensions.Hosting`:

```csharp
builder.AddServiceDefaults();
// ...
app.MapDefaultEndpoints(); // GET /health, GET /alive
```

- `AddServiceDefaults`: cấu hình OpenTelemetry (logging, metrics, tracing + OTLP exporter khi có `OTEL_EXPORTER_OTLP_ENDPOINT`), default health checks (`self`, tag `live`), service discovery, và HTTP resilience mặc định.
- `ConfigureOpenTelemetry`, `AddDefaultHealthChecks`, `MapDefaultEndpoints`: có thể gọi tách riêng khi cần.

## Build cục bộ

```bash
dotnet build WorkManagement.ServiceDefaults.slnx -c Release
dotnet test WorkManagement.ServiceDefaults.slnx -c Release --no-build
dotnet pack WorkManagement.ServiceDefaults.slnx -c Release --no-build -o out
```

## Phát hành & dùng trong server

Workflow [`.github/workflows/nuget.yml`](.github/workflows/nuget.yml): push/PR → build, test, pack (artifact `nuget`);
tag `vX.Y.Z` → GitHub Release kèm `.nupkg`/`.snupkg` **và** đẩy `.nupkg` lên GitHub Packages
(`https://nuget.pkg.github.com/phu-nam-hai-jsco/index.json`, private, xác thực bằng `GITHUB_TOKEN`).

Server dùng feed cục bộ `server/packages/` (xem `server/nuget.config`). Cập nhật server sau khi phát hành:

```bash
gh release download vX.Y.Z --repo phu-nam-hai-jsco/work-management-service-defaults --pattern "*.nupkg" --dir server/packages
```

rồi sửa `<PackageVersion Include="WorkManagement.ServiceDefaults" Version="X.Y.Z" />` trong `server/Directory.Packages.props`.

