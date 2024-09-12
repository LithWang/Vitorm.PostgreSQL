﻿using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class Query_Group_Test
    {

        [TestMethod]
        public void Test_Group_Demo()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // Linq Expression
            {
                var query =
                        from user in userQuery
                        group user by new { user.fatherId, user.motherId } into userGroup
                        select new { userGroup.Key.fatherId, userGroup.Key.motherId };

                var rows = query.ToList();

                Assert.AreEqual(3, rows.Count);
                Assert.AreEqual(0, rows.Select(u => u.fatherId).Except(new int?[] { 4, 5, null }).Count());
                Assert.AreEqual(0, rows.Select(u => u.motherId).Except(new int?[] { 6, null }).Count());
            }

            // Lambda Expression
            {
                var query =
                        userQuery
                        .GroupBy(user => new { user.fatherId, user.motherId })
                        .Select(userGroup => new
                        {
                            userGroup.Key.fatherId,
                            userGroup.Key.motherId
                        })
                        ;

                var rows = query.ToList();

                Assert.AreEqual(3, rows.Count);
                Assert.AreEqual(0, rows.Select(u => u.fatherId).Except(new int?[] { 4, 5, null }).Count());
                Assert.AreEqual(0, rows.Select(u => u.motherId).Except(new int?[] { 6, null }).Count());
            }
        }


        [TestMethod]
        public void Test_Group_Complex()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // Linq Expression
            {
                var query =
                        from user in userQuery.Where(u => u.id > 1)
                        group user by new { user.fatherId, user.motherId } into userGroup
                        where userGroup.Key.motherId != null && userGroup.Count() >= 1
                        orderby userGroup.Key.fatherId descending, userGroup.Count() descending
                        select new { userGroup.Key.fatherId, userGroup.Key.motherId, rowCount = userGroup.Count(), maxId = userGroup.Max(m => m.id) };

                query = query.Skip(1).Take(1);

                var rows = query.ToList();

                Assert.AreEqual(1, rows.Count);
                Assert.AreEqual(4, rows[0].fatherId);
                Assert.AreEqual(6, rows[0].motherId);
                Assert.AreEqual(1, rows[0].rowCount);
                Assert.AreEqual(2, rows[0].maxId);
            }

            // Lambda Expression
            {
                var query =
                        userQuery
                        .Where(u => u.id > 1)
                        .GroupBy(user => new { user.fatherId, user.motherId })
                        .Where(userGroup => userGroup.Key.motherId != null)
                        .OrderByDescending(userGroup => userGroup.Key.fatherId)
                        .Select(userGroup => new
                        {
                            userGroup.Key.fatherId,
                            userGroup.Key.motherId,
                            rowCount = userGroup.Count(),
                            maxId = userGroup.Max(m => m.id)
                        })
                        .Skip(1)
                        .Take(1)
                        ;

                var rows = query.ToList();

                Assert.AreEqual(1, rows.Count);
                Assert.AreEqual(4, rows[0].fatherId);
                Assert.AreEqual(6, rows[0].motherId);
                Assert.AreEqual(1, rows[0].rowCount);
                Assert.AreEqual(2, rows[0].maxId);
            }
        }




        [TestMethod]
        public void Test_Others()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var query =
                    userQuery
                    .Where(user => user.id < 7)
                    .GroupBy(user => new { user.fatherId, user.motherId })
                    .OrderByDescending(group => group.Count())
                    .Select(userGroup => new
                    {
                        userGroup.Key.fatherId,
                        rowCount = userGroup.Count(),
                        maxId = userGroup.Max(m => m.id),
                        minId = userGroup.Min(m => m.id),
                        sumId = userGroup.Sum(m => m.id),
                        avgId = userGroup.Average(m => (double)m.id)
                    })
                    ;

                var rows = query.ToList();

                Assert.AreEqual(3, rows.Count);
                var row = rows[1];
                Assert.AreEqual(2, row.rowCount);
                Assert.AreEqual(2, row.maxId);
                Assert.AreEqual(1, row.minId);
                Assert.AreEqual(3, row.sumId);
                Assert.AreEqual(1.5, row.avgId);
            }
            {
                var query =
                    userQuery
                    .GroupBy(user => new { user.fatherId, user.motherId })
                    .Where(userGroup => userGroup.Key.motherId != null)
                    .OrderByDescending(userGroup => userGroup.Key.fatherId)
                    .Select(userGroup => new { userGroup.Key.fatherId, userGroup.Key.motherId })
                    ;

                var rows = query.ToList();

                Assert.AreEqual(2, rows.Count);
                Assert.AreEqual(5, rows[0].fatherId);
            }
            {
                var query =
                    userQuery
                    .GroupBy(user => user.fatherId)
                    .Where(userGroup => userGroup.Key != null)
                    .OrderByDescending(userGroup => userGroup.Key)
                    .Select(userGroup => new { fatherId = userGroup.Key, rowCount = userGroup.Count() })
                    ;

                var rows = query.ToList();

                Assert.AreEqual(2, rows.Count);
                Assert.AreEqual(5, rows[0].fatherId);
            }
        }


    }
}
