using UnityEngine;

public class Tutorial1 : MonoBehaviour
{
    public string Owner;
    public decimal Balance;

    public void Deposit(decimal amount)
    {
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (Balance >= amount)
            Balance -= amount;
    }

    public string GetInfo()
    {
        return $"{Owner}: {Balance}";
    }
}