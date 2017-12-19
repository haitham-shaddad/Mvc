// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for <see cref="ICollection{T}"/>.
    /// </summary>
    public class CollectionModelBinderProvider : IModelBinderProvider
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="CollectionModelBinderProvider"/>.
        /// </summary>
        public CollectionModelBinderProvider()
            : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CollectionModelBinderProvider"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public CollectionModelBinderProvider(ILoggerFactory loggerFactory)
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

            var modelType = context.Metadata.ModelType;

            // Arrays are handled by another binder.
            if (modelType.IsArray)
            {
                return null;
            }

            // If the model type is ICollection<> then we can call its Add method, so we can always support it.
            var collectionType = ClosedGenericMatcher.ExtractGenericInterface(modelType, typeof(ICollection<>));
            if (collectionType != null)
            {
                var elementType = collectionType.GenericTypeArguments[0];
                var elementBinder = context.CreateBinder(context.MetadataProvider.GetMetadataForType(elementType));

                var binderType = typeof(CollectionModelBinder<>).MakeGenericType(collectionType.GenericTypeArguments);
                return (IModelBinder)Activator.CreateInstance(binderType, elementBinder, _loggerFactory);
            }

            // If the model type is IEnumerable<> then we need to know if we can assign a List<> to it, since
            // that's what we would create. (The cases handled here are IEnumerable<>, IReadOnlyColection<> and
            // IReadOnlyList<>).
            var enumerableType = ClosedGenericMatcher.ExtractGenericInterface(modelType, typeof(IEnumerable<>));
            if (enumerableType != null)
            {
                var listType = typeof(List<>).MakeGenericType(enumerableType.GenericTypeArguments);
                if (modelType.GetTypeInfo().IsAssignableFrom(listType.GetTypeInfo()))
                {
                    var elementType = enumerableType.GenericTypeArguments[0];
                    var elementBinder = context.CreateBinder(context.MetadataProvider.GetMetadataForType(elementType));

                    var binderType = typeof(CollectionModelBinder<>).MakeGenericType(enumerableType.GenericTypeArguments);
                    return (IModelBinder)Activator.CreateInstance(binderType, elementBinder, _loggerFactory);
                }
            }

            return null;
        }
    }
}
