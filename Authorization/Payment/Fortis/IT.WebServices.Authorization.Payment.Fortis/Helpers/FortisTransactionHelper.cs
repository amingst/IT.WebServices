﻿using FortisAPI.Standard.Controllers;
using FortisAPI.Standard.Exceptions;
using FortisAPI.Standard.Models;
using IT.WebServices.Authorization.Payment.Fortis.Clients;
using IT.WebServices.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Fortis.Helpers
{
    public class FortisTransactionHelper
    {
        private readonly FortisClient client;
        private readonly SettingsHelper settingsHelper;

        public FortisTransactionHelper(FortisClient client, SettingsHelper settingsHelper)
        {
            this.client = client;
            this.settingsHelper = settingsHelper;
        }

        public async Task<ResponseTransaction?> CreateFromAccountValut(string accountVaultId, int fixAmount)
        {
            try
            {
                return await client.Client.TransactionsCreditCardController.CCSaleTokenizedAsync(new V1TransactionsCcSaleTokenRequest()
                {
                    AccountVaultId = accountVaultId,
                    TransactionAmount = fixAmount,
                    Description = "Amount Fix",
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }

        public async Task<List<List11>> GetAllPerWeek(DateTimeOffset lower, DateTimeOffset upper, int? amount = null, string? contactId = null, int triesLeft = 5, string? state = null)
        {
            List<List11> ret = new List<List11>();

            for (var d = lower; d < upper; d = d.AddDays(7))
            {
                var d2 = d.AddDays(7);

                if (d2 > upper)
                    d2 = upper;

                Console.Write(d.ToString() + "-" + d2.ToString() + ": ");

                ret.AddRange(await GetAll(d, d2, amount, contactId, triesLeft, state));
            }

            return ret;
        }

        public async Task<List<List11>> GetAllPerDay(DateTimeOffset lower, DateTimeOffset upper, int? amount = null, string? contactId = null, int triesLeft = 5, string? state = null)
        {
            List<List11> ret = new List<List11>();

            for (var d = lower; d < upper; d = d.AddDays(1))
            {
                var d2 = d.AddDays(1);

                if (d2 > upper)
                    d2 = upper;

                Console.Write(d.ToString() + "-" + d2.ToString() + ": ");

                ret.AddRange(await GetAll(d, d2, amount, contactId, triesLeft, state));
            }

            return ret;
        }

        public async Task<List<List11>> GetAllPerHour(DateTimeOffset lower, DateTimeOffset upper, int? amount = null, string? contactId = null, int triesLeft = 5, string? state = null)
        {
            List<List11> ret = new List<List11>();

            for (var d = lower; d < upper; d = d.AddHours(1))
            {
                var d2 = d.AddHours(1);

                if (d2 > upper)
                    d2 = upper;

                Console.Write(d.ToString() + "-" + d2.ToString() + ": ");

                ret.AddRange(await GetAll(d, d2, amount, contactId, triesLeft, state));
            }

            return ret;
        }

        public async Task<List<List11>> GetAll(DateTimeOffset lower, DateTimeOffset upper, int? amount = null, string? contactId = null, int triesLeft = 5, string? state = null)
        {
            int errors = 0;
            int page = 1;
            int size = 100;

            var ret = new List<List11>();

            try
            {
                while (errors < 10)
                {
                    var list = await client.Client.TransactionsReadController.ListTransactionsAsync(
                                new Page() { Number = page, Size = size },
                                null,
                                new Filter11()
                                {
                                    CreatedTs = new()
                                    {
                                        Lower = lower.ToUnixTimeSeconds(),
                                        Upper = upper.ToUnixTimeSeconds(),
                                    },
                                    TransactionAmount = amount,
                                    ContactId = contactId,
                                    LocationId = settingsHelper.Owner.Subscription.Fortis.LocationID,
                                    ProductTransactionId = settingsHelper.Owner.Subscription.Fortis.ProductID,
                                    BillingAddress = state == null ? null : new() { State = state },
                                });

                    if (list?.List == null)
                    {
                        errors++;
                        continue;
                    }

                    if (ret.Any(i => i.Id == list.List.FirstOrDefault()?.Id))
                        break;

                    ret.AddRange(list.List);

                    page++;

                    Console.WriteLine($"Loading Transactions: {ret.Count}");

                    if (list.List.Count < size)
                        break;

                    if (ret.Count > 10000)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);

                if (triesLeft > 0)
                    return await GetAll(lower, upper, amount, contactId, triesLeft - 1, state);
                else
                    throw;
            }

            return ret;
        }

        public async Task<ResponseTransaction?> Get(string tranId)
        {
            try
            {
                return await client.Client.TransactionsReadController.GetTransactionAsync(tranId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }

        public async Task<ResponseTransactionsCollection?> GetByApiId(long tranId)
        {
            try
            {
                return await client.Client.TransactionsReadController.ListTransactionsAsync(new Page() { Number = 1, Size = 1 }, null, new Filter11()
                {
                    TransactionApiId = "\"" + tranId.ToString() + "\""
                }, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }

        public async Task<string> GetNewPaymentIntent(uint amount)
        {
            ElementsController elementsController = client.Client.ElementsController;
            var body = new V1ElementsTransactionIntentionRequest()
            {
                Action = ActionEnum.Sale,
                Amount = (int)amount * 100,
                Methods = new List<Method>(),
                LocationId = settingsHelper.Owner.Subscription.Fortis.LocationID
            };
            body.Methods.Add(new Method(TypeEnum.Cc, settingsHelper.Owner.Subscription.Fortis.ProductID));

            try
            {
                ResponseTransactionIntention result = await elementsController.TransactionIntentionAsync(body);
                return result.Data.ClientToken;
            }
            catch (ApiException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                return "";
            }
        }

        public async Task<ResponseTransaction?> ProcessOneTimeSale(string ccTokenId, uint cents)
        {
            try
            {
                return await client.Client.TransactionsCreditCardController.CCSaleTokenizedAsync(new V1TransactionsCcSaleTokenRequest()
                {
                    TransactionApiId = ccTokenId,
                    TransactionAmount = (int)cents,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return null;
        }
    }
}
