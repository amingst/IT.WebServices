using FortisAPI.Standard.Models;
using Google.Protobuf.WellKnownTypes;
using IT.WebServices.Fragments.Authorization.Payment;

namespace IT.WebServices.Authorization.Payment.Fortis.Helpers
{
    internal static class ITSubscriptionHelper
    {
        public static GenericSubscriptionRecord ToSubscriptionRecord(this Data9 fRec)
        {
            return new()
            {
                ProcessorName = PaymentConstants.PROCESSOR_NAME_FORTIS,
                ProcessorSubscriptionID = fRec.Id,
                CreatedOnUTC = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(fRec.CreatedTs).UtcDateTime),
                Status = ConvertStatus(fRec.Status),
            };
        }

        public static GenericSubscriptionRecord ToSubscriptionRecord(this List6 fRec)
        {
            return new()
            {
                ProcessorName = PaymentConstants.PROCESSOR_NAME_FORTIS,
                ProcessorSubscriptionID = fRec.Id,
                CreatedOnUTC = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(fRec.CreatedTs).UtcDateTime),
                Status = ConvertStatus(fRec.Status),
            };
        }

        public static GenericSubscriptionRecord? ToSubscriptionRecord(this ResponseRecurring fRec) => fRec?.Data?.ToSubscriptionRecord();
        public static List<GenericSubscriptionRecord> ToSubscriptionRecords(this ResponseRecurringsCollection fRec)
        {
            return fRec?.List
                        .Select(r => r?.ToSubscriptionRecord())
                        .Where(r => r is not null)
                        .Select(r => r!)
                        .ToList() ?? new List<GenericSubscriptionRecord>();
        }

        private static SubscriptionStatus ConvertStatus(StatusEnum? status)
        {
            switch (status)
            {
                case StatusEnum.Active:
                    return SubscriptionStatus.SubscriptionActive;
                case StatusEnum.EnumOnHold:
                    return SubscriptionStatus.SubscriptionPaused;
                case StatusEnum.Ended:
                    return SubscriptionStatus.SubscriptionStopped;
            }

            return SubscriptionStatus.SubscriptionUnknown;
        }
    }
}
