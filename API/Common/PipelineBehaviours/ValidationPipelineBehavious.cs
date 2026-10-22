using FluentValidation;
using SDET_POC.API.Common.Messaging;
using ValidationException = SDET_POC.API.Common.Exceptions.ValidationException;

namespace SDET_POC.API.Common.PipelineBehaviours;

internal static class ValidationPipelineBehaviour
{
    internal sealed class RequestHandler<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> _innerHandler,
        IEnumerable<IValidator<TRequest>> _validators
    ) : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v =>
                        v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .Where(r => r.Errors.Count != 0)
                    .SelectMany(r => r.Errors)
                    .ToList();

                if (failures.Count != 0)
                {
                    throw new ValidationException(failures);
                }
            }
            return await _innerHandler.Handle(request, cancellationToken);
        }
    }

    internal sealed class RequestBaseHandler<TRequest>(
        IRequestHandler<TRequest> _innerHandler,
        IEnumerable<IValidator<TRequest>> _validators
    ) : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        public async Task Handle(TRequest request, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v =>
                        v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .Where(r => r.Errors.Count != 0)
                    .SelectMany(r => r.Errors)
                    .ToList();

                if (failures.Count != 0)
                {
                    throw new ValidationException(failures);
                }
            }

            await _innerHandler.Handle(request, cancellationToken);
        }
    }
}

