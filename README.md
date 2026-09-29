# enterprise-agentic-ai
Enterprise-grade AI architecture

using System.ComponentModel.DataAnnotations;

namespace Payment.Orchestration.Models.Create;

// ======================================================
// SHARED
// ======================================================

public sealed class Subcategory
{
    [StringLength(13)]
    public string? SubcategoryCode { get; set; }

    [StringLength(13)]
    public string? SubcategoryAmount { get; set; }
}

// ======================================================
// FRONTEND - REQUEST BASE CLASSES
// ======================================================

public abstract class CreateTransactionRequestBase
{
    [Required]
    [StringLength(36)]
    public string PaymentTransactionId { get; set; } = default!;

    [Required]
    [StringLength(17)]
    public string TotalDollarAmount { get; set; } = default!;

    [Required]
    [StringLength(9)]
    public string Tin { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string TinType { get; set; } = default!;

    [Required]
    [StringLength(9)]
    public string BankRoutingNumber { get; set; } = default!;

    [Required]
    [StringLength(17)]
    public string BankAccountNumber { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string BankAccountType { get; set; } = default!;

    [Required]
    [StringLength(4)]
    public string OriginatingApplicationCode { get; set; } = default!;

    [Required]
    [StringLength(35)]
    public string NameControl { get; set; } = default!;

    [StringLength(70)]
    public string? TaxpayerFirstLastName { get; set; }

    [StringLength(40)]
    public string? EmailAddress { get; set; }

    [StringLength(40)]
    public string? StreetAddressLine1 { get; set; }

    [StringLength(23)]
    public string? StreetAddressLine2 { get; set; }

    public string? City { get; set; }

    [StringLength(2)]
    public string? State { get; set; }

    [StringLength(9)]
    public string? ZipCode { get; set; }

    [StringLength(2)]
    public string? CountryCode { get; set; }

    [StringLength(1)]
    public string? EmailPreferenceFlag { get; set; }

    [Required]
    [StringLength(19)]
    public string TransactionTimeStamp { get; set; } = default!;
}

public abstract class CreatePaymentRequestBase
{
    [Required]
    [StringLength(36)]
    public string PaymentRequestId { get; set; } = default!;

    [Required]
    [StringLength(5)]
    public string IrsTaxTypeCode { get; set; } = default!;

    [Required]
    [StringLength(6)]
    public string TaxPeriod { get; set; } = default!;

    [Required]
    [StringLength(15)]
    public string TaxPaymentAmount { get; set; } = default!;

    [Required]
    [StringLength(8)]
    public string TaxPaymentDate { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string ExistingPaymentOverrideIndicator { get; set; } = default!;
}

// ======================================================
// FRONTEND - REQUEST CONCRETE CLASSES
// ======================================================

public sealed class BtaCreateTransactionRequest
    : CreateTransactionRequestBase
{
    [Required]
    public int PaymentCount { get; set; }

    [Required]
    public List<BtaCreatePaymentRequest> Payments { get; set; } = [];
}

public sealed class BtaCreatePaymentRequest
    : CreatePaymentRequestBase
{
    public int? SubcategoryCount { get; set; }

    public List<Subcategory>? Subcategories { get; set; }
}

public sealed class OlaCreateTransactionRequest
    : CreateTransactionRequestBase
{
    [Required]
    public int TotalPaymentCount { get; set; }

    [Required]
    public List<OlaCreatePaymentRequest> PaymentRequests { get; set; } = [];
}

public sealed class OlaCreatePaymentRequest
    : CreatePaymentRequestBase
{
}

public sealed class TaxProCreateTransactionRequest
    : CreateTransactionRequestBase
{
    [Required]
    public int TotalPaymentCount { get; set; }

    [Required]
    public List<TaxProCreatePaymentRequest> PaymentRequests { get; set; } = [];
}

public sealed class TaxProCreatePaymentRequest
    : CreatePaymentRequestBase
{
}

public sealed class TaxProBusinessCreateTransactionRequest
    : CreateTransactionRequestBase
{
    [Required]
    public int TotalPaymentCount { get; set; }

    [Required]
    public List<TaxProBusinessCreatePaymentRequest> PaymentRequests { get; set; } = [];
}

public sealed class TaxProBusinessCreatePaymentRequest
    : CreatePaymentRequestBase
{
    public int? SubcategoryCount { get; set; }

    public List<Subcategory>? Subcategories { get; set; }
}

// ======================================================
// FRONTEND - RESPONSE BASE CLASSES
// ======================================================

public abstract class CreateTransactionResponseBase
{
    [Required]
    [StringLength(36)]
    public string PaymentTransactionId { get; set; } = default!;

    [Required]
    [StringLength(4)]
    public string ConfirmationId { get; set; } = default!;

    [Required]
    [StringLength(3)]
    public string StatusCode { get; set; } = default!;

    [Required]
    [StringLength(17)]
    public string TotalDollarAmount { get; set; } = default!;

    [Required]
    [StringLength(9)]
    public string Tin { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string TinType { get; set; } = default!;

    [Required]
    [StringLength(9)]
    public string BankRoutingNumber { get; set; } = default!;

    [Required]
    [StringLength(17)]
    public string BankAccountNumber { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string BankAccountType { get; set; } = default!;

    [Required]
    [StringLength(19)]
    public string TransactionTimeStamp { get; set; } = default!;

    [Required]
    public int CurrentTransactionCount { get; set; }

    [Required]
    public int MaxDailyTransactionCount { get; set; }

    [StringLength(70)]
    public string? EmailAddress { get; set; }

    [StringLength(1)]
    public string? EmailPreferenceFlag { get; set; }

    public List<string>? ResponseCodes { get; set; }
}

public abstract class CreatePaymentResponseBase
{
    [Required]
    [StringLength(36)]
    public string PaymentRequestId { get; set; } = default!;

    [Required]
    [StringLength(8)]
    public string TaxPaymentDate { get; set; } = default!;

    [Required]
    [StringLength(15)]
    public string EftNumber { get; set; } = default!;

    [Required]
    [StringLength(15)]
    public string TaxPaymentAmount { get; set; } = default!;

    [Required]
    [StringLength(6)]
    public string TaxPeriod { get; set; } = default!;

    [Required]
    [StringLength(5)]
    public string IrsTaxTypeCode { get; set; } = default!;

    [Required]
    [StringLength(9)]
    public string PaymentStatus { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string ExistingPaymentOverrideIndicator { get; set; } = default!;

    public List<string>? ReturnResponseCodes { get; set; }

    [StringLength(12)]
    public string? EligibleForEditCancelDate { get; set; }
}

// ======================================================
// FRONTEND - RESPONSE CONCRETE CLASSES
// ======================================================

public sealed class BtaCreateTransactionResponse
    : CreateTransactionResponseBase
{
    [Required]
    public int PaymentCount { get; set; }

    [Required]
    public List<BtaCreatePaymentResponse> Payments { get; set; } = [];
}

public sealed class BtaCreatePaymentResponse
    : CreatePaymentResponseBase
{
    public List<Subcategory>? Subcategories { get; set; }
}

public sealed class OlaCreateTransactionResponse
    : CreateTransactionResponseBase
{
    [Required]
    public int TotalPaymentCount { get; set; }

    [Required]
    public List<OlaCreatePaymentResponse> PaymentResponses { get; set; } = [];
}

public sealed class OlaCreatePaymentResponse
    : CreatePaymentResponseBase
{
}

public sealed class TaxProCreateTransactionResponse
    : CreateTransactionResponseBase
{
    [Required]
    public int TotalPaymentCount { get; set; }

    [Required]
    public List<TaxProCreatePaymentResponse> PaymentResponses { get; set; } = [];
}

public sealed class TaxProCreatePaymentResponse
    : CreatePaymentResponseBase
{
}

public sealed class TaxProBusinessCreateTransactionResponse
    : CreateTransactionResponseBase
{
    [Required]
    public int TotalPaymentCount { get; set; }

    [Required]
    public List<TaxProBusinessCreatePaymentResponse> PaymentResponses { get; set; } = [];
}

public sealed class TaxProBusinessCreatePaymentResponse
    : CreatePaymentResponseBase
{
    public List<Subcategory>? Subcategories { get; set; }
}

// ======================================================
// CONSOLIDATED BACKEND DTOs - REQUEST
// ======================================================

public sealed class CreateTransactionRequestDto
{
    [Required]
    [StringLength(36)]
    public string PaymentTransactionId { get; set; } = default!;

    [Required]
    public int TotalPaymentCount { get; set; }

    [Required]
    [StringLength(17)]
    public string TotalDollarAmount { get; set; } = default!;

    [Required]
    [StringLength(9)]
    public string Tin { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string TinType { get; set; } = default!;

    [Required]
    [StringLength(9)]
    public string BankRoutingNumber { get; set; } = default!;

    [Required]
    [StringLength(17)]
    public string BankAccountNumber { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string BankAccountType { get; set; } = default!;

    [Required]
    [StringLength(4)]
    public string OriginatingApplicationCode { get; set; } = default!;

    [Required]
    [StringLength(35)]
    public string NameControl { get; set; } = default!;

    [StringLength(70)]
    public string? TaxpayerFirstLastName { get; set; }

    [StringLength(40)]
    public string? EmailAddress { get; set; }

    [StringLength(40)]
    public string? StreetAddressLine1 { get; set; }

    [StringLength(23)]
    public string? StreetAddressLine2 { get; set; }

    public string? City { get; set; }

    [StringLength(2)]
    public string? State { get; set; }

    [StringLength(9)]
    public string? ZipCode { get; set; }

    [StringLength(2)]
    public string? CountryCode { get; set; }

    [StringLength(1)]
    public string? EmailPreferenceFlag { get; set; }

    [Required]
    [StringLength(19)]
    public string TransactionTimeStamp { get; set; } = default!;

    [Required]
    public List<CreatePaymentRequestDto> PaymentRequests { get; set; } = [];
}

public sealed class CreatePaymentRequestDto
{
    [Required]
    [StringLength(36)]
    public string PaymentRequestId { get; set; } = default!;

    [Required]
    [StringLength(5)]
    public string IrsTaxTypeCode { get; set; } = default!;

    [Required]
    [StringLength(6)]
    public string TaxPeriod { get; set; } = default!;

    [Required]
    [StringLength(15)]
    public string TaxPaymentAmount { get; set; } = default!;

    [Required]
    [StringLength(8)]
    public string TaxPaymentDate { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string ExistingPaymentOverrideIndicator { get; set; } = default!;

    public int? SubcategoryCount { get; set; }

    public List<Subcategory>? Subcategories { get; set; }
}

// ======================================================
// CONSOLIDATED BACKEND DTOs - RESPONSE
// ======================================================

public sealed class CreateTransactionResponseDto
{
    [Required]
    [StringLength(36)]
    public string PaymentTransactionId { get; set; } = default!;

    [Required]
    [StringLength(4)]
    public string ConfirmationId { get; set; } = default!;

    [Required]
    [StringLength(3)]
    public string StatusCode { get; set; } = default!;

    [Required]
    public int TotalPaymentCount { get; set; }

    [Required]
    [StringLength(17)]
    public string TotalDollarAmount { get; set; } = default!;

    [Required]
    [StringLength(9)]
    public string Tin { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string TinType { get; set; } = default!;

    [Required]
    [StringLength(9)]
    public string BankRoutingNumber { get; set; } = default!;

    [Required]
    [StringLength(17)]
    public string BankAccountNumber { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string BankAccountType { get; set; } = default!;

    [Required]
    [StringLength(19)]
    public string TransactionTimeStamp { get; set; } = default!;

    [Required]
    public int CurrentTransactionCount { get; set; }

    [Required]
    public int MaxDailyTransactionCount { get; set; }

    [StringLength(70)]
    public string? EmailAddress { get; set; }

    [StringLength(1)]
    public string? EmailPreferenceFlag { get; set; }

    public List<string>? ResponseCodes { get; set; }

    [Required]
    public List<CreatePaymentResponseDto> PaymentResponses { get; set; } = [];
}

public sealed class CreatePaymentResponseDto
{
    [Required]
    [StringLength(36)]
    public string PaymentRequestId { get; set; } = default!;

    [Required]
    [StringLength(8)]
    public string TaxPaymentDate { get; set; } = default!;

    [Required]
    [StringLength(15)]
    public string EftNumber { get; set; } = default!;

    [Required]
    [StringLength(15)]
    public string TaxPaymentAmount { get; set; } = default!;

    [Required]
    [StringLength(6)]
    public string TaxPeriod { get; set; } = default!;

    [Required]
    [StringLength(5)]
    public string IrsTaxTypeCode { get; set; } = default!;

    [Required]
    [StringLength(9)]
    public string PaymentStatus { get; set; } = default!;

    [Required]
    [StringLength(1)]
    public string ExistingPaymentOverrideIndicator { get; set; } = default!;

    public List<string>? ReturnResponseCodes { get; set; }

    public List<Subcategory>? Subcategories { get; set; }

    [StringLength(12)]
    public string? EligibleForEditCancelDate { get; set; }
}