﻿using Polly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeepSound.Helpers.Controller
{
    public static class PollyController
    {
        public static void RunRetryPolicyFunction(List<Func<Task>> actionList, int retryCount = 4, int everySecond = 1)
        {
            var retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(retryCount, i => TimeSpan.FromSeconds(everySecond));
            foreach (var action in actionList)
                retryPolicy.ExecuteAsync(action);
        }
    }
}