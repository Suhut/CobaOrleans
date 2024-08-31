﻿namespace Idu.Orleans.Grains.State;

[GenerateSerializer]
public record CheckingAccountState
{
    [Id(0)]
    public Guid AccountId { get; set; }
    [Id(1)]
    public DateTime OpenedAtUtc { get; set; }
    [Id(2)]
    public string AccountType { get; set; }
}
