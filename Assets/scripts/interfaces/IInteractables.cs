public interface IOpenable
{
    void Open();
    void Close();
    bool IsOpen { get; }
    bool IsLocked { get; }
}

public interface ILockable
{
    void Lock();
    void Unlock();
    bool IsLocked { get; }
}

public interface IKeyLockable : ILockable
{
    string RequiredKeyId { get; }
    bool TryUnlock(string keyId);
    bool HasValidKey(string keyId);
}

public interface IInteractable
{
    void Interact();
    string GetInteractionText();
}
