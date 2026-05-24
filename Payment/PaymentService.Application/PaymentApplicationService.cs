using PaymentService.Application.Commands;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.ValueObjects;
using DentalClinic.SharedKernel.DomainEvents;
using DentalClinic.SharedKernel.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace PaymentService.Application;

public class PaymentApplicationService
{
    private readonly IPaymentRepository _repository;
    private readonly IValidator<CreatePaymentCommand> _createValidator;
    private readonly IValidator<AuthorizePaymentCommand> _authorizeValidator;
    private readonly IValidator<CapturePaymentCommand> _captureValidator;
    private readonly IValidator<RefundPaymentCommand> _refundValidator;
    private readonly ILogger<PaymentApplicationService> _logger;

    public PaymentApplicationService(
        IPaymentRepository repository,
        IValidator<CreatePaymentCommand> createValidator,
        IValidator<AuthorizePaymentCommand> authorizeValidator,
        IValidator<CapturePaymentCommand> captureValidator,
        IValidator<RefundPaymentCommand> refundValidator,
        ILogger<PaymentApplicationService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _authorizeValidator = authorizeValidator;
        _captureValidator = captureValidator;
        _refundValidator = refundValidator;
        _logger = logger;
    }

    public async Task HandleAppointmentConfirmed(AppointmentConfirmedEvent evt)
    {
        _logger.LogInformation(
            "Creating payment for appointment {AppointmentId}",
            evt.AppointmentId);

        var command = new CreatePaymentCommand(
            Guid.NewGuid(),
            evt.AppointmentId,
            500, // Fixed amount for MVP
            "DKK");

        await Handle(command);
    }

    public async Task Handle(CreatePaymentCommand command)
    {
        // Validate command
        var validationResult = await _createValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException(errors);
        }

        _logger.LogInformation(
            "Creating payment {PaymentId} for appointment {AppointmentId}",
            command.PaymentId,
            command.AppointmentId);

        try
        {
            var payment = new Payment(
                command.PaymentId,
                command.AppointmentId,
                new Money(command.Amount, command.Currency));

            await _repository.AddAsync(payment);

            _logger.LogInformation("Successfully created payment {PaymentId}", command.PaymentId);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Invalid amount when creating payment");
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException("Amount", ex.Message);
        }
    }

    public async Task Handle(AuthorizePaymentCommand command)
    {
        // Validate command
        var validationResult = await _authorizeValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException(errors);
        }

        _logger.LogInformation("Authorizing payment {PaymentId}", command.PaymentId);

        var payment = await _repository.GetByIdAsync(command.PaymentId)
            ?? throw new NotFoundException("Payment", command.PaymentId);

        try
        {
            payment.Authorize();
            await _repository.UpdateAsync(payment);

            _logger.LogInformation("Successfully authorized payment {PaymentId}", command.PaymentId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot authorize payment {PaymentId}", command.PaymentId);
            throw new BusinessRuleException(ex.Message, "CANNOT_AUTHORIZE_PAYMENT");
        }
    }

    public async Task Handle(CapturePaymentCommand command)
    {
        // Validate command
        var validationResult = await _captureValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException(errors);
        }

        _logger.LogInformation("Capturing payment {PaymentId}", command.PaymentId);

        var payment = await _repository.GetByIdAsync(command.PaymentId)
            ?? throw new NotFoundException("Payment", command.PaymentId);

        try
        {
            payment.Capture();
            await _repository.UpdateAsync(payment);

            _logger.LogInformation("Successfully captured payment {PaymentId}", command.PaymentId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot capture payment {PaymentId}", command.PaymentId);
            throw new BusinessRuleException(ex.Message, "CANNOT_CAPTURE_PAYMENT");
        }
    }

    public async Task Handle(RefundPaymentCommand command)
    {
        // Validate command
        var validationResult = await _refundValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException(errors);
        }

        _logger.LogInformation("Refunding payment {PaymentId}", command.PaymentId);

        var payment = await _repository.GetByIdAsync(command.PaymentId)
            ?? throw new NotFoundException("Payment", command.PaymentId);

        try
        {
            payment.Refund();
            await _repository.UpdateAsync(payment);

            _logger.LogInformation("Successfully refunded payment {PaymentId}", command.PaymentId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot refund payment {PaymentId}", command.PaymentId);
            throw new BusinessRuleException(ex.Message, "CANNOT_REFUND_PAYMENT");
        }
    }

    // Backward compatible methods (deprecated)
    [Obsolete("Use Handle(CreatePaymentCommand) instead")]
    public async Task HandleCreate(Guid paymentId, Guid appointmentId, decimal totalAmount)
    {
        await Handle(new CreatePaymentCommand(paymentId, appointmentId, totalAmount));
    }

    [Obsolete("Use Handle(AuthorizePaymentCommand) instead")]
    public async Task HandleAuthorize(Guid paymentId)
    {
        await Handle(new AuthorizePaymentCommand(paymentId));
    }

    [Obsolete("Use Handle(CapturePaymentCommand) instead")]
    public async Task HandleCapture(Guid paymentId)
    {
        await Handle(new CapturePaymentCommand(paymentId));
    }

    [Obsolete("Use Handle(RefundPaymentCommand) instead")]
    public async Task HandleRefund(Guid paymentId)
    {
        await Handle(new RefundPaymentCommand(paymentId));
    }
}
