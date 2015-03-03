using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BuildMon
{
    public class PeriodicTask
    {
    //    public static async Task Run(Action action, TimeSpan period, CancellationToken cancellationToken)
    //    {
    //        while (!cancellationToken.IsCancellationRequested)
    //        {
    //            await Task.Delay(period, cancellationToken);
    //            action();
    //        }
    //    }

    //    public static async Task RunAsync(Func<Task> action, TimeSpan period, CancellationToken cancellationToken)
    //    {
    //        while (!cancellationToken.IsCancellationRequested)
    //        {
    //            await Task.Delay(period, cancellationToken);
    //            await action();
    //        }
    //    }

    //    public static Task Run(Action action, TimeSpan period)
    //    {
    //        return Run(action, period, CancellationToken.None);
    //    }


        
        public static async Task RunSync(Action action, TimeSpan period)
        {
            while (true)
            {
                await Task.Delay(period);
                action();
            }
        }

        public static async Task RunAsync(Func<Task> action, TimeSpan period)
        {
            while (true)
            {
                await Task.Delay(period);
                await action();
            }
        }

        public static Task Run(Action action, TimeSpan period)
        {
            return RunSync(action, period);
        }
    }
}
