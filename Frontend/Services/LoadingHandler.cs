namespace Frontend.Services;

public class LoadingHandler : DelegatingHandler
{
    private readonly LoadingState _loadingState;

    public LoadingHandler(LoadingState loadingState)
    {
        _loadingState = loadingState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _loadingState.Increment();
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            _loadingState.Decrement();
        }
    }
}