using SDET_POC.API.Common.Messaging;

namespace SDET_POC.API.Common.PipelineBehaviours;

internal static class UnhandledExceptionPipelineBehaviour
{
    internal sealed class RequestHandler<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> _innerHandler,
        ILogger<RequestHandler<TRequest, TResponse>> _logger
    ) : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await _innerHandler.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogError(ex, "Unhandled Exception for Request {Name} {@Request}", requestName, request);

                throw;
            }
        }
    }

    internal sealed class RequestBaseHandler<TRequest>(
        IRequestHandler<TRequest> _innerHandler,
        ILogger<RequestBaseHandler<TRequest>> _logger
    ) : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        public async Task Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _innerHandler.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogError(ex, "Unhandled Exception for Request {Name} {@Request}", requestName, request);

                throw;
            }
        }
    }
}

