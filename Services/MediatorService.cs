using System;
using System.Collections.Generic;

namespace ledger_vault.Services;

public class MediatorService<TMessage>
{
    #region PUBLIC API

    public void Subscribe(Action<TMessage> callback)
    {
        if (!_subscribers.Contains(callback))
            _subscribers.Add(callback);
    }

    public void Unsubscribe(Action<TMessage> callback)
    {
        _subscribers.Remove(callback);
    }

    public void Publish(TMessage message)
    {
        foreach (var callback in _subscribers)
            callback.Invoke(message);
    }

    #endregion

    #region PRIVATE PROPERTIES

    private readonly List<Action<TMessage>> _subscribers = [];

    #endregion
}