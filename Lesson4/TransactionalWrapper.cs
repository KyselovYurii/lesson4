using System.Collections.Generic;

namespace lesson4
{
    public class TransactionalWrapper<TK, TV> : Dictionary<TK, TV>
    {
        public Transaction<TK, TV> BeginTransaction()
        {
            return new Transaction<TK, TV>(this);
        }

        public void Commit(Transaction<TK, TV> transaction)
        {
            transaction.Commit();
        }

        public void RollbackTransation(Transaction<TK, TV> transaction)
        {
            transaction.RollBack();
        }
    }
}