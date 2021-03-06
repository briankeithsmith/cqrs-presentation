﻿using Autofac;
using NUnit.Framework;
using FWF.Bootstrap;
using FWF.ComponentModel;
using FWF.Basketball.Bootstrap;
using FWF.Basketball.Test.Bootstrap;
using FWF.Test.Boostrap;

namespace FWF.Basketball.Test
{
    internal static class TestApplicationState
    {
        public static IContainer Container { get; set; }
    }

    [SetUpFixture]
    public class TestApplication
    {
        
        [OneTimeSetUp]
        public void Start()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<RootModule>();
            builder.RegisterModule<BasketballModule>();
            
            builder.RegisterModule<TestModule>();
            builder.RegisterModule<BasketballTestModule>();
            
            TestApplicationState.Container = builder.Build();

        }

        [OneTimeTearDown]
        public void Stop()
        {
            TestApplicationState.Container.Dispose();

        }
    }
}



