using UnityEngine;

public class Tutorial2 : MonoBehaviour
{
    void Start()
    {
        Tutorial1 acc1 = new Tutorial1();
        acc1.Owner = "Иван";
        acc1.Balance = 1000;

        acc1.Deposit(500);
        acc1.Withdraw(200);
        print(acc1.GetInfo());

        Tutorial1 acc2 = new Tutorial1();
        acc2.Owner = "Мария";
        acc2.Balance = 2500;

        acc2.Withdraw(300);
        print(acc2.GetInfo());
    }
}