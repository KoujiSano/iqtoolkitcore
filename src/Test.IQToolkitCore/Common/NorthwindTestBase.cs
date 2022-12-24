// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

namespace Test
{
    public abstract class NorthwindTestBase : QueryTestBase
    {
        protected Northwind db;

        public NorthwindTestBase()
        {
            this.db = new Northwind(this.GetProvider());
        }

    }
}