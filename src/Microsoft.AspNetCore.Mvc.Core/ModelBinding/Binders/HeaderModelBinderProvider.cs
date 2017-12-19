// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for binding header values.
    /// </summary>
    public class HeaderModelBinderProvider : IModelBinderProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="HeaderModelBinderProvider"/>.
        /// </summary>
        public HeaderModelBinderProvider()
            : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HeaderModelBinderProvider"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public HeaderModelBinderProvider(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger(GetType());
        }

        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.BindingInfo.BindingSource != null &&
                context.BindingInfo.BindingSource.CanAcceptDataFrom(BindingSource.Header))
            {
                // We only support strings and collections of strings. Some cases can fail
                // at runtime due to collections we can't modify.
                if (context.Metadata.ModelType == typeof(string) ||
                    context.Metadata.ElementType == typeof(string))
                {
                    return new HeaderModelBinder(_loggerFactory);
                }
                else
                {
                    _logger.CannotCreateHeaderModelBinder(context.Metadata.ModelType);
                }
            }

            return null;
        }
    }
}
