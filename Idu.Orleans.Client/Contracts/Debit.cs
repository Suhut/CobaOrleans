using System.Runtime.Serialization;

namespace Idu.Orleans.Client.Contracts;

[DataContract]
public record Debit
{
    [DataMember]
    public decimal Amount { get; init; }
}
