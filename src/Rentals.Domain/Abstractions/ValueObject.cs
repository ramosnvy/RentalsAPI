﻿namespace Rentals.Domain.Abstractions;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
    public override bool Equals(object? obj)
        => obj is ValueObject other &&
           GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    public override int GetHashCode()
        => GetEqualityComponents()
            .Aggregate(1, (acc, obj) => HashCode.Combine(acc, obj));
}
