﻿using System.Runtime.Serialization;

namespace Idu.Orleans.Client.Contracts;

[DataContract]
public record CreateAccount
{
    [DataMember]
    public decimal OpeningBalance { get; init; }
}
