using System;
using System.Linq;
using TrueLayer.TransactionData.Models.ApiModels;

namespace TrueLayer.TransactionData.Services.Extensions
{
    public static class LinqExtensions
    {
        public static TransactionCategorySummary CreateCategorySummary(this IGrouping<string, Transaction> group)
        {
            return new TransactionCategorySummary
            {
                CategoryTitle = group.Key ?? "Uncategorised",
                TotalAmount = Math.Round(group.Sum(y => y.Amount), 2),
                AverageAmount = Math.Round(group.Average(y => y.Amount), 2),
                LargestPurchase = group.OrderByDescending(transaction => Math.Abs(transaction.Amount)).First()
            };
        }
    }
}