﻿namespace Rentals.Domain.Abstractions;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;
}

public abstract class Entity : Entity<long>
{
}