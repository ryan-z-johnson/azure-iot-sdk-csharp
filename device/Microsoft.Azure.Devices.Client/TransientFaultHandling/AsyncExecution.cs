//Copyright(c) Microsoft.All rights reserved.
//Microsoft would like to thank its contributors, a list
//of whom are at http://aka.ms/entlib-contributors

//Licensed under the Apache License, Version 2.0 (the "License"); you
//may not use this file except in compliance with the License. You may
//obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
//implied. See the License for the specific language governing permissions
//and limitations under the License.

// THIS FILE HAS BEEN MODIFIED FROM ITS ORIGINAL FORM.
// Change Log:
// 9/1/2017 jasminel Renamed namespace to Microsoft.Azure.Devices.Client.TransientFaultHandling.

namespace Microsoft.Azure.Devices.Client.TransientFaultHandling
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client.TransientFaultHandling.Properties;

    /// <summary>
    /// Provides a wrapper for a non-generic <see cref="T:System.Threading.Tasks.Task" /> and calls into the pipeline
    /// to retry only the generic version of the <see cref="T:System.Threading.Tasks.Task" />.
    /// </summary>
    internal class AsyncExecution : AsyncExecution<Task<bool>>
    {
        private static Task<bool> cachedBoolTask;

        public AsyncExecution(Func<Task> taskAction, ShouldRetry shouldRetry, Func<Exception, bool> isTransient, Action<int, Exception, TimeSpan> onRetrying, bool fastFirstRetry, CancellationToken cancellationToken) : base(() => AsyncExecution.StartAsGenericTask(taskAction, cancellationToken), shouldRetry, isTransient, onRetrying, fastFirstRetry, cancellationToken)
        {
        }

        /// <summary>
        /// Wraps the non-generic <see cref="T:System.Threading.Tasks.Task" /> into a generic <see cref="T:System.Threading.Tasks.Task" />.
        /// </summary>
        /// <param name="taskAction">The task to wrap.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that wraps the non-generic <see cref="T:System.Threading.Tasks.Task" />.</returns>
        private async static Task<Task<bool>> StartAsGenericTask(Func<Task> taskAction, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {

                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                taskCompletionSource.TrySetCanceled();
                return taskCompletionSource.Task;

            }
            Task task = taskAction();
            if (task == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.TaskCannotBeNull, new object[]
                {
                    "taskAction"
                }), "taskAction");
            }
            if (task.Status == TaskStatus.RanToCompletion)
            {
                return AsyncExecution.GetCachedTask();
            }
            if (task.Status == TaskStatus.Created)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.TaskMustBeScheduled, new object[]
                {
                    "taskAction"
                }), "taskAction");
            }
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            await task.ContinueWith(delegate (Task t)
            {
                if (t.IsFaulted)
                {
                    tcs.TrySetException(t.Exception.InnerExceptions);
                    return;
                }
                if (t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                    return;
                }
                tcs.TrySetResult(true);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }

        private static Task<bool> GetCachedTask()
        {
            if (AsyncExecution.cachedBoolTask == null)
            {
                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                taskCompletionSource.TrySetResult(true);
                AsyncExecution.cachedBoolTask = taskCompletionSource.Task;
            }
            return AsyncExecution.cachedBoolTask;
        }
    }
}
