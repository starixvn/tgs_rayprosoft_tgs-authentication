using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Tgs.Authentication.Models;

public class TicketDataProtector : IDataProtector, IDisposable
{
    private readonly Aes _encryptor;

    private bool _disposedValue;

    public TicketDataProtector(byte[] key, byte[] iv)
    {
        _encryptor = Aes.Create();
        _encryptor.Key = key;
        _encryptor.IV = iv;
        _encryptor.Mode = CipherMode.CBC;
        _encryptor.Padding = PaddingMode.PKCS7;
    }

    public byte[] Protect(byte[] plaintext)
    {
        var cryptor = _encryptor.CreateEncryptor();

        return cryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
    }

    public byte[] Unprotect(byte[] protectedData)
    {
        var cryptor = _encryptor.CreateDecryptor();

        return cryptor.TransformFinalBlock(protectedData, 0, protectedData.Length);
    }

    public IDataProtector CreateProtector(string purpose) => this;

    public void Dispose()
    {
        // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
                _encryptor.Dispose();

            _disposedValue = true;
        }
    }
}