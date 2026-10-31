namespace BlockHouse.Services;

public class BackendProcessService
{
    public async Task<bool> StartBackendWithHealthCheck(CancellationToken cancellationToken = default)
    {
        if (await BackendHost.IsReady(cancellationToken)) return true;
        if (await BackendHost.IsResponding(cancellationToken))
            throw new InvalidOperationException("Cổng 5050 đang chạy BE cũ hoặc API không tương thích. Dừng BE cũ rồi chạy BE mới (API /api/web/health phải trả api_version = 2), sau đó bấm Thử lại.");
        BackendHost.StartSupervisor();
        for (var i = 0; i < 40; i++)
        {
            await Task.Delay(500, cancellationToken);
            if (await BackendHost.IsReady(cancellationToken)) return true;
        }
        return false;
    }

    // Legacy callers may dispose their window; the shared BE must keep serving web users.
    public void KillBackendProcess() { }
}
