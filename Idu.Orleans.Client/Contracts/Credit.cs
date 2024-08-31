using System.Runtime.Serialization;

namespace Idu.Orleans.Client.Contracts;

[DataContract]
public class Credit
{
    [DataMember]
    public decimal Amount { get; init; }
}
