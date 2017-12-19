// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for arrays.
    /// </summary>
    public class ArrayModelBinderProvider : IModelBinderProvider
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="ArrayModelBinderProvider"/>.
        /// </summary>
        public ArrayModelBinderProvider()
            : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ArrayModelBinderProvider"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public ArrayModelBinderProvider(ILoggerFactory loggerFactory)
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

            if (context.Metadata.ModelType.IsArray)
            {
                var elementType = context.Metadata.ElementMetadata.ModelType;
                var elementBinder = context.CreateBinder(context.Metadata.ElementMetadata);

                var binderType = typeof(ArrayModelBinder<>).MakeGenericType(elementType);
                return (IModelBinder)Activator.CreateInstance(binderType, elementBinder, _loggerFactory);
            }

            return null;
        }
    }
}
