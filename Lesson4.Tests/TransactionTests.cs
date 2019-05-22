using lesson4;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lesson4.Tests
{
    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void AddOnCommit()
        {
            TransactionalWrapper<int, string> wrapper = new TransactionalWrapper<int, string>();

            using (var transaction = wrapper.BeginTransaction())
            {
                transaction.Add(1, "1");
                transaction.Add(2, "2");
                transaction.Add(3, "3");

                Assert.IsTrue(wrapper.Count == 0);
                Assert.IsTrue(transaction.Count == 3);
            }

            Assert.IsTrue(wrapper.Count == 3);
        }

        [TestMethod]
        public void RemoveOnCommit()
        {
            TransactionalWrapper<int, string> wrapper = new TransactionalWrapper<int, string> { { 1, "1" }, { 2, "2" }, { 3, "3" } };

            using (var transaction = wrapper.BeginTransaction())
            {
                transaction.Remove(1);
                transaction.Remove(3);

                Assert.IsTrue(wrapper.Count == 3);
            }

            Assert.IsTrue(wrapper.Count == 1);
        }

        [TestMethod]
        public void IncompatibleAddAndRemove()
        {
            TransactionalWrapper<int, string> wrapper = new TransactionalWrapper<int, string> { { 1, "1" }, { 2, "2" }, { 3, "3" } };

            using (Transaction<int, string> transaction = wrapper.BeginTransaction())
            {
                transaction.Remove(2);
                Assert.IsTrue(transaction.Count == 2);

                transaction.Add(2, "2");
                Assert.IsTrue(transaction.Count == 3);
            }

            Assert.IsTrue(wrapper.Count == 3);
        }

        [TestMethod]
        public void DoNotAddOnRollback()
        {
            TransactionalWrapper<int, string> wrapper = new TransactionalWrapper<int, string>();

            using (var transaction = wrapper.BeginTransaction())
            {
                transaction.Add(1, "1");
                transaction.Add(2, "2");
                transaction.Add(3, "3");

                Assert.IsTrue(transaction.Count == 3);

                wrapper.RollbackTransation(transaction);
            }

            Assert.IsTrue(wrapper.Count == 0);
        }

        [TestMethod]
        public void DoNotRemoveOnRollback()
        {
            TransactionalWrapper<int, string> wrapper = new TransactionalWrapper<int, string> { { 1, "1" }, { 2, "2" }, { 3, "3" } };

            using (var transaction = wrapper.BeginTransaction())
            {
                transaction.Remove(1);
                transaction.Remove(3);

                Assert.IsTrue(transaction.Count == 1);

                wrapper.RollbackTransation(transaction);
            }
            Assert.IsTrue(wrapper.Count == 3);
        }
        
        [TestMethod]
        public void TwoTransactions()
        {
            TransactionalWrapper<int, string> wrapper = new TransactionalWrapper<int, string> { { 1, "1" }, { 2, "2" }, { 3, "3" } };

            using (Transaction<int, string> firstTransaction = wrapper.BeginTransaction())
            {
                using (Transaction<int, string> secondTransaction = wrapper.BeginTransaction())
                {
                    secondTransaction.Remove(2);
                    secondTransaction.Remove(3);
                }

                Assert.IsTrue(wrapper.Count == 1);

                firstTransaction.Add(2, "2");
                firstTransaction.Add(3, "3");
            }

            Assert.IsTrue(wrapper.Count == 3);
        }
    }
}