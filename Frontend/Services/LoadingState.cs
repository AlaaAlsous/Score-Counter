namespace Frontend.Services;

public class LoadingState
{
    private int _pendingRequests;

    public bool IsLoading => _pendingRequests > 0;

    public event Action? OnChanged;

    public void Increment()
    {
        var wasZero = _pendingRequests == 0;
        _pendingRequests++;
        if (wasZero)
            OnChanged?.Invoke();
    }

    public void Decrement()
    {
        if (_pendingRequests > 0)
        {
            _pendingRequests--;
            if (_pendingRequests == 0)
                OnChanged?.Invoke();
        }
    }
}