using IT.WebServices.Fragments.Authorization.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Generic
{
    public class GenericPaymentProcessorProvider
    {
        private readonly List<IGenericPaymentProcessor> genericProcessorProviders;

        public GenericPaymentProcessorProvider(IEnumerable<IGenericPaymentProcessor> genericProcessorProviders)
        {
            this.genericProcessorProviders = genericProcessorProviders.ToList();
        }

        public IGenericPaymentProcessor[] AllProviders => genericProcessorProviders.ToArray();

        public IGenericPaymentProcessor[] AllEnabledProviders => genericProcessorProviders.Where(p => p.IsEnabled).ToArray();

        public IGenericPaymentProcessor GetProcessor(GenericSubscriptionFullRecord record) => GetProcessor(record.SubscriptionRecord);

        public IGenericPaymentProcessor GetProcessor(GenericSubscriptionRecord record)
        {
            var provider = genericProcessorProviders.FirstOrDefault(p => p.ProcessorName == record.ProcessorName);
            if (provider == null)
                throw new NotImplementedException($"GenericPaymentProvider {record.ProcessorName} not found");

            return provider;
        }
    }
}
