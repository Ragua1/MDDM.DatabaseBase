using MDDM.DatabaseBase.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MDDM.DatabaseBase.Test
{
    [TestClass]
    public class DbBaseTest
    {
        internal string _conn = "Data Source=localhost;Initial Catalog=TestDb;Integrated Security=True;Encrypt=false";
        private static readonly Random _rand = new Random(Guid.NewGuid().GetHashCode());

        [TestMethod]
        public void TestOpenConnection()
        {
            var db = new ExampleDb(_conn);

            db.OpenConnection();
            db.CloseConnection();
        }

        [TestMethod]
        public void TestInsertData()
        {
            var db = new ExampleDb(_conn);

            var data = new Table_1
            {
                ColText = nameof(TestInsertData),
                ColInt = DateTime.Now.Millisecond,
                ColDate = DateTime.Now,
            };

            var res = db.InsertData(data);
            Assert.AreNotEqual(-1, res);
        }

        [TestMethod]
        public void TestSelectData()
        {
            var db = new ExampleDb(_conn);

            var data = new Table_1
            {
                ColText = nameof(TestSelectData),
                ColInt = DateTime.Now.Millisecond,
                ColDate = DateTime.Now,
            };

            data.Id = db.InsertData(data);
            Assert.IsTrue(data.Id > 0);

            var res = db.SelectData(data.Id);
            Assert.IsNotNull(res);

            Assert.AreEqual(data.Id, res.Id);
            Assert.AreEqual(data.ColText, res.ColText);
            Assert.AreEqual(data.ColInt, res.ColInt);
            Assert.IsTrue((data.ColDate - res.ColDate).Value.Seconds < 1);
        }

        [TestMethod]
        public void TestDeleteData()
        {
            var db = new ExampleDb(_conn);

            var data = new Table_1
            {
                ColText = nameof(TestDeleteData),
                ColInt = DateTime.Now.Millisecond,
                ColDate = DateTime.Now,
            };

            data.Id = db.InsertData(data);
            Assert.IsTrue(data.Id > 0);

            var res = db.SelectData(data.Id);
            Assert.IsNotNull(res);

            Assert.AreEqual(data.Id, res.Id);
            Assert.AreEqual(data.ColText, res.ColText);
            Assert.AreEqual(data.ColInt, res.ColInt);
            Assert.IsTrue((data.ColDate - res.ColDate).Value.Seconds < 1);

            db.Dispose();

            var isDeleted = db.DeleteData(data.Id);
            Assert.IsTrue(isDeleted);

            res = db.SelectData(data.Id);
            Assert.IsNull(res);
        }

        [TestMethod]
        public async Task TestInsertDataAsync()
        {
            var db = new ExampleDb(_conn);

            var data = new Table_1
            {
                ColText = nameof(TestInsertDataAsync),
                ColInt = DateTime.Now.Millisecond,
                ColDate = DateTime.Now,
            };

            var res = await db.InsertDataAsync(data);
            Assert.AreNotEqual(-1, res);
        }

        [TestMethod]
        public async Task TestSelectDataAsync()
        {
            var db = new ExampleDb(_conn);

            var data = new Table_1
            {
                ColText = nameof(TestSelectDataAsync),
                ColInt = DateTime.Now.Millisecond,
                ColDate = DateTime.Now,
            };

            data.Id = await db.InsertDataAsync(data);
            Assert.IsTrue(data.Id > 0);

            var res = await db.SelectDataAsync(data.Id);
            Assert.IsNotNull(res);

            Assert.AreEqual(data.Id, res.Id);
            Assert.AreEqual(data.ColText, res.ColText);
            Assert.AreEqual(data.ColInt, res.ColInt);
            Assert.IsTrue((data.ColDate - res.ColDate).Value.Seconds < 1);
        }

        [TestMethod]
        public async Task TestDeleteDataAsync()
        {
            var db = new ExampleDb(_conn);

            var data = new Table_1
            {
                ColText = nameof(TestDeleteDataAsync),
                ColInt = DateTime.Now.Millisecond,
                ColDate = DateTime.Now,
            };

            data.Id = await db.InsertDataAsync(data);
            Assert.IsTrue(data.Id > 0);

            var res = await db.SelectDataAsync(data.Id);
            Assert.IsNotNull(res);

            Assert.AreEqual(data.Id, res.Id);
            Assert.AreEqual(data.ColText, res.ColText);
            Assert.AreEqual(data.ColInt, res.ColInt);
            Assert.IsTrue((data.ColDate - res.ColDate).Value.Seconds < 1);

            //db.Dispose();

            var isDeleted = await db.DeleteDataAsync(data.Id);
            Assert.IsTrue(isDeleted);

            res = await db.SelectDataAsync(data.Id);
            Assert.IsNull(res);
        }
    }
}
