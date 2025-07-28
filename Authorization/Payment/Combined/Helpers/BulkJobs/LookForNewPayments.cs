using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Generic.Data;
using IT.WebServices.Authorization.Payment.Generic;
using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authorization.Payment.Helpers.Models;
using System.Diagnostics;
using FortisAPI.Standard.Models;

namespace IT.WebServices.Authorization.Payment.Helpers.BulkJobs
{
    public class LookForNewPayments : IBulkJob
    {
        private readonly ILogger logger;
        private readonly IGenericSubscriptionFullRecordProvider fullProvider;
        private readonly IGenericSubscriptionRecordProvider subProvider;
        private readonly IGenericPaymentRecordProvider paymentProvider;
        private readonly GenericPaymentProcessorProvider genericProcessorProvider;
        private readonly ReconcileHelper reconcileHelper;

        private Task? task;
        private CancellationTokenSource cancelToken = new();
        private ONUser user;

        private const int DAYS_TO_LOOK_BACK = 10;

        public LookForNewPayments(ILogger<LookForNewPayments> logger, IGenericSubscriptionFullRecordProvider fullProvider, IGenericSubscriptionRecordProvider subProvider, IGenericPaymentRecordProvider paymentProvider, GenericPaymentProcessorProvider genericProcessorProvider, ReconcileHelper reconcileHelper)
        {
            this.logger = logger;
            this.fullProvider = fullProvider;
            this.subProvider = subProvider;
            this.paymentProvider = paymentProvider;
            this.genericProcessorProvider = genericProcessorProvider;
            this.reconcileHelper = reconcileHelper;
        }

        public PaymentBulkActionProgress Progress { get; init; } = new() { Action = PaymentBulkAction.ReconcileAll };

        public void Cancel(ONUser user)
        {
            cancelToken.Cancel();

            Progress.CanceledOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            Progress.CanceledBy = user.Id.ToString();
            Progress.Progress = 100;
            Progress.StatusMessage = "Canceled";
        }

        public void Start(ONUser user)
        {
            Progress.CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            Progress.CreatedBy = user.Id.ToString();
            Progress.Progress = 0;
            Progress.StatusMessage = "Starting";

            this.user = user;

            task = LoadAll();
        }

        private async Task LoadAll()
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var range = new DateTimeOffsetRange(now.AddDays(-DAYS_TO_LOOK_BACK), now);

                var processors = genericProcessorProvider.AllEnabledProviders;

                for (int i = 0; i < processors.Length; i++)
                {
                    Progress.Progress = 1F * i / processors.Length;
                    var processor = processors[i];
                    var payments = processor.GetAllPaymentsForDateRange(range);

                    await foreach (var payment in payments)
                        await LoadPayment(payment);
                }

                Progress.StatusMessage = "Completed Successfully";
                Progress.CompletedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
                Progress.Progress = 1;
            }
            catch (Exception ex)
            {
                Progress.StatusMessage = ex.Message;
                Progress.CompletedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            }
        }

        private async Task LoadPayment(GenericPaymentRecord payment)
        {
            var localSub = await subProvider.GetByProcessorId(payment.ProcessorPaymentID);
            if (localSub is null)
                return;

            await reconcileHelper.EnsurePayment(localSub, payment, user);
        }
    }
}
