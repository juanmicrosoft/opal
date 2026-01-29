namespace ClassWithConstructor
{
    public class BankAccount
    {
        public string Owner;
        public decimal Balance;

        public BankAccount(string owner, decimal initialBalance)
        {
            Owner = owner;
            Balance = initialBalance;
        }

        public void Deposit(decimal amount)
        {
            Balance = Balance + amount;
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= Balance)
            {
                Balance = Balance - amount;
                return true;
            }
            return false;
        }
    }
}
