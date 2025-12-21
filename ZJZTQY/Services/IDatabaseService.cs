
using ZJZTQY.Models;

namespace ZJZTQY.Services
{
    public interface IDatabaseService
    {
        Task<(bool IsSuccess, string Message, User? User)> LoginOrRegisterAsync(string email);
    }
}