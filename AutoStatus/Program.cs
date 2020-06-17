using Autofac;
using Interfaces;
using TfsProject;

namespace AutoStatus
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AutoStatusSender>().As<IAutoStatusSender>();
            builder.RegisterType<TfsService>().As<ITfsService>();
            builder.RegisterType<EmailSender.EmailSender>().As<IEmailSender>();
            var container = builder.Build();

            var statusSender = container.Resolve<IAutoStatusSender>();

            statusSender.SendStatus();
        }
    }
}
