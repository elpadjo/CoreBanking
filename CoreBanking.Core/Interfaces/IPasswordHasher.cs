using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Core.Interfaces
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        string VerifyHashedPassword(string hashedPassword, string providedPassword);
    }
}
