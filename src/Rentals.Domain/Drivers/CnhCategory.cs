namespace Rentals.Domain.Drivers;

[Flags]
public enum CnhCategory
{
    None = 0,
    A = 1,
    B = 2,
    AB = A | B
}