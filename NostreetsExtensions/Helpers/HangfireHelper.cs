using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;

namespace NostreetsExtensions.Helpers
{
    public class HangfireHelper
    {
        public static void Register(Action action, string cronExp = null)
        {
            RecurringJob.AddOrUpdate(() => action(), cronExp ?? Cron.Daily());
        }

        public static void Register(Func<Task> method, string cronExp = null)
        {
            RecurringJob.AddOrUpdate(() => method(), cronExp ?? Cron.Daily());
        }

        public static void Register<T>(Action<T> action, string cronExp = null)
        {
            Expression<Action<T>> expression = (x) => action(x);
            RecurringJob.AddOrUpdate(expression, cronExp ?? Cron.Daily());
        }

        public static void Register<T>(Func<T, Task> method, string cronExp = null)
        {
            Expression<Action<T>> expression = (x) => method(x);
            RecurringJob.AddOrUpdate(expression, cronExp ?? Cron.Daily());
        }

    }
}
