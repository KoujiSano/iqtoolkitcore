namespace Test
{
    public abstract class NorthwindCUDTests : NorthwindTestBase
    {
        //public override void RunTest(Action testAction)
        //{
        //    this.CleaupDatabase();
        //    base.RunTest(testAction);
        //}

        public NorthwindCUDTests()
        {
            this.CleaupDatabase();
            // base.Setup(args);
        }

        public override void Dispose()
        {
            this.CleaupDatabase();
            base.Dispose();
        }

        private void CleaupDatabase()
        {
            this.ExecSilent("DELETE FROM Orders WHERE CustomerID LIKE 'XX%'");
            this.ExecSilent("DELETE FROM Customers WHERE CustomerID LIKE 'XX%'");
        }

        [Fact]
        public void TestInsertCustomerNoResult()
        {
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };
            var result = db.Customers.Insert(cust);
            Assert.Equal(1, result);  // returns 1 for success
        }

        [Fact]
        public void TestInsertCustomerWithResult()
        {
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };
            var result = db.Customers.Insert(cust, c => c.City);
            Assert.Equal(result, "Seattle");  // should be value we asked for
        }

        [Fact]
        public void TestBatchInsertCustomersNoResult()
        {
            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Seattle",
                    Country = "USA"
                });
            var results = db.Customers.Batch(custs, (u, c) => u.Insert(c));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, 1)));
        }

        [Fact]
        public void TestBatchInsertCustomersWithResult()
        {
            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Seattle",
                    Country = "USA"
                });
            var results = db.Customers.Batch(custs, (u, c) => u.Insert(c, d => d.City));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, "Seattle")));
        }

        [Fact]
        [ExcludeProvider("Access")] // problem with generating OrderID
        public void TestInsertOrderWithNoResult()
        {
            this.TestInsertCustomerNoResult(); // create customer "XX1"
            var order = new Order
            {
                CustomerID = "XX1",
                OrderDate = DateTime.Today,
            };
            var result = db.Orders.Insert(order);
            Assert.Equal(1, result);
        }

        [Fact]
        [ExcludeProvider("Access")] // problem with generating OrderID
        public void TestInsertOrderWithGeneratedIDResult()
        {
            this.TestInsertCustomerNoResult(); // create customer "XX1"
            var order = new Order
            {
                CustomerID = "XX1",
                OrderDate = DateTime.Today,
            };
            var result = db.Orders.Insert(order, o => o.OrderID);
            Assert.NotEqual(1, result);
        }

        [Fact]
        public void TestUpdateCustomerNoResult()
        {
            this.TestInsertCustomerNoResult(); // create customer "XX1"

            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Portland", // moved to Portland!
                Country = "USA"
            };

            var result = db.Customers.Update(cust);
            Assert.Equal(1, result);
        }

        [Fact]
        public void TestUpdateCustomerWithResult()
        {
            this.TestInsertCustomerNoResult(); // create customer "XX1"

            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Portland", // moved to Portland!
                Country = "USA"
            };

            var result = db.Customers.Update(cust, null, c => c.City);
            Assert.Equal("Portland", result);
        }

        [Fact]
        public void TestUpdateCustomerWithUpdateCheckThatDoesNotSucceed()
        {
            this.TestInsertCustomerNoResult(); // create customer "XX1"

            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Portland", // moved to Portland!
                Country = "USA"
            };

            var result = db.Customers.Update(cust, d => d.City == "Detroit");
            Assert.Equal(0, result); // 0 for failure
        }

        [Fact]
        public void TestUpdateCustomerWithUpdateCheckThatSucceeds()
        {
            this.TestInsertCustomerNoResult(); // create customer "XX1"

            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Portland", // moved to Portland!
                Country = "USA"
            };

            var result = db.Customers.Update(cust, d => d.City == "Seattle");
            Assert.Equal(1, result);
        }

        [Fact]
        public void TestBatchUpdateCustomer()
        {
            this.TestBatchInsertCustomersNoResult();

            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Seattle",
                    Country = "USA"
                });

            var results = db.Customers.Batch(custs, (u, c) => u.Update(c));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, 1)));
        }

        [Fact]
        public void TestBatchUpdateCustomerWithUpdateCheck()
        {
            this.TestBatchInsertCustomersNoResult();

            int n = 10;
            var pairs = Enumerable.Range(1, n).Select(
                i => new
                {
                    original = new Customer
                    {
                        CustomerID = "XX" + i,
                        CompanyName = "Company" + i,
                        ContactName = "Contact" + i,
                        City = "Seattle",
                        Country = "USA"
                    },
                    current = new Customer
                    {
                        CustomerID = "XX" + i,
                        CompanyName = "Company" + i,
                        ContactName = "Contact" + i,
                        City = "Portland",
                        Country = "USA"
                    }
                });

            var results = db.Customers.Batch(pairs, (u, x) => u.Update(x.current, d => d.City == x.original.City));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, 1)));
        }

        [Fact]
        public void TestBatchUpdateCustomerWithResult()
        {
            this.TestBatchInsertCustomersNoResult();

            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Portland",
                    Country = "USA"
                });

            var results = db.Customers.Batch(custs, (u, c) => u.Update(c, null, d => d.City));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, "Portland")));
        }

        [Fact]
        public void TestBatchUpdateCustomerWithUpdateCheckAndResult()
        {
            this.TestBatchInsertCustomersNoResult();

            int n = 10;
            var pairs = Enumerable.Range(1, n).Select(
                i => new
                {
                    original = new Customer
                    {
                        CustomerID = "XX" + i,
                        CompanyName = "Company" + i,
                        ContactName = "Contact" + i,
                        City = "Seattle",
                        Country = "USA"
                    },
                    current = new Customer
                    {
                        CustomerID = "XX" + i,
                        CompanyName = "Company" + i,
                        ContactName = "Contact" + i,
                        City = "Portland",
                        Country = "USA"
                    }
                });

            var results = db.Customers.Batch(pairs, (u, x) => u.Update(x.current, d => d.City == x.original.City, d => d.City));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, "Portland")));
        }

        [Fact]
        public void TestUpsertNewCustomerNoResult()
        {
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle", // moved to Portland!
                Country = "USA"
            };

            var result = db.Customers.InsertOrUpdate(cust);
            Assert.Equal(1, result);
        }

        [Fact]
        public void TestUpsertExistingCustomerNoResult()
        {
            this.TestInsertCustomerNoResult();

            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Portland", // moved to Portland!
                Country = "USA"
            };

            var result = db.Customers.InsertOrUpdate(cust);
            Assert.Equal(1, result);
        }

        [Fact]
        public void TestUpsertNewCustomerWithResult()
        {
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle", // moved to Portland!
                Country = "USA"
            };

            var result = db.Customers.InsertOrUpdate(cust, null, d => d.City);
            Assert.Equal("Seattle", result);
        }

        [Fact]
        public void TestUpsertExistingCustomerWithResult()
        {
            this.TestInsertCustomerNoResult();

            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Portland", // moved to Portland!
                Country = "USA"
            };

            var result = db.Customers.InsertOrUpdate(cust, null, d => d.City);
            Assert.Equal("Portland", result);
        }

        [Fact]
        public void TestUpsertNewCustomerWithUpdateCheck()
        {
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Portland", // moved to Portland!
                Country = "USA"
            };

            var result = db.Customers.InsertOrUpdate(cust, d => d.City == "Portland");
            Assert.Equal(1, result);
        }

        [Fact]
        public void TestUpsertExistingCustomerWithUpdateCheck()
        {
            this.TestInsertCustomerNoResult();

            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Portland", // moved to Portland!
                Country = "USA"
            };

            var result = db.Customers.InsertOrUpdate(cust, d => d.City == "Seattle");
            Assert.Equal(1, result);
        }

        [Fact]
        public void TestBatchUpsertNewCustomersNoResult()
        {
            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Portland",
                    Country = "USA"
                });

            var results = db.Customers.Batch(custs, (u, c) => u.InsertOrUpdate(c));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, 1)));
        }

        [Fact]
        public void TestBatchUpsertExistingCustomersNoResult()
        {
            this.TestBatchInsertCustomersNoResult();

            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Portland",
                    Country = "USA"
                });

            var results = db.Customers.Batch(custs, (u, c) => u.InsertOrUpdate(c));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, 1)));
        }

        [Fact]
        public void TestBatchUpsertNewCustomersWithResult()
        {
            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Portland",
                    Country = "USA"
                });

            var results = db.Customers.Batch(custs, (u, c) => u.InsertOrUpdate(c, null, d => d.City));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, "Portland")));
        }

        [Fact]
        public void TestBatchUpsertExistingCustomersWithResult()
        {
            this.TestBatchInsertCustomersNoResult();

            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Portland",
                    Country = "USA"
                });

            var results = db.Customers.Batch(custs, (u, c) => u.InsertOrUpdate(c, null, d => d.City));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, "Portland")));
        }

        [Fact]
        public void TestBatchUpsertNewCustomersWithUpdateCheck()
        {
            int n = 10;
            var pairs = Enumerable.Range(1, n).Select(
                i => new
                {
                    original = new Customer
                    {
                        CustomerID = "XX" + i,
                        CompanyName = "Company" + i,
                        ContactName = "Contact" + i,
                        City = "Seattle",
                        Country = "USA"
                    },
                    current = new Customer
                    {
                        CustomerID = "XX" + i,
                        CompanyName = "Company" + i,
                        ContactName = "Contact" + i,
                        City = "Portland",
                        Country = "USA"
                    }
                });

            var results = db.Customers.Batch(pairs, (u, x) => u.InsertOrUpdate(x.current, d => d.City == x.original.City));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, 1)));
        }

        [Fact]
        public void TestBatchUpsertExistingCustomersWithUpdateCheck()
        {
            this.TestBatchInsertCustomersNoResult();

            int n = 10;
            var pairs = Enumerable.Range(1, n).Select(
                i => new
                {
                    original = new Customer
                    {
                        CustomerID = "XX" + i,
                        CompanyName = "Company" + i,
                        ContactName = "Contact" + i,
                        City = "Seattle",
                        Country = "USA"
                    },
                    current = new Customer
                    {
                        CustomerID = "XX" + i,
                        CompanyName = "Company" + i,
                        ContactName = "Contact" + i,
                        City = "Portland",
                        Country = "USA"
                    }
                });

            var results = db.Customers.Batch(pairs, (u, x) => u.InsertOrUpdate(x.current, d => d.City == x.original.City));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, 1)));
        }

        [Fact]
        public void TestDeleteCustomer()
        {
            this.TestInsertCustomerNoResult();

            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };

            var result = db.Customers.Delete(cust);
            Assert.Equal(1, result);
        }

        [Fact]
        public void TestDeleteCustomerForNonExistingCustomer()
        {
            this.TestInsertCustomerNoResult();

            var cust = new Customer
            {
                CustomerID = "XX2",
                CompanyName = "Company2",
                ContactName = "Contact2",
                City = "Seattle",
                Country = "USA"
            };

            var result = db.Customers.Delete(cust);
            Assert.Equal(0, result);
        }

        [Fact]
        public void TestDeleteCustomerWithDeleteCheckThatSucceeds()
        {
            this.TestInsertCustomerNoResult();

            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };

            var result = db.Customers.Delete(cust, d => d.City == "Seattle");
            Assert.Equal(1, result);
        }

        [Fact]
        public void TestDeleteCustomerWithDeleteCheckThatDoesNotSucceed()
        {
            this.TestInsertCustomerNoResult();

            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };

            var result = db.Customers.Delete(cust, d => d.City == "Portland");
            Assert.Equal(0, result);
        }

        [Fact]
        public void TestBatchDeleteCustomers()
        {
            this.TestBatchInsertCustomersNoResult();

            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Seattle",
                    Country = "USA"
                });

            var results = db.Customers.Batch(custs, (u, c) => u.Delete(c));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, 1)));
        }

        [Fact]
        public void TestBatchDeleteCustomersWithDeleteCheck()
        {
            this.TestBatchInsertCustomersNoResult();

            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Seattle",
                    Country = "USA"
                });

            var results = db.Customers.Batch(custs, (u, c) => u.Delete(c, d => d.City == c.City));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, 1)));
        }

        [Fact]
        public void TestBatchDeleteCustomersWithDeleteCheckThatDoesNotSucceed()
        {
            this.TestBatchInsertCustomersNoResult();

            int n = 10;
            var custs = Enumerable.Range(1, n).Select(
                i => new Customer
                {
                    CustomerID = "XX" + i,
                    CompanyName = "Company" + i,
                    ContactName = "Contact" + i,
                    City = "Portland",
                    Country = "USA"
                });

            var results = db.Customers.Batch(custs, (u, c) => u.Delete(c, d => d.City == c.City));
            Assert.Equal(n, results.Count());
            Assert.Equal(true, results.All(r => object.Equals(r, 0)));
        }

        [Fact]
        public void TestDeleteWhere()
        {
            this.TestBatchInsertCustomersNoResult();

            var result = db.Customers.Delete(c => c.CustomerID.StartsWith("XX"));
            Assert.Equal(10, result);
        }

        [Fact]
        public void TestSessionIdentityCache()
        {
            NorthwindSession ns = new NorthwindSession(this.GetProvider());

            // both objects should be the same instance
            var cust = ns.Customers.Single(c => c.CustomerID == "ALFKI");
            var cust2 = ns.Customers.Single(c => c.CustomerID == "ALFKI");

            Assert.NotEqual(null, cust);
            Assert.NotEqual(null, cust2);
            Assert.Equal(cust, cust2);
        }

        [Fact]
        public void TestSessionProviderNotIdentityCached()
        {
            NorthwindSession ns = new NorthwindSession(this.GetProvider());
            Northwind db2 = new Northwind(ns.Session.Provider);

            // both objects should be different instances
            var cust = ns.Customers.Single(c => c.CustomerID == "ALFKI");
            var cust2 = ns.Customers.Table.Single(c => c.CustomerID == "ALFKI");

            Assert.NotEqual(null, cust);
            Assert.NotEqual(null, cust2);
            Assert.Equal(cust.CustomerID, cust2.CustomerID);
            Assert.NotEqual(cust, cust2);
        }

        [Fact]
        public void TestSessionSubmitActionOnModify()
        {
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };

            this.db.Customers.Insert(cust);

            var ns = new NorthwindSession(this.GetProvider());
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            // fetch the previously inserted customer
            cust = ns.Customers.Single(c => c.CustomerID == "XX1");
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            cust.ContactName = "Contact Modified";
            Assert.Equal(SubmitAction.Update, ns.Customers.GetSubmitAction(cust));

            ns.SubmitChanges();
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            // prove actually modified by fetching through provider
            var cust2 = this.db.Customers.Single(c => c.CustomerID == "XX1");
            Assert.Equal("Contact Modified", cust2.ContactName);

            // ready to be submitted again!
            cust.City = "SeattleX";
            Assert.Equal(SubmitAction.Update, ns.Customers.GetSubmitAction(cust));
        }

        [Fact]
        public void TestSessionSubmitActionOnInsert()
        {
            NorthwindSession ns = new NorthwindSession(this.GetProvider());
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            ns.Customers.InsertOnSubmit(cust);
            Assert.Equal(SubmitAction.Insert, ns.Customers.GetSubmitAction(cust));

            ns.SubmitChanges();
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            cust.City = "SeattleX";
            Assert.Equal(SubmitAction.Update, ns.Customers.GetSubmitAction(cust));
        }

        [Fact]
        public void TestSessionSubmitActionOnInsertOrUpdate()
        {
            NorthwindSession ns = new NorthwindSession(this.GetProvider());
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            ns.Customers.InsertOrUpdateOnSubmit(cust);
            Assert.Equal(SubmitAction.InsertOrUpdate, ns.Customers.GetSubmitAction(cust));

            ns.SubmitChanges();
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            cust.City = "SeattleX";
            Assert.Equal(SubmitAction.Update, ns.Customers.GetSubmitAction(cust));
        }

        [Fact]
        public void TestSessionSubmitActionOnUpdate()
        {
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };
            this.db.Customers.Insert(cust);

            NorthwindSession ns = new NorthwindSession(this.GetProvider());
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            ns.Customers.UpdateOnSubmit(cust);
            Assert.Equal(SubmitAction.Update, ns.Customers.GetSubmitAction(cust));

            ns.SubmitChanges();
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            cust.City = "SeattleX";
            Assert.Equal(SubmitAction.Update, ns.Customers.GetSubmitAction(cust));
        }

        [Fact]
        public void TestSessionSubmitActionOnDelete()
        {
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };
            this.db.Customers.Insert(cust);

            NorthwindSession ns = new NorthwindSession(this.GetProvider());
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            ns.Customers.DeleteOnSubmit(cust);
            Assert.Equal(SubmitAction.Delete, ns.Customers.GetSubmitAction(cust));

            ns.SubmitChanges();
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            // modifications after delete don't trigger updates
            cust.City = "SeattleX";
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));
        }

        [Fact]
        public void TestDeleteThenInsertSamePK()
        {
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };

            var cust2 = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company2",
                ContactName = "Contact2",
                City = "Chicago",
                Country = "USA"
            };

            this.db.Customers.Insert(cust);

            NorthwindSession ns = new NorthwindSession(this.GetProvider());
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust2));

            ns.Customers.DeleteOnSubmit(cust);
            Assert.Equal(SubmitAction.Delete, ns.Customers.GetSubmitAction(cust));
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust2));

            ns.Customers.InsertOnSubmit(cust2);
            Assert.Equal(SubmitAction.Delete, ns.Customers.GetSubmitAction(cust));
            Assert.Equal(SubmitAction.Insert, ns.Customers.GetSubmitAction(cust2));

            ns.SubmitChanges();
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust2));

            // modifications after delete don't trigger updates
            cust.City = "SeattleX";
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            // modifications after insert do trigger updates
            cust2.City = "ChicagoX";
            Assert.Equal(SubmitAction.Update, ns.Customers.GetSubmitAction(cust2));
        }

        [Fact]
        public void TestInsertThenDeleteSamePK()
        {
            var cust = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company1",
                ContactName = "Contact1",
                City = "Seattle",
                Country = "USA"
            };

            var cust2 = new Customer
            {
                CustomerID = "XX1",
                CompanyName = "Company2",
                ContactName = "Contact2",
                City = "Chicago",
                Country = "USA"
            };

            this.db.Customers.Insert(cust);

            NorthwindSession ns = new NorthwindSession(this.GetProvider());
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust2));

            ns.Customers.InsertOnSubmit(cust2);
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));
            Assert.Equal(SubmitAction.Insert, ns.Customers.GetSubmitAction(cust2));

            ns.Customers.DeleteOnSubmit(cust);
            Assert.Equal(SubmitAction.Delete, ns.Customers.GetSubmitAction(cust));
            Assert.Equal(SubmitAction.Insert, ns.Customers.GetSubmitAction(cust2));

            ns.SubmitChanges();
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust2));

            // modifications after delete don't trigger updates
            cust.City = "SeattleX";
            Assert.Equal(SubmitAction.None, ns.Customers.GetSubmitAction(cust));

            // modifications after insert do trigger updates
            cust2.City = "ChicagoX";
            Assert.Equal(SubmitAction.Update, ns.Customers.GetSubmitAction(cust2));
        }

        [Fact]
        [ExcludeProvider("Access")] // Access does not auto generate the OrderID
        public void TestSessionGeneratedId()
        {
            this.TestInsertCustomerNoResult(); // create customer "XX1"

            NorthwindSession ns = new NorthwindSession(this.GetProvider());

            var order = new Order
            {
                CustomerID = "XX1",
                OrderDate = DateTime.Today,
            };

            ns.Orders.InsertOnSubmit(order);

            Assert.Equal(0, order.OrderID);
            ns.SubmitChanges();

            Assert.NotEqual(0, order.OrderID);
        }
    }
}
