// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for binding <see cref="decimal"/>, <see cref="double"/>,
    /// <see cref="float"/>, and their <see cref="Nullable{T}"/> wrappers.
    /// </summary>
    public class FloatingPointTypeModelBinderProvider : IModelBinderProvider
    {
        // SimpleTypeModelBinder uses DecimalConverter and similar. Those TypeConverters default to NumberStyles.Float.
        // Internal for testing.
        internal static readonly NumberStyles SupportedStyles = NumberStyles.Float | NumberStyles.AllowThousands;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="FloatingPointTypeModelBinderProvider"/>.
        /// </summary>
        public FloatingPointTypeModelBinderProvider()
            : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FloatingPointTypeModelBinderProvider"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public FloatingPointTypeModelBinderProvider(ILoggerFactory loggerFactory)
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

            var modelType = context.Metadata.UnderlyingOrModelType;
            if (modelType == typeof(decimal))
            {
                return new DecimalModelBinder(SupportedStyles, _loggerFactory);
            }

            if (modelType == typeof(double))
            {
                return new DoubleModelBinder(SupportedStyles, _loggerFactory);
            }

            if (modelType == typeof(float))
            {
                return new FloatModelBinder(SupportedStyles, _loggerFactory);
            }

            return null;
        }
    }
}
