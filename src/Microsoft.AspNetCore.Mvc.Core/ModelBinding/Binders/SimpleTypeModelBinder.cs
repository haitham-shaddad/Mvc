// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    /// <summary>
    /// An <see cref="IModelBinder"/> for simple types.
    /// </summary>
    public class SimpleTypeModelBinder : IModelBinder
    {
        private readonly TypeConverter _typeConverter;
        protected readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleTypeModelBinder"/>.
        /// </summary>
        public SimpleTypeModelBinder()
            : this(NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleTypeModelBinder"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public SimpleTypeModelBinder(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleTypeModelBinder"/>.
        /// </summary>
        /// <param name="type">The type to create binder for.</param>
        public SimpleTypeModelBinder(Type type)
            : this(type, NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SimpleTypeModelBinder"/>.
        /// </summary>
        /// <param name="type">The type to create binder for.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public SimpleTypeModelBinder(Type type, ILoggerFactory loggerFactory)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            _typeConverter = TypeDescriptor.GetConverter(type);
            logger = loggerFactory.CreateLogger(GetType());
        }

        /// <inheritdoc />
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                logger.FoundNoValueOnRequest(bindingContext);

                // no entry
                return Task.CompletedTask;
            }

            logger.TryingToBindModel(bindingContext);

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            try
            {
                var value = valueProviderResult.FirstValue;

                object model;
                if (bindingContext.ModelType == typeof(string))
                {
                    // Already have a string. No further conversion required but handle ConvertEmptyStringToNull.
                    if (bindingContext.ModelMetadata.ConvertEmptyStringToNull && string.IsNullOrWhiteSpace(value))
                    {
                        model = null;
                    }
                    else
                    {
                        model = value;
                    }
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    // Other than the StringConverter, converters Trim() the value then throw if the result is empty.
                    model = null;
                }
                else
                {
                    model = _typeConverter.ConvertFrom(
                        context: null,
                        culture: valueProviderResult.Culture,
                        value: value);
                }

                CheckModel(bindingContext, valueProviderResult, model);

                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                var isFormatException = exception is FormatException;
                if (!isFormatException && exception.InnerException != null)
                {
                    // TypeConverter throws System.Exception wrapping the FormatException,
                    // so we capture the inner exception.
                    exception = ExceptionDispatchInfo.Capture(exception.InnerException).SourceException;
                }

                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName,
                    exception,
                    bindingContext.ModelMetadata);

                // Were able to find a converter for the type but conversion failed.
                return Task.CompletedTask;
            }
        }

        protected virtual void CheckModel(
            ModelBindingContext bindingContext,
            ValueProviderResult valueProviderResult,
            object model)
        {
            // When converting newModel a null value may indicate a failed conversion for an otherwise required
            // model (can't set a ValueType to null). This detects if a null model value is acceptable given the
            // current bindingContext. If not, an error is logged.
            if (model == null && !bindingContext.ModelMetadata.IsReferenceOrNullableType)
            {
                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName,
                    bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(
                        valueProviderResult.ToString()));
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(model);
            }
        }
    }
}
