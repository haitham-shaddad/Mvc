// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for complex types.
    /// </summary>
    public class ComplexTypeModelBinderProvider : IModelBinderProvider
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="ComplexTypeModelBinderProvider"/>.
        /// </summary>
        public ComplexTypeModelBinderProvider()
            : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ComplexTypeModelBinderProvider"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public ComplexTypeModelBinderProvider(ILoggerFactory loggerFactory)
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

            if (context.Metadata.IsComplexType && !context.Metadata.IsCollectionType)
            {
                var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
                for (var i = 0; i < context.Metadata.Properties.Count; i++)
                {
                    var property = context.Metadata.Properties[i];
                    propertyBinders.Add(property, context.CreateBinder(property));
                }

                return new ComplexTypeModelBinder(propertyBinders, _loggerFactory);
            }

            return null;
        }
    }
}
