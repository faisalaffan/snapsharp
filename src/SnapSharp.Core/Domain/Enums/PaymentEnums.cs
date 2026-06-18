using System.Text.Json.Serialization;

namespace SnapSharp.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType { Savings, Current, Loan, TimeDeposit, Investment, Other }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionStatus { Pending, Success, Failed, Reversed, Refunded }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransferStatus { Pending, Success, Failed, Reversed }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VaStatus { Active, Inactive, Expired, Paid, Cancelled }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethod { VirtualAccount, Qris, DirectDebit, Transfer }
