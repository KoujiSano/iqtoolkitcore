// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

namespace Test
{

    public abstract class NorthwindExecutionTests : NorthwindTestBase
    {
        [Fact]
        public void TestCompiledQuery()
        {
            var fn = QueryCompiler.Compile((string id) => db.Customers.Where(c => c.CustomerID == id));
            var items = fn("ALKFI").ToList();
        }

        [Fact]
        public void TestCompiledQuerySingleton()        {
            var fn = QueryCompiler.Compile((string id) => db.Customers.SingleOrDefault(c => c.CustomerID == id));
            Customer? cust = fn("ALKFI");
        }

        [Fact]
        public void TestCompiledQueryCount()
        {
            var fn = QueryCompiler.Compile((string id) => db.Customers.Count(c => c.CustomerID == id));
            int n = fn("ALKFI");
        }

        [Fact]
        public void TestCompiledQueryIsolated()
        {
            var fn = QueryCompiler.Compile((Northwind n, string id) => n.Customers.Where(c => c.CustomerID == id));
            var items = fn(this.db, "ALFKI").ToList();
        }

        [Fact]
        public void TestCompiledQueryIsolatedWithHeirarchy()
        {
            var fn = QueryCompiler.Compile((Northwind n, string id) => n.Customers.Where(c => c.CustomerID == id).Select(c => n.Orders.Where(o => o.CustomerID == c.CustomerID)));
            var items = fn(this.db, "ALFKI").ToList();
        }

        [Fact]
        public void TestWhere()
        {
            var list = db.Customers.Where(c => c.City == "London").ToList();
            Assert.Equal(6, list.Count);
        }

        [Fact]
        public void TestWhereTrue()
        {
            var list = db.Customers.Where(c => true).ToList();
            Assert.Equal(91, list.Count);
        }

        [Fact]
        public void TestCompareEntityEqual()
        {
            Customer alfki = new Customer { CustomerID = "ALFKI" };
            var list = db.Customers.Where(c => c == alfki).ToList();
            Assert.Single(list);
            Assert.Equal("ALFKI", list[0].CustomerID);
        }

        [Fact]
        public void TestCompareEntityNotEqual()
        {
            Customer alfki = new Customer { CustomerID = "ALFKI" };
            var list = db.Customers.Where(c => c != alfki).ToList();
            Assert.Equal(90, list.Count);
        }

        [Fact]
        public void TestCompareConstructedEqual()
        {
            var list = db.Customers.Where(c => new { x = c.City } == new { x = "London" }).ToList();
            Assert.Equal(6, list.Count);
        }

        [Fact]
        public void TestCompareConstructedMultiValueEqual()
        {
            var list = db.Customers.Where(c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" }).ToList();
            Assert.Equal(6, list.Count);
        }

        [Fact]
        public void TestCompareConstructedMultiValueNotEqual()
        {
            var list = db.Customers.Where(c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" }).ToList();
            Assert.Equal(85, list.Count);
        }

        [Fact]
        public void TestSelectScalar()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => c.City).ToList();
            Assert.Equal(6, list.Count);
            Assert.Equal("London", list[0]);
            Assert.True(list.All(x => x == "London"));
        }

