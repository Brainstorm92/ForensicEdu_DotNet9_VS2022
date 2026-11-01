namespace ForensicEdu.Application.Common.Exceptions;

// Only explicitly unavailable mail configuration is translated to the recovery message.
public sealed class EmailDeliveryUnavailableException(string reason) : InvalidOperationException(reason);
