// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

namespace Test
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ExcludeProvider : Attribute
    {
        public string Provider { get; set; }

        public ExcludeProvider(string provider)
        {
            this.Provider = provider;
        }
    }
}