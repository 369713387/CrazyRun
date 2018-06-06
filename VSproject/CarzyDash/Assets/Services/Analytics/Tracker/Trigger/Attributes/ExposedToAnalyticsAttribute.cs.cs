using UnityEngine;
using System;
using System.Collections.Generic;


[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class ExposedToAnalyticsAttribute : Attribute
{
    public ExposedToAnalyticsAttribute()
    {
        
    }
}



/// EXPLORATIONS INTO DYNAMIC PROXY

//public class foo
//{
//    
//    public virtual object CreateClassProxyWithTarget(Type classToProxy, Type[] additionalInterfacesToProxy, object target,
//        ProxyGenerationOptions options, object[] constructorArguments,
//        params IInterceptor[] interceptors)
//    {
//
//        var proxyType = CreateClassProxyTypeWithTarget(classToProxy, additionalInterfacesToProxy, options);
//
//        // create constructor arguments (initialized with mixin implementations, interceptors and target type constructor arguments)
//        var arguments = BuildArgumentListForClassProxyWithTarget(target, options, interceptors);
//        if (constructorArguments != null && constructorArguments.Length != 0)
//        {
//            arguments.AddRange(constructorArguments);
//        }
//        return CreateClassProxyInstance(proxyType, arguments, classToProxy, constructorArguments);
//
//    }
//
//
//    protected List<object> BuildArgumentListForClassProxyWithTarget(object target, ProxyGenerationOptions options,
//        IInterceptor[] interceptors)
//    {
//        var arguments = new List<object>();
//        arguments.Add(target);
//        arguments.AddRange(options.MixinData.Mixins);
//        arguments.Add(interceptors);
//        if (options.Selector != null)
//        {
//            arguments.Add(options.Selector);
//        }
//        return arguments;
//    }
//
//    protected object CreateClassProxyInstance(Type proxyType, List<object> proxyArguments, Type classToProxy,
//        object[] constructorArguments)
//    {
//        try
//        {
//            return Activator.CreateInstance(proxyType, proxyArguments.ToArray());
//        }
//        catch (MissingMethodException)
//        {
//            var message = new StringBuilder();
//            message.AppendFormat("Can not instantiate proxy of class: {0}.", classToProxy.FullName);
//            message.AppendLine();
//            if (constructorArguments == null || constructorArguments.Length == 0)
//            {
//                message.Append("Could not find a parameterless constructor.");
//            }
//            else
//            {
//                message.AppendLine("Could not find a constructor that would match given arguments:");
//                foreach (var argument in constructorArguments)
//                {
//                    var argumentText = argument == null ? "<null>" : argument.GetType().ToString();
//                    message.AppendLine(argumentText);
//                }
//            }
//
//            throw new InvalidProxyConstructorArgumentsException(message.ToString(),proxyType,classToProxy);
//        }
//    }
//
//
//
//    protected Type CreateClassProxyTypeWithTarget(Type classToProxy, Type[] additionalInterfacesToProxy,
//        ProxyGenerationOptions options)
//    {
//        // create proxy
//        return CreateClassProxyTypeWithTarget(classToProxy, additionalInterfacesToProxy, options);
//    }
//
//    /// <summary>
//    ///   Creates proxy object intercepting calls to virtual members of type <paramref name = "classToProxy" /> on newly created instance of that type with given <paramref
//    ///    name = "interceptors" />.
//    /// </summary>
//    /// <param name = "classToProxy">Type of class which will be proxied.</param>
//    /// <param name = "additionalInterfacesToProxy">Additional interface types. Calls to their members will be proxied as well.</param>
//    /// <param name = "target">The target object, calls to which will be intercepted.</param>
//    /// <param name = "options">The proxy generation options used to influence generated proxy type and object.</param>
//    /// <param name = "constructorArguments">Arguments of constructor of type <paramref name = "classToProxy" /> which should be used to create a new instance of that type.</param>
//    /// <param name = "interceptors">The interceptors called during the invocation of proxied methods.</param>
//    /// <returns>
//    ///   New object of type <paramref name = "classToProxy" /> proxying calls to virtual members of <paramref
//    ///    name = "classToProxy" /> and <paramref name = "additionalInterfacesToProxy" /> types.
//    /// </returns>
//    /// <exception cref = "ArgumentNullException">Thrown when given <paramref name = "classToProxy" /> object is a null reference (Nothing in Visual Basic).</exception>
//    /// <exception cref = "ArgumentNullException">Thrown when given <paramref name = "options" /> object is a null reference (Nothing in Visual Basic).</exception>
//    /// <exception cref = "ArgumentException">Thrown when given <paramref name = "classToProxy" /> or any of <paramref
//    ///    name = "additionalInterfacesToProxy" /> is a generic type definition.</exception>
//    /// <exception cref = "ArgumentException">Thrown when given <paramref name = "classToProxy" /> is not a class type.</exception>
//    /// <exception cref = "ArgumentException">Thrown when no constructor exists on type <paramref name = "classToProxy" /> with parameters matching <paramref
//    ///    name = "constructorArguments" />.</exception>
//    /// <exception cref = "TargetInvocationException">Thrown when constructor of type <paramref name = "classToProxy" /> throws an exception.</exception>
//    /// <remarks>
//    ///   This method uses <see cref = "IProxyBuilder" /> implementation to generate a proxy type.
//    ///   As such caller should expect any type of exception that given <see cref = "IProxyBuilder" /> implementation may throw.
//    /// </remarks>
//    public virtual object CreateClassProxyWithTarget(Type classToProxy, Type[] additionalInterfacesToProxy, object target,
//        ProxyGenerationOptions options, object[] constructorArguments,
//        params IInterceptor[] interceptors)
//    {
//        if (classToProxy == null)
//        {
//            throw new ArgumentNullException("classToProxy");
//        }
//        if (options == null)
//        {
//            throw new ArgumentNullException("options");
//        }
//        if (!classToProxy.GetTypeInfo().IsClass)
//        {
//            throw new ArgumentException("'classToProxy' must be a class", "classToProxy");
//        }
//
//        CheckNotGenericTypeDefinition(classToProxy, "classToProxy");
//        CheckNotGenericTypeDefinitions(additionalInterfacesToProxy, "additionalInterfacesToProxy");
//
//        var proxyType = CreateClassProxyTypeWithTarget(classToProxy, additionalInterfacesToProxy, options);
//
//        // create constructor arguments (initialized with mixin implementations, interceptors and target type constructor arguments)
//        var arguments = BuildArgumentListForClassProxyWithTarget(target, options, interceptors);
//        if (constructorArguments != null && constructorArguments.Length != 0)
//        {
//            arguments.AddRange(constructorArguments);
//        }
//        return CreateClassProxyInstance(proxyType, arguments, classToProxy, constructorArguments);
//    }
//}