        [Fact]
        public void TestSelectAnonymousOne()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City }).ToList();
            Assert.Equal(6, list.Count);
            Assert.Equal("London", list[0].City);
            Assert.True(list.All(x => x.City == "London"));
        }

        [Fact]
        public void TestSelectAnonymousTwo()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City, c.Phone }).ToList();
            Assert.Equal(6, list.Count);
            Assert.Equal("London", list[0].City);
            Assert.True(list.All(x => x.City == "London"));
            Assert.True(list.All(x => x.Phone != null));
        }

        [Fact]
        public void TestSelectCustomerTable()
        {
            var list = db.Customers.ToList();
            Assert.Equal(91, list.Count);
        }

        [Fact]
        public void TestSelectAnonymousWithObject()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { c.City, c }).ToList();
            Assert.Equal(6, list.Count);
            Assert.Equal("London", list[0].City);
            Assert.True(list.All(x => x.City == "London"));
            Assert.True(list.All(x => x.c.City == x.City));
        }

        [Fact]
        public void TestSelectAnonymousLiteral()
        {
            var list = db.Customers.Where(c => c.City == "London").Select(c => new { X = 10 }).ToList();
            Assert.Equal(6, list.Count);
            Assert.True(list.All(x => x.X == 10));
        }

        [Fact]
        public void TestSelectConstantInt()
        {
            var list = db.Customers.Select(c => 10).ToList();
            Assert.Equal(91, list.Count);
            Assert.True(list.All(x => x == 10));
        }

        [Fact]
        public void TestSelectConstantNullString()
        {
            var list = db.Customers.Select(c => (string?)null).ToList();
            Assert.Equal(91, list.Count);
            Assert.True(list.All(x => x == null));
        }

        [Fact]
        public void TestSelectLocal()
        {
            int x = 10;
            var list = db.Customers.Select(c => x).ToList();
            Assert.Equal(91, list.Count);
            Assert.True(list.All(y => y == 10));
        }

        [Fact]
        public void TestSelectNestedCollection()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                select db.Orders.Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID)
                ).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0].Count());
        }

        [Fact]
        public void TestSelectNestedCollectionInAnonymousType()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                select new { Foos = db.Orders.Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID).ToList() }
                ).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0].Foos.Count);
        }

        [Fact]
        public void TestJoinCustomerOrders()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                join o in db.Orders on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID }
                ).ToList();
            Assert.Equal(6, list.Count);
        }

        [Fact]
        public void TestJoinMultiKey()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                join o in db.Orders on new { a = c.CustomerID, b = c.CustomerID } equals new { a = o.CustomerID, b = o.CustomerID }
                select new { c, o }
                ).ToList();
            Assert.Equal(6, list.Count);
        }

        [Fact]
        public void TestJoinIntoCustomersOrdersCount()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "ALFKI"
                join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                select new { cust = c, ords = ords.Count() }
                ).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0].ords);
        }

        [Fact]
        public void TestJoinIntoDefaultIfEmpty()
        {
            var list = (
                from c in db.Customers
                where c.CustomerID == "PARIS"
                join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                from o in ords.DefaultIfEmpty()
                select new { c, o }
                ).ToList();

            Assert.Single(list);
            Assert.Null(list[0].o);
        }

        [Fact]
        public void TestMultipleJoinsWithJoinConditionsInWhere()
        {
            // this should reduce to inner joins
            var list = (
                from c in db.Customers
                from o in db.Orders
                from d in db.OrderDetails
                where o.CustomerID == c.CustomerID && o.OrderID == d.OrderID
                where c.CustomerID == "ALFKI"
                select d
                ).ToList();

            Assert.Equal(12, list.Count);
        }

        [Fact]
        [ExcludeProvider("MySql")]
        public void TestMultipleJoinsWithMissingJoinCondition()
        {
            // this should force a naked cross join
            var list = (
                from c in db.Customers
                from o in db.Orders
                from d in db.OrderDetails
                where o.CustomerID == c.CustomerID /*&& o.OrderID == d.OrderID*/
                where c.CustomerID == "ALFKI"
                select d
                ).ToList();

            Assert.Equal(12930, list.Count);
        }

        [Fact]
        public void TestOrderBy()
        {
            var list = db.Customers.OrderBy(c => c.CustomerID).Select(c => c.CustomerID).ToList();
            var sorted = list.OrderBy(c => c).ToList();
            Assert.Equal(91, list.Count);
            Assert.True(Enumerable.SequenceEqual(list, sorted));
        }

        [Fact]
        public void TestOrderByOrderBy()
        {
            var list = db.Customers.OrderBy(c => c.Phone).OrderBy(c => c.CustomerID).ToList();
            var sorted = list.OrderBy(c => c.CustomerID).ToList();
            Assert.Equal(91, list.Count);
            Assert.True(Enumerable.SequenceEqual(list, sorted));
        }

        [Fact]
        public void TestOrderByThenBy()
        {
            var list = db.Customers.OrderBy(c => c.CustomerID).ThenBy(c => c.Phone).ToList();
            var sorted = list.OrderBy(c => c.CustomerID).ThenBy(c => c.Phone).ToList();
            Assert.Equal(91, list.Count);
            Assert.True(Enumerable.SequenceEqual(list, sorted));
        }

        [Fact]
        public void TestOrderByDescending()
        {
            var list = db.Customers.OrderByDescending(c => c.CustomerID).ToList();
            var sorted = list.OrderByDescending(c => c.CustomerID).ToList();
            Assert.Equal(91, list.Count);
            Assert.True(Enumerable.SequenceEqual(list, sorted));
        }

        [Fact]
        public void TestOrderByDescendingThenBy()
        {
            var list = db.Customers.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).ToList();
            var sorted = list.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).ToList();
            Assert.Equal(91, list.Count);
            Assert.True(Enumerable.SequenceEqual(list, sorted));
        }

        [Fact]
        public void TestOrderByDescendingThenByDescending()
        {
            var list = db.Customers.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).ToList();
            var sorted = list.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).ToList();
            Assert.Equal(91, list.Count);
            Assert.True(Enumerable.SequenceEqual(list, sorted));
        }

        [Fact]
        public void TestOrderByJoin()
        {
            var list = (
                from c in db.Customers.OrderBy(c => c.CustomerID)
                join o in db.Orders.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                select new { c.CustomerID, o.OrderID }
                ).ToList();

            var sorted = list.OrderBy(x => x.CustomerID).ThenBy(x => x.OrderID);
            Assert.True(Enumerable.SequenceEqual(list, sorted));
        }

        [Fact]
        public void TestOrderBySelectMany()
        {
            var list = (
                from c in db.Customers.OrderBy(c => c.CustomerID)
                from o in db.Orders.OrderBy(o => o.OrderID)
                where c.CustomerID == o.CustomerID
                select new { c.CustomerID, o.OrderID }
                ).ToList();
            var sorted = list.OrderBy(x => x.CustomerID).ThenBy(x => x.OrderID).ToList();
            Assert.True(Enumerable.SequenceEqual(list, sorted));
        }

        [Fact]
        public void TestCountProperty()
        {
            var list = db.Customers.Where(c => c.Orders != null && c.Orders.Count > 0).ToList();
            Assert.Equal(89, list.Count);
        }

        [Fact]
        public void TestGroupBy()
        {
            var list = db.Customers.GroupBy(c => c.City).ToList();
            Assert.Equal(69, list.Count);
        }

        [Fact]
        public void TestGroupByOne()
        {
            var list = db.Customers.Where(c => c.City == "London").GroupBy(c => c.City).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0].Count());
        }

        [Fact]
        public void TestGroupBySelectMany()
        {
            var list = db.Customers.GroupBy(c => c.City).SelectMany(g => g).ToList();
            Assert.Equal(91, list.Count);
        }

        [Fact]
        public void TestGroupBySum()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1))).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0]);
        }

        [Fact]
        public void TestGroupByCount()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.Count()).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0]);
        }

        [Fact]
        public void TestGroupByLongCount()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g => g.LongCount()).ToList();
            Assert.Single(list);
            Assert.Equal(6L, list[0]);
        }

        [Fact]
        public void TestGroupBySumMinMaxAvg()
        {
            var list = 
                db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Select(g =>
                    new
                    {
                        Sum = g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1)),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    }).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0].Sum);
        }

        [Fact]
        public void TestGroupByWithResultSelector()
        {
            var list = 
                db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, (k, g) =>
                    new
                    {
                        Sum = g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1)),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    }).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0].Sum);           
        }

        [Fact]
        public void TestGroupByWithElementSelectorSum()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).Select(g => g.Sum()).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0]);
        }

        [Fact]
        public void TestGroupByWithElementSelector()
        {
            // note: groups are retrieved through a separately execute subquery per row
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0].Count());
            Assert.Equal(6, list[0].Sum());
        }

        [Fact]
        public void TestGroupByWithElementSelectorSumMax()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => (o.CustomerID == "ALFKI" ? 1 : 1)).Select(g => new { Sum = g.Sum(), Max = g.Max() }).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0].Sum);
            Assert.Equal(1, list[0].Max);
        }

        [Fact]
        public void TestGroupByWithAnonymousElement()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID, o => new { X = (o.CustomerID == "ALFKI" ? 1 : 1) }).Select(g => g.Sum(x => x.X)).ToList();
            Assert.Single(list);
            Assert.Equal(6, list[0]);
        }

        [Fact]
        public void TestGroupByWithTwoPartKey()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => new { o.CustomerID, o.OrderDate }).Select(g => g.Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1))).ToList();
            Assert.Equal(6, list.Count);
        }

        [Fact]
        public void TestGroupByWithCountInWhere()
        {
            var list = db.Customers.Where(a => a.Orders != null && a.Orders.Count() > 15).GroupBy(a => a.City).ToList();
            Assert.Equal(9, list.Count);
        }

        [Fact]
        public void TestOrderByGroupBy()
        {
            // note: order-by is lost when group-by is applied (the sequence of groups is not ordered)
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).ToList();
            Assert.Single(list);
            var grp = list[0].ToList();
            var sorted = grp.OrderBy(o => o.OrderID);
            Assert.True(Enumerable.SequenceEqual(grp, sorted));
        }

        [Fact]
        public void TestOrderByGroupBySelectMany()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).SelectMany(g => g).ToList();
            Assert.Equal(6, list.Count);
            var sorted = list.OrderBy(o => o.OrderID).ToList();
            Assert.True(Enumerable.SequenceEqual(list, sorted));
        }

        [Fact]
        public void TestSumWithNoArg()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Select(o => (o.CustomerID == "ALFKI" ? 1 : 1)).Sum();
            Assert.Equal(6, sum);
        }

        [Fact]
        public void TestSumWithArg()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1));
            Assert.Equal(6, sum);
        }

        [Fact]
        public void TestCountWithNoPredicate()
        {
            var cnt = db.Orders.Count();
            Assert.Equal(830, cnt);
        }

        [Fact]
        public void TestCountWithPredicate()
        {
            var cnt = db.Orders.Count(o => o.CustomerID == "ALFKI");
            Assert.Equal(6, cnt);
        }

        [Fact]
        public void TestDistinctNoDupes()
        {
            var list = db.Customers.Distinct().ToList();
            Assert.Equal(91, list.Count);
        }

        [Fact]
        public void TestDistinctScalar()
        {
            var list = db.Customers.Select(c => c.City).Distinct().ToList();
            Assert.Equal(69, list.Count);
        }

        [Fact]
        public void TestDistinctWithCase()
        {
            var query = db.Customers.Select(c => c.CustomerID == "ALFKI" ? 1 : 0).Distinct();
            var list = query.ToList();
            Assert.Equal(2, list.Count);
        }

#if false
        public void TestOrderByDistinct()
        {
            // ordering doesn't make sense here: Distinct operator is not guaranteed to retain ordering.
            var list = db.Customers.Where(c => c.City.StartsWith("P")).OrderBy(c => c.City).Select(c => c.City).Distinct().ToList();
            var sorted = list.OrderBy(x => x).ToList();
            Assert.Equal(list[0], sorted[0]);
            Assert.Equal(list[list.Count - 1], sorted[list.Count - 1]);
        }
