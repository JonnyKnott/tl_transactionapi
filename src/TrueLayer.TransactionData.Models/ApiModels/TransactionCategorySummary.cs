using System.Collections.Generic;

namespace TrueLayer.TransactionData.Models.ApiModels
{
    public class TransactionCategorySummary
    {
        public string CategoryTitle { get; set; }
        public double TotalAmount { get; set; }
        public double AverageAmount { get; set; }
        public Transaction LargestPurchase { get; set; } 
    }

    public class TransactionCategoryResponse
    {
        public ICollection<TransactionCategorySummary> DebitTransactions { get; set; }
        public ICollection<TransactionCategorySummary> CreditTransactions { get; set; }
    }
}