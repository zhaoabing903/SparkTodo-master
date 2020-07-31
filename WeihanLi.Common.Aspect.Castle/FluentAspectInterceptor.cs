﻿using System;
using System.Linq;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;

namespace WeihanLi.Common.Aspect.Castle
{
    public sealed class CastleFluentAspectInterceptor : global::Castle.DynamicProxy.IInterceptor
    {
        public void Intercept(global::Castle.DynamicProxy.IInvocation invocation)
        {
            var proxyMethod = invocation.GetConcreteMethod();
            var methodBase = invocation.GetConcreteMethodInvocationTarget();
            var aspectInvocation = new AspectInvocation(
                proxyMethod,
                methodBase,
                invocation.Proxy,
                invocation.InvocationTarget,
                invocation.Arguments
            );

            var hasTarget = null != invocation.InvocationTarget
                            && null != invocation.MethodInvocationTarget
                            && null != invocation.TargetType;
            if (FluentAspects.AspectOptions.NoInterceptionConfigurations
                .Any(x => x.Invoke(aspectInvocation)))
            {
                invocation.Proceed();
            }
            else
            {
                Func<IInvocation, Task> completeFunc;
                if (hasTarget)
                {
                    completeFunc = c =>
                    {
                        invocation.Proceed();
                        c.ReturnValue = invocation.ReturnValue;
                        return TaskHelper.CompletedTask;
                    };
                }
                else
                {
                    completeFunc = c => TaskHelper.CompletedTask;
                }
                AspectDelegate.InvokeWithCompleteFunc(aspectInvocation, completeFunc);
            }
        }
    }
}
