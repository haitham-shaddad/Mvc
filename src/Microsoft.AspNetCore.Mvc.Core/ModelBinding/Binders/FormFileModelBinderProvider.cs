// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for <see cref="IFormFile"/>, collections
    /// of <see cref="IFormFile"/>, and <see cref="IFormFileCollection"/>.
    /// </summary>
    public class FormFileModelBinderProvider : IModelBinderProvider
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="FormFileModelBinderProvider"/>.
        /// </summary>
        public FormFileModelBinderProvider()
            : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FormFileModelBinderProvider"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public FormFileModelBinderProvider(ILoggerFactory loggerFactory)
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

            // Note: This condition needs to be kept in sync with ApiBehaviorApplicationModelProvider.
            var modelType = context.Metadata.ModelType;
            if (modelType == typeof(IFormFile) ||
                modelType == typeof(IFormFileCollection) ||
                typeof(IEnumerable<IFormFile>).IsAssignableFrom(modelType))
            {
                return new FormFileModelBinder(_loggerFactory);
            }

            return null;
        }
    }
}
