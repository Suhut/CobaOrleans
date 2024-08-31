using System.Runtime.Serialization;

namespace Idu.Orleans.Client.Contracts;

[DataContract]
public class CreateAccount
{
    [DataMember]
    public decimal OpeningBalance { get; init; }
}
