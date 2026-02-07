using System.Threading.Tasks;

namespace BookwormsOnlineSecurity.Services
{
    public interface IRecaptchaValidator
    {
        Task<bool> IsValidAsync(string token, string action, string? remoteIp = null);
    }
}
