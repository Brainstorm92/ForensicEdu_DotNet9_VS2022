using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForensicEdu.Application.Common.Interfaces;

public interface IAccountLifecycleService
{
    Task ProcessDailyAccountExpirationsAsync(CancellationToken cancellationToken = default);
    Task ExtendValidityOnApprovedPaymentAsync(string userId, DateTime paymentDate, CancellationToken cancellationToken = default);
}
