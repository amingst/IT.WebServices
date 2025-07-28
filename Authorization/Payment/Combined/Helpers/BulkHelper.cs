using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Helpers.BulkJobs;
using IT.WebServices.Fragments.Authorization.Payment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Helpers
{
    public class BulkHelper
    {
        private readonly ILogger log;
        private readonly ServiceProvider serviceProvider;

        private readonly ConcurrentDictionary<PaymentBulkAction, IBulkJob> runningJobs = new();

        public BulkHelper(ILogger<BulkHelper> log, ServiceProvider serviceProvider)
        {
            this.log = log;
            this.serviceProvider = serviceProvider;
        }

        public List<PaymentBulkActionProgress> CancelAction(PaymentBulkAction action, ONUser user)
        {
            try
            {
                if (runningJobs.Remove(action, out var job))
                {
                    job.Cancel(user);
                }
            }
            catch { }

            return GetRunningActions();
        }

        public List<PaymentBulkActionProgress> GetRunningActions()
        {
            CheckAll();

            return runningJobs.Values.Select(j => j.Progress).ToList();
        }

        public List<PaymentBulkActionProgress> StartAction(PaymentBulkAction action, ONUser user)
        {
            var newJob = GetNewJob(action);
            if (newJob == null)
                return GetRunningActions();

            if (runningJobs.TryAdd(action, newJob))
            {
                newJob.Start(user);
            }

            return GetRunningActions();
        }

        private void CheckAll()
        {
            foreach (var kv in runningJobs)
            {
                if (kv.Value.Progress.IsCompletedOrCanceled)
                    runningJobs.TryRemove(kv);
            }
        }

        private IBulkJob? GetNewJob(PaymentBulkAction action)
        {
            switch (action)
            {
                case PaymentBulkAction.LookForNewPayments:
                    return serviceProvider.GetService<LookForNewPayments>();
                case PaymentBulkAction.ReconcileAll:
                    return serviceProvider.GetService<ReconcileAll>();
            }

            return null;
        }
    }
}
