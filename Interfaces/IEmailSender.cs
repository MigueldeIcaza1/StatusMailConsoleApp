namespace Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Models;

    public interface IEmailSender
    {
        void SendEmail(List<StatusRecord> statusList);
    }
}