#endif

        [Fact]
        public void TestDistinctOrderBy()
        {
            var list = db.Customers.Where(c => c.City.StartsWith("P")).Select(c => c.City).Distinct().OrderBy(c => c).ToList();
            var sorted = list.OrderBy(x => x).ToList();
            Assert.Equal(list[0], sorted[0]);
            Assert.Equal(list[list.Count - 1], sorted[list.Count - 1]);
        }

        [Fact]
        public void TestDistinctGroupBy()
        {
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").Distinct().GroupBy(o => o.CustomerID).ToList();
            Assert.Single(list);
        }

        [Fact]
        public void TestGroupByDistinct()
        {
            // distinct after group-by should not do anything
            var list = db.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Distinct().ToList();
            Assert.Single(list);
        }

        [Fact]
        public void TestDistinctCount()
        {
            var cnt = db.Customers.Distinct().Count();
            Assert.Equal(91, cnt);
        }

        [Fact]
        public void TestSelectDistinctCount()
        {
            // cannot do: SELECT COUNT(DISTINCT some-colum) FROM some-table
            // because COUNT(DISTINCT some-column) does not count nulls
            var cnt = db.Customers.Select(c => c.City).Distinct().Count();
            Assert.Equal(69, cnt);
        }

        [Fact]
        public void TestSelectSelectDistinctCount()
        {
            var cnt = db.Customers.Select(c => c.City).Select(c => c).Distinct().Count();
            Assert.Equal(69, cnt);
        }

        [Fact]
        public void TestDistinctCountPredicate()
        {
            var cnt = db.Customers.Select(c => new {c.City, c.Country}).Distinct().Count(c => c.City == "London");
            Assert.Equal(1, cnt);
        }

        [Fact]
        public void TestDistinctSumWithArg()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Distinct().Sum(o => (o.CustomerID == "ALFKI" ? 1 : 1));
            Assert.Equal(6, sum);
        }

        [Fact]
        public void TestSelectDistinctSum()
        {
            var sum = db.Orders.Where(o => o.CustomerID == "ALFKI").Select(o => o.OrderID).Distinct().Sum();
            Assert.Equal(64835, sum);
        }

        [Fact]
        public void TestTake()
        {
            var list = db.Orders.Take(5).ToList();
            Assert.Equal(5, list.Count);
        }

        [Fact]
        public void TestTakeDistinct()
        {
            // distinct must be forced to apply after top has been computed
            var list = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Distinct().ToList();
            Assert.Single(list);
        }

        [Fact]
        public void TestDistinctTake()
        {
            // top must be forced to apply after distinct has been computed
            var list = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Distinct().Take(5).ToList();
            Assert.Equal(5, list.Count);
        }

        [Fact]
        [ExcludeProvider("Access")]  // ??? this produces a count of 6 ???
        public void TestDistinctTakeCount()
        {
            var cnt = db.Orders.Distinct().OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Count();
            Assert.Equal(5, cnt);
        }

        [Fact]
        public void TestTakeDistinctCount()
        {
            var cnt = db.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).Take(5).Distinct().Count();
            Assert.Equal(1, cnt);
        }

        [Fact]
        public void TestFirst()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).First();
            Assert.NotNull(first);
            Assert.Equal("ROMEY", first.CustomerID);
        }

        [Fact]
        public void TestFirstPredicate()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).First(c => c.City == "London");
            Assert.NotNull(first);
            Assert.Equal("EASTC", first.CustomerID);
        }

        [Fact]
        public void TestWhereFirst()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").First();
            Assert.NotNull(first);
            Assert.Equal("EASTC", first.CustomerID);
        }

        [Fact]
        public void TestFirstOrDefault()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault();
            Assert.NotNull(first);
            Assert.Equal("ROMEY", first.CustomerID);
        }

        [Fact]
        public void TestFirstOrDefaultPredicate()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "London");
            Assert.NotNull(first);
            Assert.Equal("EASTC", first?.CustomerID);
        }

        [Fact]
        public void TestWhereFirstOrDefault()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").FirstOrDefault();
            Assert.NotNull(first);
            Assert.Equal("EASTC", first?.CustomerID);
        }

        [Fact]
        public void TestFirstOrDefaultPredicateNoMatch()
        {
            var first = db.Customers.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "SpongeBob");
            Assert.Null(first);
        }

        [Fact]
        public void TestReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().ToList();
            Assert.Equal(91, list.Count);
            Assert.Equal("WOLZA", list[0].CustomerID);
            Assert.Equal("ROMEY", list[90].CustomerID);
        }

        [Fact]
        public void TestReverseReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Reverse().ToList();
            Assert.Equal(91, list.Count);
            Assert.Equal("ROMEY", list[0].CustomerID);
            Assert.Equal("WOLZA", list[90].CustomerID);
        }

        [Fact]
        public void TestReverseWhereReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Where(c => c.City == "London").Reverse().ToList();
            Assert.Equal(6, list.Count);
            Assert.Equal("EASTC", list[0].CustomerID);
            Assert.Equal("BSBEV", list[5].CustomerID);
        }

        [Fact]
        public void TestReverseTakeReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Take(5).Reverse().ToList();
            Assert.Equal(5, list.Count);
            Assert.Equal("CHOPS", list[0].CustomerID);
            Assert.Equal("WOLZA", list[4].CustomerID);
        }

        [Fact]
        public void TestReverseWhereTakeReverse()
        {
            var list = db.Customers.OrderBy(c => c.ContactName).Reverse().Where(c => c.City == "London").Take(5).Reverse().ToList();
            Assert.Equal(5, list.Count);
            Assert.Equal("CONSH", list[0].CustomerID);
            Assert.Equal("BSBEV", list[4].CustomerID);
        }

        [Fact]
        public void TestLast()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Last();
            Assert.NotNull(last);
            Assert.Equal("WOLZA", last.CustomerID);
        }

        [Fact]
        public void TestLastPredicate()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Last(c => c.City == "London");
            Assert.NotNull(last);
            Assert.Equal("BSBEV", last.CustomerID);
        }

        [Fact]
        public void TestWhereLast()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").Last();
            Assert.NotNull(last);
            Assert.Equal("BSBEV", last.CustomerID);
        }

        [Fact]
        public void TestLastOrDefault()
        {
           var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault();
           Assert.NotNull(last);
           Assert.Equal("WOLZA", last.CustomerID);
        }

        [Fact]
        public void TestLastOrDefaultPredicate()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "London");
            Assert.NotNull(last);
            Assert.Equal("BSBEV", last.CustomerID);
        }

        [Fact]
        public void TestWhereLastOrDefault()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).Where(c => c.City == "London").LastOrDefault();
            Assert.NotNull(last);
            Assert.Equal("BSBEV", last.CustomerID);
        }

        [Fact]
        public void TestLastOrDefaultNoMatches()
        {
            var last = db.Customers.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "SpongeBob");
            Assert.Null(last);
        }

        [Fact]
        public void TestSingleFails()
        {
            Assert.Throws<Exception>(() =>
            {
                var single = db.Customers.Single();
            });
        }

        [Fact]
        public void TestSinglePredicate()
        {
            var single = db.Customers.Single(c => c.CustomerID == "ALFKI");
            Assert.NotNull(single);
            Assert.Equal("ALFKI", single.CustomerID);
        }

        [Fact]
        public void TestWhereSingle()
        {
            var single = db.Customers.Where(c => c.CustomerID == "ALFKI").Single();
            Assert.NotNull(single);
            Assert.Equal("ALFKI", single.CustomerID);
        }

        [Fact]
        public void TestSingleOrDefaultFails()
        {
            Assert.Throws<Exception>(() =>
            {
                var single = db.Customers.SingleOrDefault();
            });
        }

        [Fact]
        public void TestSingleOrDefaultPredicate()
        {
            var single = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI");
            Assert.NotNull(single);
            Assert.Equal("ALFKI", single.CustomerID);
        }

        [Fact]
        public void TestWhereSingleOrDefault()
        {
            var single = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault();
            Assert.NotNull(single);
            Assert.Equal("ALFKI", single.CustomerID);
        }

        [Fact]
        public void TestSingleOrDefaultNoMatches()
        {
            var single = db.Customers.SingleOrDefault(c => c.CustomerID == "SpongeBob");
            Assert.Null(single);
        }

        [Fact]
        public void TestAnyTopLevel()
        {
            var any = db.Customers.Any();
            Assert.True(any);
        }

        [Fact]
        public void TestAnyWithSubquery()
        {
            var list = db.Customers.Where(c => c.Orders != null && c.Orders.Any(o => o.CustomerID == "ALFKI")).ToList();
            Assert.Single(list);
        }

        [Fact]
        public void TestAnyWithSubqueryNoPredicate()
        {
            // customers with at least one order
            var list = db.Customers.Where(c => db.Orders.Where(o => o.CustomerID == c.CustomerID).Any()).ToList();
            Assert.Equal(89, list.Count);
        }

        [Fact]
        public void TestAnyWithLocalCollection()
        {
            // get customers for any one of these IDs
            string[] ids = new[] { "ALFKI", "WOLZA", "NOONE" };
            var list = db.Customers.Where(c => ids.Any(id => c.CustomerID == id)).ToList();
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void TestAllWithSubquery()
        {
            var list = db.Customers.Where(c => c.Orders != null && c.Orders.All(o => o.CustomerID == "ALFKI")).ToList();
            // includes customers w/ no orders
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void TestAllWithLocalCollection()
        {
            // get all customers with a name that contains both 'm' and 'd'  (don't use vowels since these often depend on collation)
            string[] patterns = new[] { "m", "d" };

            var list = db.Customers.Where(c => patterns.All(p => c.ContactName.Contains(p))).Select(c => c.ContactName).ToList();
            var local = db.Customers.AsEnumerable().Where(c => patterns.All(p => c.ContactName.ToLower().Contains(p))).Select(c => c.ContactName).ToList();

            Assert.Equal(local.Count, list.Count);
        }

        [Fact]
        public void TestAllTopLevel()
        {
            // all customers have name length > 0?
            var all = db.Customers.All(c => c.ContactName.Length > 0);
            Assert.True(all);
        }

        [Fact]
        public void TestAllTopLevelNoMatches()
        {
            // all customers have name with 'a'
            var all = db.Customers.All(c => c.ContactName.Contains("a"));
            Assert.False(all);
        }

        [Fact]
        public void TestContainsWithSubquery()
        {
            // this is the long-way to determine all customers that have at least one order
            var list = db.Customers.Where(c => db.Orders.Select(o => o.CustomerID).Contains(c.CustomerID)).ToList();
            Assert.Equal(89, list.Count);
        }

        [Fact]
        public void TestContainsWithLocalCollection()
        {
            string[] ids = new[] { "ALFKI", "WOLZA", "NOONE" };
            var list = db.Customers.Where(c => ids.Contains(c.CustomerID)).ToList();
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void TestContainsTopLevel()
        {
            var contains = db.Customers.Select(c => c.CustomerID).Contains("ALFKI");
            Assert.True(contains);
        }

        [Fact]
        public void TestSkipTake()
        {
            var list = db.Customers.OrderBy(c => c.CustomerID).Skip(5).Take(10).ToList();
            Assert.Equal(10, list.Count);
            Assert.Equal("BLAUS", list[0].CustomerID);
            Assert.Equal("COMMI", list[9].CustomerID);
        }

        [Fact]
        public void TestDistinctSkipTake()
        {
            var list = db.Customers.Select(c => c.City).Distinct().OrderBy(c => c).Skip(5).Take(10).ToList();
            Assert.Equal(10, list.Count);
            var hs = new HashSet<string>(list);
            Assert.Equal(10, hs.Count);
        }

        [Fact]
        public void TestCoalesce()
        {
            var list = db.Customers.Select(c => new { City = (c.City == "London" ? null : c.City), Country = (c.CustomerID == "EASTC" ? null : c.Country) })
                         .Where(x => (x.City ?? "NoCity") == "NoCity").ToList();
            Assert.Equal(6, list.Count);
            Assert.Null(list[0].City);
        }

        [Fact]
        public void TestCoalesce2()
        {
            var list = db.Customers.Select(c => new { City = (c.City == "London" ? null : c.City), Country = (c.CustomerID == "EASTC" ? null : c.Country) })
                         .Where(x => (x.City ?? x.Country ?? "NoCityOrCountry") == "NoCityOrCountry").ToList();
            Assert.Single(list);
            Assert.Null(list[0].City);
            Assert.Null(list[0].Country);
        }

        // framework function tests
        [Fact]
        public void TestStringLength()
        {
            var list = db.Customers.Where(c => c.City.Length == 7).ToList();
            Assert.Equal(9, list.Count);
        }

        [Fact]
        public void TestStringStartsWithLiteral()
        {
            var list = db.Customers.Where(c => c.ContactName.StartsWith("M")).ToList();
            Assert.Equal(12, list.Count);
        }

        [Fact]
        public void TestStringStartsWithColumn()
        {
            var list = db.Customers.Where(c => c.ContactName.StartsWith(c.ContactName)).ToList();
            Assert.Equal(91, list.Count);
        }

        [Fact]
        public void TestStringEndsWithLiteral()
        {
            var list = db.Customers.Where(c => c.ContactName.EndsWith("s")).ToList();
            Assert.Equal(9, list.Count);
        }

        [Fact]
        public void TestStringEndsWithColumn()
        {
            var list = db.Customers.Where(c => c.ContactName.EndsWith(c.ContactName)).ToList();
            Assert.Equal(91, list.Count);
        }

        [Fact]
        public void TestStringContainsLiteral()
        {
            var list = db.Customers.Where(c => c.ContactName.Contains("nd")).Select(c => c.ContactName).ToList();
            var local = db.Customers.AsEnumerable().Where(c => c.ContactName.ToLower().Contains("nd")).Select(c => c.ContactName).ToList();
            Assert.Equal(local.Count, list.Count);
        }

        [Fact]
        public void TestStringContainsColumn()
        {
            var list = db.Customers.Where(c => c.ContactName.Contains(c.ContactName)).ToList();
            Assert.Equal(91, list.Count);
        }

        [Fact]
        public void TestStringConcatImplicit2Args()
        {
            var list = db.Customers.Where(c => c.ContactName + "X" == "Maria AndersX").ToList();
            Assert.Single(list);
        }

        [Fact]
        public void TestStringConcatExplicit2Args()
        {
            var list = db.Customers.Where(c => string.Concat(c.ContactName, "X") == "Maria AndersX").ToList();
            Assert.Single(list);
        }

        [Fact]
        public void TestStringConcatExplicit3Args()
        {
            var list = db.Customers.Where(c => string.Concat(c.ContactName, "X", c.Country) == "Maria AndersXGermany").ToList();
            Assert.Single(list);
        }

        [Fact]
        public void TestStringConcatExplicitNArgs()
        {
            var list = db.Customers.Where(c => string.Concat(new string[] { c.ContactName, "X", c.Country }) == "Maria AndersXGermany").ToList();
            Assert.Single(list);
        }

        [Fact]
        public void TestStringIsNullOrEmpty()
        {
            var list = db.Customers.Select(c => c.City == "London" ? null : c.CustomerID).Where(x => string.IsNullOrEmpty(x)).ToList();
            Assert.Equal(6, list.Count);
        }

        [Fact]
        public void TestStringToUpper()
        {
            var str = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? "abc" : "abc").ToUpper());
            Assert.Equal("ABC", str);
        }

        [Fact]
        public void TestStringToLower()
        {
            var str = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? "ABC" : "ABC").ToLower());
            Assert.Equal("abc", str);
        }

        [Fact]
        public void TestStringSubstring()
        {
            var list = db.Customers.Where(c => c.City.Substring(0, 4) == "Seat").ToList();
            Assert.Single(list);
            Assert.Equal("Seattle", list[0].City);
        }

        [Fact]
        public void TestStringSubstringNoLength()
        {
            var list = db.Customers.Where(c => c.City.Substring(4) == "tle").ToList();
            Assert.Single(list);
            Assert.Equal("Seattle", list[0].City);
        }

        [Fact]
        [ExcludeProvider("SQLite")]  // no equivalent function
        public void TestStringIndexOf()
        {
            var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf("ar"));
            Assert.Equal(1, n);
        }

        [Fact]
        [ExcludeProvider("SQLite")]  // no equivalent function
        public void TestStringIndexOfChar()
        {
            var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf('r'));
            Assert.Equal(2, n);
        }

        [Fact]
        [ExcludeProvider("SQLite")] // no equivalent function
        public void TestStringIndexOfWithStart()
        {
            var n = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.ContactName.IndexOf("a", 3));
            Assert.Equal(4, n);
        }

        [Fact]
        public void TestStringTrim()
        {
            var notrim = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => ("  " + c.City + " "));
            var trim = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => ("  " + c.City + " ").Trim());
            Assert.NotEqual(notrim, trim);
            Assert.Equal(notrim?.Trim(), trim);
        }

        [Fact]
        [ExcludeProvider("SQLite")]  // no function to help build correct string representation
        [ExcludeProvider("MySql")]   // MySQL returns datetime as binary after combination of MAX and CONVERT
        public void TestDateTimeConstructYMD()
        {
            var dt = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4));
            Assert.Equal(1997, dt.Year);
            Assert.Equal(7, dt.Month);
            Assert.Equal(4, dt.Day);
            Assert.Equal(0, dt.Hour);
            Assert.Equal(0, dt.Minute);
            Assert.Equal(0, dt.Second);
        }

        [Fact]
        [ExcludeProvider("SQLite")]  // no function to help build correct string representation
        [ExcludeProvider("MySql")]   // MySQL returns datetime as binary after combination of MAX and CONVERT
        public void TestDateTimeConstructYMDHMS()
        {
            var dt = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6));
            Assert.Equal(1997, dt.Year);
            Assert.Equal(7, dt.Month);
            Assert.Equal(4, dt.Day);
            Assert.Equal(3, dt.Hour);
            Assert.Equal(5, dt.Minute);
            Assert.Equal(6, dt.Second);
        }

        [Fact]
        public void TestDateTimeDay()
        {
            var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Day);
            Assert.Equal(25, v);
        }

        [Fact]
        public void TestDateTimeMonth()
        {
            var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Month);
            Assert.Equal(8, v);
        }

        [Fact]
        public void TestDateTimeYear()
        {
            var v = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.Year);
            Assert.Equal(1997, v);
        }

        [Fact]
        [ExcludeProvider("SQLite")]   // not able to test via construction
        public void TestDateTimeHour()
        {
            var hour = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Hour);
            Assert.Equal(3, hour);
        }

        [Fact]
        [ExcludeProvider("SQLite")]   // not able to test via construction
        public void TestDateTimeMinute()
        {
            var minute = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Minute);
            Assert.Equal(5, minute);
        }

        [Fact]
        [ExcludeProvider("SQLite")]   // not able to test via construction
        public void TestDateTimeSecond()
        {
            var second = db.Customers.Where(c => c.CustomerID == "ALFKI").Max(c => new DateTime((c.CustomerID == "ALFKI") ? 1997 : 1997, 7, 4, 3, 5, 6).Second);
            Assert.Equal(6, second);
        }

        [Fact]
        public void TestDateTimeDayOfWeek()
        {
            var dow = db.Orders.Where(o => o.OrderDate == new DateTime(1997, 8, 25)).Take(1).Max(o => o.OrderDate.DayOfWeek);
            Assert.Equal(DayOfWeek.Monday, dow);
        }

        [Fact]
        [ExcludeProvider("SQLite")]
        public void TestDateTimeAddYears()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddYears(2).Year == 1999);
            Assert.NotNull(od);
        }

        [Fact]
        [ExcludeProvider("SQLite")]
        public void TestDateTimeAddMonths()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddMonths(2).Month == 10);
            Assert.NotNull(od);
        }

        [Fact]
        [ExcludeProvider("SQLite")]
        public void TestDateTimeAddDays()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddDays(2).Day == 27);
            Assert.NotNull(od);
        }

        [Fact]
        [ExcludeProvider("SQLite")]
        public void TestDateTimeAddHours()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddHours(3).Hour == 3);
            Assert.NotNull(od);
        }

        [Fact]
        [ExcludeProvider("SQLite")]
        public void TestDateTimeAddMinutes()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddMinutes(5).Minute == 5);
            Assert.NotNull(od);
        }

        [Fact]
        [ExcludeProvider("SQLite")]
        public void TestDateTimeAddSeconds()
        {
            var od = db.Orders.FirstOrDefault(o => o.OrderDate == new DateTime(1997, 8, 25) && o.OrderDate.AddSeconds(6).Second == 6);
            Assert.NotNull(od);
        }

        [Fact]
        public void TestMathAbs()
        {
            var neg1 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Abs((c.CustomerID == "ALFKI") ? -1 : 0));
            var pos1 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Abs((c.CustomerID == "ALFKI") ? 1 : 0));
            Assert.Equal(Math.Abs(-1), neg1);
            Assert.Equal(Math.Abs(1), pos1);
        }

        [Fact]
        public void TestMathAtan()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Atan((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Atan((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            Assert.Equal(Math.Atan(0.0), zero, 0.0001);
            Assert.Equal(Math.Atan(1.0), one, 0.0001);
        }

        [Fact]
        public void TestMathCos()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Cos((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Cos((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
            Assert.Equal(Math.Cos(0.0), zero, 0.0001);
            Assert.Equal(Math.Cos(Math.PI), pi, 0.0001);
        }

        [Fact]
        public void TestMathSin()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
            var pi2 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sin(((c.CustomerID == "ALFKI") ? Math.PI : Math.PI)/2.0));
            Assert.Equal(Math.Sin(0.0), zero);
            Assert.Equal(Math.Sin(Math.PI), pi, 0.0001);
            Assert.Equal(Math.Sin(Math.PI/2.0), pi2, 0.0001);
        }

        [Fact]
        public void TestMathTan()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Tan((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var pi = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Tan((c.CustomerID == "ALFKI") ? Math.PI : Math.PI));
            Assert.Equal(Math.Tan(0.0), zero, 0.0001);
            Assert.Equal(Math.Tan(Math.PI), pi, 0.0001);
        }

        [Fact]
        public void TestMathExp()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 0.0 : 0.0));
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            var two = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Exp((c.CustomerID == "ALFKI") ? 2.0 : 2.0));
            Assert.Equal(Math.Exp(0.0), zero, 0.0001);
            Assert.Equal(Math.Exp(1.0), one, 0.0001);
            Assert.Equal(Math.Exp(2.0), two, 0.0001);
        }

        [Fact]
        public void TestMathLog()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Log((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            var e = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Log((c.CustomerID == "ALFKI") ? Math.E : Math.E));
            Assert.Equal(Math.Log(1.0), one, 0.0001);
            Assert.Equal(Math.Log(Math.E), e, 0.0001);
        }

        [Fact]
        public void TestMathSqrt()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 1.0 : 1.0));
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 4.0 : 4.0));
            var nine = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Sqrt((c.CustomerID == "ALFKI") ? 9.0 : 9.0));
            Assert.Equal(1.0, one);
            Assert.Equal(2.0, four);
            Assert.Equal(3.0, nine);
        }

        [Fact]
        public void TestMathPow()
        {
            // 2^n
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 0.0));
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 1.0));
            var two = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 2.0));
            var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Pow((c.CustomerID == "ALFKI") ? 2.0 : 2.0, 3.0));
            Assert.Equal(1.0, zero);
            Assert.Equal(2.0, one);
            Assert.Equal(4.0, two);
            Assert.Equal(8.0, three);
        }

        [Fact]
        public void TestMathRoundDefault()
        {
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Round((c.CustomerID == "ALFKI") ? 3.4 : 3.4));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Round((c.CustomerID == "ALFKI") ? 3.6 : 3.6));
            Assert.Equal(3.0, four);
            Assert.Equal(4.0, six);
        }

        [Fact]
        [ExcludeProvider("Access")]
        [ExcludeProvider("SQLite")]
        public void TestMathFloor()
        {
            // The difference between floor and truncate is how negatives are handled.  Floor drops the decimals and moves the
            // value to the more negative, so Floor(-3.4) is -4.0 and Floor(3.4) is 3.0.
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? 3.4 : 3.4)));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? 3.6 : 3.6)));
            var nfour = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Floor((c.CustomerID == "ALFKI" ? -3.4 : -3.4)));
            Assert.Equal(Math.Floor(3.4), four);
            Assert.Equal(Math.Floor(3.6), six);
            Assert.Equal(Math.Floor(-3.4), nfour);
        }

        [Fact]
        [ExcludeProvider("Access")]
        [ExcludeProvider("SQLite")]
        public void TestDecimalFloor()
        {
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? 3.4m : 3.4m)));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? 3.6m : 3.6m)));
            var nfour = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Floor((c.CustomerID == "ALFKI" ? -3.4m : -3.4m)));
            Assert.Equal(decimal.Floor(3.4m), four);
            Assert.Equal(decimal.Floor(3.6m), six);
            Assert.Equal(decimal.Floor(-3.4m), nfour);
        }

        [Fact]
        [ExcludeProvider("SQLite")]
        public void TestMathTruncate()
        {
            // The difference between floor and truncate is how negatives are handled.  Truncate drops the decimals, 
            // therefore a truncated negative often has a more positive value than non-truncated (never has a less positive),
            // so Truncate(-3.4) is -3.0 and Truncate(3.4) is 3.0.
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.4 : 3.4));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.6 : 3.6));
            var neg4 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? -3.4 : -3.4));
            Assert.Equal(Math.Truncate(3.4), four);
            Assert.Equal(Math.Truncate(3.6), six);
            Assert.Equal(Math.Truncate(-3.4), neg4);
        }

        [Fact]
        public void TestStringCompareTo()
        {
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Seattle"));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Aaa"));
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => c.City.CompareTo("Berlin"));
            Assert.Equal(-1, lt);
            Assert.Equal(1, gt);
            Assert.Equal(0, eq);
        }

        [Fact]
        public void TestStringCompareToLT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") < 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") < 0);
            Assert.NotNull(cmpLT);
            Assert.Null(cmpEQ);
        }

        [Fact]
        public void TestStringCompareToLE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") <= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") <= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") <= 0);
            Assert.NotNull(cmpLE);
            Assert.NotNull(cmpEQ);
            Assert.Null(cmpGT);
        }

        [Fact]
        public void TestStringCompareToGT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") > 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") > 0);
            Assert.NotNull(cmpLT);
            Assert.Null(cmpEQ);
        }

        [Fact]
        public void TestStringCompareToGE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") >= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") >= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") >= 0);
            Assert.Null(cmpLE);
            Assert.NotNull(cmpEQ);
            Assert.NotNull(cmpGT);
        }

        [Fact]
        public void TestStringCompareToEQ()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") == 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") == 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") == 0);
            Assert.Null(cmpLE);
            Assert.NotNull(cmpEQ);
            Assert.Null(cmpGT);
        }

        [Fact]
        public void TestStringCompareToNE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Seattle") != 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Berlin") != 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => c.City.CompareTo("Aaa") != 0);
            Assert.NotNull(cmpLE);
            Assert.Null(cmpEQ);
            Assert.NotNull(cmpGT);
        }

        [Fact]
        public void TestStringCompare()
        {
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Seattle"));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Aaa"));
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => string.Compare(c.City, "Berlin"));
            Assert.Equal(-1, lt);
            Assert.Equal(1, gt);
            Assert.Equal(0, eq);
        }

        [Fact]
        public void TestStringCompareLT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") < 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") < 0);
            Assert.NotNull(cmpLT);
            Assert.Null(cmpEQ);
        }

        [Fact]
        public void TestStringCompareLE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") <= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") <= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") <= 0);
            Assert.NotNull(cmpLE);
            Assert.NotNull(cmpEQ);
            Assert.Null(cmpGT);
        }

        [Fact]
        public void TestStringCompareGT()
        {
            var cmpLT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") > 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") > 0);
            Assert.NotNull(cmpLT);
            Assert.Null(cmpEQ);
        }

        [Fact]
        public void TestStringCompareGE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") >= 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") >= 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") >= 0);
            Assert.Null(cmpLE);
            Assert.NotNull(cmpEQ);
            Assert.NotNull(cmpGT);
        }

        [Fact]
        public void TestStringCompareEQ()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") == 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") == 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") == 0);
            Assert.Null(cmpLE);
            Assert.NotNull(cmpEQ);
            Assert.Null(cmpGT);
        }

        [Fact]
        public void TestStringCompareNE()
        {
            var cmpLE = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Seattle") != 0);
            var cmpEQ = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Berlin") != 0);
            var cmpGT = db.Customers.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(c => string.Compare(c.City, "Aaa") != 0);
            Assert.NotNull(cmpLE);
            Assert.Null(cmpEQ);
            Assert.NotNull(cmpGT);
        }

        [Fact]
        public void TestIntCompareTo()
        {
            // prove that x.CompareTo(y) works for types other than string
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(10));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(9));
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => (c.CustomerID == "ALFKI" ? 10 : 10).CompareTo(11));
            Assert.Equal(0, eq);
            Assert.Equal(1, gt);
            Assert.Equal(-1, lt);
        }

        [Fact]
        public void TestDecimalCompare()
        {
            // prove that type.Compare(x,y) works with decimal
            var eq = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 10m));
            var gt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 9m));
            var lt = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Compare((c.CustomerID == "ALFKI" ? 10m : 10m), 11m));
            Assert.Equal(0, eq);
            Assert.Equal(1, gt);
            Assert.Equal(-1, lt);
        }

        [Fact]
        public void TestDecimalAdd()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Add((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
            Assert.Equal(3m, onetwo);
        }

        [Fact]
        public void TestDecimalSubtract()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Subtract((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
            Assert.Equal(-1m, onetwo);
        }

        [Fact]
        public void TestDecimalMultiply()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Multiply((c.CustomerID == "ALFKI" ? 1m : 1m), 2m));
            Assert.Equal(2m, onetwo);
        }

        [Fact]
        public void TestDecimalDivide()
        {
            var onetwo = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Divide((c.CustomerID == "ALFKI" ? 1.0m : 1.0m), 2.0m));
            Assert.Equal(0.5m, onetwo);
        }

        [Fact]
        public void TestDecimalNegate()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Negate((c.CustomerID == "ALFKI" ? 1m : 1m)));
            Assert.Equal(-1m, one);
        }

        [Fact]
        public void TestDecimalRoundDefault()
        {
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Round((c.CustomerID == "ALFKI" ? 3.4m : 3.4m)));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Round((c.CustomerID == "ALFKI" ? 3.5m : 3.5m)));
            Assert.Equal(3.0m, four);
            Assert.Equal(4.0m, six);
        }

        [Fact]
        [ExcludeProvider("SQLite")]
        public void TestDecimalTruncate()
        {
            // The difference between floor and truncate is how negatives are handled.  Truncate drops the decimals, 
            // therefore a truncated negative often has a more positive value than non-truncated (never has a less positive),
            // so Truncate(-3.4) is -3.0 and Truncate(3.4) is 3.0.
            var four = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => decimal.Truncate((c.CustomerID == "ALFKI") ? 3.4m : 3.4m));
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? 3.6m : 3.6m));
            var neg4 = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => Math.Truncate((c.CustomerID == "ALFKI") ? -3.4m : -3.4m));
            Assert.Equal(decimal.Truncate(3.4m), four);
            Assert.Equal(decimal.Truncate(3.6m), six);
            Assert.Equal(decimal.Truncate(-3.4m), neg4);
        }

        [Fact]
        public void TestDecimalLT()
        {
            // prove that decimals are treated normally with respect to normal comparison operators
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1.0m : 3.0m) < 2.0m);
            Assert.NotNull(alfki);
        }

        [Fact]
        public void TestIntLessThan()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) < 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) < 2);
            Assert.NotNull(alfki);
            Assert.Null(alfkiN);
        }

        [Fact]
        public void TestIntLessThanOrEqual()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) <= 2);
            var alfki2 = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 3) <= 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) <= 2);
            Assert.NotNull(alfki);
            Assert.NotNull(alfki2);
            Assert.Null(alfkiN);
        }

        [Fact]
        public void TestIntGreaterThan()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) > 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) > 2);
            Assert.NotNull(alfki);
            Assert.Null(alfkiN);
        }

        [Fact]
        public void TestIntGreaterThanOrEqual()
        {
            var alfki = db.Customers.Single(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 1) >= 2);
            var alfki2 = db.Customers.Single(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 3 : 2) >= 2);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 3) > 2);
            Assert.NotNull(alfki);
            Assert.NotNull(alfki2);
            Assert.Null(alfkiN);
        }

        [Fact]
        public void TestIntEqual()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 1) == 1);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 1 : 1) == 2);
            Assert.NotNull(alfki);
            Assert.Null(alfkiN);
        }

        [Fact]
        public void TestIntNotEqual()
        {
            var alfki = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 2) != 1);
            var alfkiN = db.Customers.SingleOrDefault(c => c.CustomerID == "ALFKI" && (c.CustomerID == "ALFKI" ? 2 : 2) != 2);
            Assert.NotNull(alfki);
            Assert.Null(alfkiN);
        }

        [Fact]
        public void TestIntAdd()
        {
            var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) + 2);
            Assert.Equal(3, three);
        }

        [Fact]
        public void TestIntSubtract()
        {
            var negone = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) - 2);
            Assert.Equal(-1, negone);
        }

        [Fact]
        public void TestIntMultiply()
        {
            var six = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 2 : 2) * 3);
            Assert.Equal(6, six);
        }

        [Fact]
        public void TestIntDivide()
        {
            var one = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 3 : 3) / 2);
            Assert.Equal(1, one);
        }

        [Fact]
        public void TestIntModulo()
        {
            var three = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 7 : 7) % 4);
            Assert.Equal(3, three);
        }

        [Fact]
        public void TestIntLeftShift()
        {
            var eight = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) << 3);
            Assert.Equal(8, eight);
        }

        [Fact]
        public void TestIntRightShift()
        {
            var eight = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 32 : 32) >> 2);
            Assert.Equal(8, eight);
        }

        [Fact]
        public void TestIntBitwiseAnd()
        {
            var band = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 6 : 6) & 3);
            Assert.Equal(2, band);
        }

        [Fact]
        public void TestIntBitwiseOr()
        {
            var eleven = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 10 : 10) | 3);
            Assert.Equal(11, eleven);
        }

        [Fact]
        public void TestIntBitwiseExclusiveOr()
        {
            var zero = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ((c.CustomerID == "ALFKI") ? 1 : 1) ^ 1);
            Assert.Equal(0, zero);
        }

        [Fact]
        public void TestIntBitwiseNot()
        {
            var bneg = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => ~((c.CustomerID == "ALFKI") ? -1 : -1));
            Assert.Equal(~-1, bneg);
        }

        [Fact]
        public void TestIntNegate()
        {
            var neg = db.Customers.Where(c => c.CustomerID == "ALFKI").Sum(c => -((c.CustomerID == "ALFKI") ? 1 : 1));
            Assert.Equal(-1, neg);
        }

        [Fact]
        public void TestAnd()
        {
            var custs = db.Customers.Where(c => c.Country == "USA" && c.City.StartsWith("A")).Select(c => c.City).ToList();
            Assert.Equal(2, custs.Count);
            Assert.True(custs.All(c => c.StartsWith("A")));
        }

        [Fact]
        [ExcludeProvider("MySqlClient")]
        public void TestOr()
        {
            var custs = db.Customers.Where(c => c.Country == "USA" || c.City.StartsWith("A")).Select(c => new { c.Country, c.City }).ToList();
            Assert.Equal(14, custs.Count);
        }

        [Fact]
        public void TestNot()
        {
            var custs = db.Customers.Where(c => !(c.Country == "USA")).Select(c => c.Country).ToList();
            Assert.Equal(78, custs.Count);
        }

        [Fact]
        public void TestEqualLiteralNull()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => x == null);
            Assert.Contains("IS NULL", this.GetProvider().GetQueryText(q.Expression));
            var n = q.Count();
            Assert.Equal(1, n);
        }

        [Fact]
        public void TestEqualLiteralNullReversed()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => null == x);
            Assert.Contains("IS NULL", this.GetProvider().GetQueryText(q.Expression));
            var n = q.Count();
            Assert.Equal(1, n);
        }

        [Fact]
        public void TestNotEqualLiteralNull()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => x != null);
            Assert.Contains("IS NOT NULL", this.GetProvider().GetQueryText(q.Expression));
            var n = q.Count();
            Assert.Equal(90, n);
        }

        [Fact]
        public void TestNotEqualLiteralNullReversed()
        {
            var q = db.Customers.Select(c => c.CustomerID == "ALFKI" ? null : c.CustomerID).Where(x => null != x);
            Assert.Contains("IS NOT NULL", this.GetProvider().GetQueryText(q.Expression));
            var n = q.Count();
            Assert.Equal(90, n);
        }

        [Fact]
        public void TestRelationshipEqualNull()
        {
            var q = db.Orders.Where(o => o.Customer == null);
            Assert.Contains("IS NULL", this.GetProvider().GetQueryText(q.Expression));
            Assert.Empty(q);
        }

        [Fact]
        public void TestRelationshipEqualNullReversed()
        {
            var q = db.Orders.Where(o => null == o.Customer);
            Assert.Contains("IS NULL", this.GetProvider().GetQueryText(q.Expression));
            Assert.Empty(q);
        }

        [Fact]
        public void TestRelationshipNotEqualNull()
        {
            var q = db.Orders.Where(o => o.Customer != null);
            Assert.Contains("IS NOT NULL", this.GetProvider().GetQueryText(q.Expression));
            var n = q.Count();
            Assert.Equal(830, n);
        }

        [Fact]
        public void TestRelationshipNotEqualNullReversed()
        {
            var q = db.Orders.Where(o => null != o.Customer);
            Assert.Contains("IS NOT NULL", this.GetProvider().GetQueryText(q.Expression));
            var n = q.Count();
            Assert.Equal(830, n);
        }

        [Fact]
        public void TestConditionalResultsArePredicates()
        {
            bool value = db.Orders.Where(c => c.CustomerID == "ALFKI").Max(c => (c.CustomerID == "ALFKI" ? string.Compare(c.CustomerID, "POTATO") < 0 : string.Compare(c.CustomerID, "POTATO") > 0));
            Assert.True(value);
        }

        [Fact]
        public void TestConditionalWithInvalidCase()
        {
            bool? hasOrders = null;

            var query = db.Customers.Select(r => new
            {
                CustomerID = r.CustomerID,
                HasOrders = hasOrders != null
                                ? (bool)hasOrders
                                : db.Orders.Any(o => o.CustomerID.Equals(r.CustomerID))
            });

            var test = query.ToList();
            Assert.Equal(91, test.Count());
        }

        [Fact]
        public void TestSelectManyJoined()
        {
            var cods = 
                (from c in db.Customers
                from o in db.Orders.Where(o => o.CustomerID == c.CustomerID)
                select new { c.ContactName, o.OrderDate }).ToList();
            Assert.Equal(830, cods.Count);
        }

        [Fact]
        public void TestSelectManyJoinedDefaultIfEmpty()
        {
            var cods = (
                from c in db.Customers
                from o in db.Orders.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                select new { c.ContactName, o.OrderDate }
                ).ToList();
            Assert.Equal(832, cods.Count);
        }

        [Fact]
        public void TestAssociationInWhere()
        {
            var ords = (
                from o in db.Orders
                where o.Customer.City == "Seattle"
                select o
                ).ToList();
            Assert.Equal(14, ords.Count);
        }

        [Fact]
        public void TestAssociationInWhereDuplicate()
        {
            var n = db.Orders.Where(c => c.CustomerID == "WHITC").Count();
            var ords = (
                from o in db.Orders
                where o.Customer.Country == "USA" && o.Customer.City == "Seattle"
                select o
                ).ToList();
            Assert.Equal(n, ords.Count);
        }

        [Fact]
        public void TestAssociationInWhereDeep()
        {
            var q = from d in db.OrderDetails
                    where d.Order != null && d.Order.Customer != null && d.Order.Customer.CustomerID == "VINET"
                    select d;

            var ods = q.ToList();
            Assert.Equal(10, ods.Count);
        }

        [Fact]
        public void TestAssocationInSelectDeep()
        {
            var q = from od in db.OrderDetails
                    where od.Order != null && od.Order.Customer != null
                    select od.Order.Customer.CompanyName;
            var result = q.ToList();
            Assert.Equal(2155, result.Count);
        }

        [Fact]
        public void TestAssociationInOrderByDeep()
        {
            var q = from od in db.OrderDetails
                    where od.Order != null && od.Order.Customer != null
                    orderby od.Order.Customer.CompanyName
                    select od.OrderID;

            var result = q.ToList();
        }

        /*
        public void TestAssociationInGroupByDeep()
        {
            var q = from od in db.OrderDetails
                    group od by od.Order.Customer.CustomerID;
            var result = q.ToList();
        }
        */

        [Fact]
        public void TestAssociationInSelect()
        {
            var custs = (
                from o in db.Orders
                where o.CustomerID == "ALFKI"
                select o.Customer
                ).ToList();
            Assert.Equal(6, custs.Count);
            Assert.True(custs.All(c => c.CustomerID == "ALFKI"));
        }

        [Fact]
        public void TestAssociationInSelectDuplicate()
        {
            var doubleCusts = (
                from o in db.Orders
                where o.CustomerID == "ALFKI"
                select new { A = o.Customer, B = o.Customer }
                ).ToList();

            Assert.Equal(6, doubleCusts.Count);
            Assert.True(doubleCusts.All(c => c.A.CustomerID == "ALFKI" && c.B.CustomerID == "ALFKI"));
        }

        [Fact]
        public void TestAssociationInSelectAndWhereDuplicate()
        {
            var stuff = (
                from o in db.Orders
                where o.Customer.Country == "USA"
                where o.Customer.City != "Seattle"
                select new { A = o.Customer, B = o.Customer }
                ).ToList();
            Assert.Equal(108, stuff.Count);
        }

        [Fact]
        public void TestCustomersIncludeOrders()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Customer>(c => c.Orders);
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
            Assert.Single(custs);
            Assert.NotNull(custs[0].Orders);
            Assert.Equal(6, custs[0].Orders?.Count);
        }

        [Fact]
        public void TestCustomersIncludeOrdersAndDetails()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Customer>(c => c.Orders);
            policy.IncludeWith<Order>(o => o.Details);
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
            Assert.Single(custs);
            Assert.NotNull(custs[0].Orders);
            Assert.Equal(6, custs[0].Orders?.Count);
            Assert.True(custs[0].Orders?.Any(o => o.OrderID == 10643));
            Assert.NotNull(custs[0].Orders?.Single(o => o.OrderID == 10643).Details);
            Assert.Equal(3, custs[0].Orders?.Single(o => o.OrderID == 10643)?.Details?.Count);
        }

        [Fact]
        public void TestCustomersIncludeOrdersViaConstructorOnly()
        {
            var mapping = new AttributeMapping(typeof(NorthwindX));
            var policy = new EntityPolicy();
            policy.IncludeWith<CustomerX>(c => c.Orders);
            NorthwindX nw = new NorthwindX(this.GetProvider().WithPolicy(policy).WithMapping(mapping));

            var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
            Assert.Single(custs);
            Assert.NotNull(custs[0].Orders);
            Assert.Equal(6, custs[0].Orders?.Count);
        }

        [Fact]
        public void TestCustomersIncludeOrdersWhere()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Customer>(c => c.Orders.Where(o => (o.OrderID & 1) == 0));
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
            Assert.Single(custs);
            Assert.NotNull(custs[0].Orders);
            Assert.Equal(3, custs[0].Orders?.Count);
        }

        [Fact]
        public void TestCustomersIncludeOrdersDeferred()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Customer>(c => c.Orders, true);
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
            Assert.Single(custs);
            Assert.NotNull(custs[0].Orders);
            Assert.Equal(6, custs[0].Orders?.Count);
        }

        [Fact]
        public void TestCustomersAssociateOrders()
        {
            var policy = new EntityPolicy();
            policy.AssociateWith<Customer>(c => c.Orders.Where(o => (o.OrderID & 1) == 0));
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI")
                .Select(c => new { CustomerID = c.CustomerID, FilteredOrdersCount = c.Orders.Count() }).ToList();
            Assert.Single(custs);
            Assert.Equal(3, custs[0].FilteredOrdersCount);
        }

        [Fact]
        public void TestCustomersIncludeThenAssociateOrders()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Customer>(c => c.Orders);
            policy.AssociateWith<Customer>(c => c.Orders.Where(o => (o.OrderID & 1) == 0));
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
            Assert.Single(custs);
            Assert.NotNull(custs[0].Orders);
            Assert.Equal(3, custs[0].Orders?.Count);
        }

        [Fact]
        public void TestCustomersAssociateThenIncludeOrders()
        {
            var policy = new EntityPolicy();
            policy.AssociateWith<Customer>(c => c.Orders.Where(o => (o.OrderID & 1) == 0));
            policy.IncludeWith<Customer>(c => c.Orders);
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
            Assert.Single(custs);
            Assert.NotNull(custs[0].Orders);
            Assert.Equal(3, custs[0].Orders?.Count);
        }

        [Fact]
        public void TestOrdersIncludeDetailsWithGroupBy()
        {
            var policy = new EntityPolicy();
            policy.IncludeWith<Order>(o => o.Details);
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));
            var list = nw.Orders.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).ToList();
            Assert.Single(list);
            var grp = list[0].ToList();
            Assert.Equal(6, grp.Count);
            var o10643 = grp.SingleOrDefault(o => o.OrderID == 10643);
            Assert.NotNull(o10643);
            Assert.Equal(3, o10643?.Details?.Count);
        }

        [Fact]
        public void TestCustomersApplyFilter()
        {
            var policy = new EntityPolicy();
            policy.Apply<Customer>(seq => seq.Where(c => c.City == "London"));
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.ToList();
            Assert.Equal(6, custs.Count);
        }

        [Fact]
        public void TestCustomersApplyComputedFilter()
        {
            string ci = "Lon";
            string ty = "don";
            var policy = new EntityPolicy();
            policy.Apply<Customer>(seq => seq.Where(c => c.City == ci + ty));
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.ToList();
            Assert.Equal(6, custs.Count);
        }

        [Fact]
        public void TestCustomersApplyFilterTwice()
        {
            var policy = new EntityPolicy();
            policy.Apply<Customer>(seq => seq.Where(c => c.City == "London"));
            policy.Apply<Customer>(seq => seq.Where(c => c.Country == "UK"));
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.ToList();
            Assert.Equal(6, custs.Count);
        }

        [Fact]
        public void TestCustomersApplyOrder()
        {
            var policy = new EntityPolicy();
            policy.Apply<Customer>(seq => seq.OrderBy(c => c.ContactName));
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var list = nw.Customers.Where(c => c.City == "London").ToList();

            Assert.Equal(6, list.Count);
            var sorted = list.OrderBy(c => c.ContactName).ToList();
            Assert.True(Enumerable.SequenceEqual(list, sorted));
        }

        [Fact]
        public void TestCustomersApplyOrderAndAssociateOrders()
        {
            var policy = new EntityPolicy();
            policy.Apply<Order>(ords => ords.Where(o => o.OrderDate.Year > 0));
            policy.IncludeWith<Customer>(c => c.Orders.Where(o => (o.OrderID & 1) == 0));
            Northwind nw = new Northwind(this.GetProvider().WithPolicy(policy));

            var custs = nw.Customers.Where(c => c.CustomerID == "ALFKI").ToList();
            Assert.Single(custs);
            Assert.NotNull(custs[0].Orders);
            Assert.Equal(3, custs[0].Orders?.Count);
        }

        [Fact]
        public void TestOrdersIncludeDetailsWithFirst()
        {
            EntityPolicy policy = new EntityPolicy();
            policy.IncludeWith<Order>(o => o.Details);

            var ndb = new Northwind(this.GetProvider().WithPolicy(policy));
            var q = from o in ndb.Orders
                    where o.OrderID == 10248
                    select o;

            Order so = q.Single();
            Assert.Equal(3, so.Details?.Count);
            Order fo = q.First();
            Assert.Equal(3, fo.Details?.Count);
        }
    }
}