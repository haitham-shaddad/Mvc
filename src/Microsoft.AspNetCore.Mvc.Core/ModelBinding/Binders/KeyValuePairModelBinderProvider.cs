// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for <see cref="KeyValuePair{TKey, TValue}"/>.
    /// </summary>
    public class KeyValuePairModelBinderProvider : IModelBinderProvider
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="KeyValuePairModelBinderProvider"/>.
        /// </summary>
        public KeyValuePairModelBinderProvider()
            : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="KeyValuePairModelBinderProvider"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public KeyValuePairModelBinderProvider(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var modelTypeInfo = context.Metadata.ModelType.GetTypeInfo();
            if (modelTypeInfo.IsGenericType &&
                modelTypeInfo.GetGenericTypeDefinition().GetTypeInfo() == typeof(KeyValuePair<,>).GetTypeInfo())
            {
                var typeArguments = modelTypeInfo.GenericTypeArguments;

                var keyMetadata = context.MetadataProvider.GetMetadataForType(typeArguments[0]);
                var keyBinder = context.CreateBinder(keyMetadata);

                var valueMetadata = context.MetadataProvider.GetMetadataForType(typeArguments[1]);
                var valueBinder = context.CreateBinder(valueMetadata);

                var binderType = typeof(KeyValuePairModelBinder<,>).MakeGenericType(typeArguments);
                return (IModelBinder)Activator.CreateInstance(binderType, keyBinder, valueBinder, _loggerFactory);
            }

            return null;
        }
    }
}
