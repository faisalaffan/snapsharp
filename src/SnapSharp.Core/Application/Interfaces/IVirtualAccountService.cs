using SnapSharp.Application.Contracts.VirtualAccount;

namespace SnapSharp.Application.Interfaces;

public interface IVirtualAccountService
{
    Task<CreateVaResponse> CreateAsync(CreateVaRequest request, CancellationToken ct = default);
    Task<VAInquiryResponse> InquiryAsync(VAInquiryRequest request, CancellationToken ct = default);
    Task<VAPaymentNotifyResponse> PaymentNotifyAsync(VAPaymentNotifyRequest request, CancellationToken ct = default);
}
