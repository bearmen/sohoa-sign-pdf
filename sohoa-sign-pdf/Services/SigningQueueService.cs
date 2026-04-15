using sohoa_sign_pdf.Models;

namespace sohoa_sign_pdf.Services;

public sealed class SigningQueueService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly TokenService _tokenService;

    public SigningQueueService(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<SignResult> SignDataAsync(SignRequest request, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _tokenService.SignDataAsync(request.Data, request.CertId, request.Pin, request.HashAlgorithm, request.DataIsBase64, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<SignResult> SignHashAsync(SignHashRequest request, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _tokenService.SignHashAsync(request.HashBase64, request.CertId, request.Pin, request.HashAlgorithm, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
